using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;
using NotificationPriority = RateTheWork.Application.Common.Interfaces.NotificationPriority;

namespace RateTheWork.Infrastructure.Services;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly FirebaseApp _firebaseApp;
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService
    (
        IConfiguration configuration
        , ILogger<FirebasePushNotificationService> logger
        , ICacheService cacheService
    )
    {
        _logger = logger;
        _configuration = configuration;
        _cacheService = cacheService;

        var serviceAccountJson = configuration["FIREBASE_SERVICE_ACCOUNT_JSON"] ??
                                 throw new InvalidOperationException("FIREBASE_SERVICE_ACCOUNT_JSON not configured");

        if (FirebaseApp.DefaultInstance == null)
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountJson)
            });
        }
        else
        {
            _firebaseApp = FirebaseApp.DefaultInstance;
        }
    }

    public async Task<bool> SendAsync
        (string userId, PushNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await GetUserDeviceTokensAsync(userId, cancellationToken);
            if (!deviceTokens.Any())
            {
                _logger.LogWarning("No device tokens found for user: {UserId}", userId);
                return false;
            }

            var results = await Task.WhenAll(deviceTokens.Select(token =>
                SendToDeviceAsync(token, notification, cancellationToken)));

            return results.Any(r => r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user: {UserId}", userId);
            return false;
        }
    }

    public Task<bool> SendAsync
    (
        string userId
        , string message
        , Dictionary<string, string>? data = null
        , CancellationToken cancellationToken = default
    )
    {
        var notification = new PushNotification
        {
            Title = "RateTheWork", Body = message, Data = data ?? new Dictionary<string, string>()
        };

        return SendAsync(userId, notification, cancellationToken);
    }

    public async Task<bool> SendToDeviceAsync
        (string deviceToken, PushNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken, Notification = new Notification
                {
                    Title = notification.Title, Body = notification.Body, ImageUrl = notification.ImageUrl
                }
                , Data = notification.Data, Android = new AndroidConfig
                {
                    Priority = MapPriority(notification.Priority), Notification = new AndroidNotification
                    {
                        ClickAction = notification.ClickAction, Sound = notification.Sound ?? "default"
                    }
                }
                , Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Badge = notification.Badge, Sound = notification.Sound ?? "default"
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("Push notification sent successfully. MessageId: {MessageId}", response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to device: {DeviceToken}", deviceToken);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> SendBulkAsync
        (IEnumerable<string> userIds, PushNotification notification, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        foreach (var userId in userIds)
        {
            try
            {
                var result = await SendAsync(userId, notification, cancellationToken);
                results[userId] = result;
            }
            catch
            {
                results[userId] = false;
            }
        }

        return results;
    }

    public async Task<bool> SendToTopicAsync
        (string topic, PushNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Topic = topic, Notification = new Notification
                {
                    Title = notification.Title, Body = notification.Body, ImageUrl = notification.ImageUrl
                }
                , Data = notification.Data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("Push notification sent to topic {Topic}. MessageId: {MessageId}", topic, response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to topic: {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> RegisterDeviceTokenAsync
        (string userId, string deviceToken, DeviceType deviceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"user_device_tokens:{userId}";
            var tokens = await _cacheService.GetAsync<List<DeviceTokenInfo>>(cacheKey, cancellationToken) ??
                         new List<DeviceTokenInfo>();

            if (!tokens.Any(t => t.Token == deviceToken))
            {
                tokens.Add(new DeviceTokenInfo
                {
                    Token = deviceToken, DeviceType = deviceType, RegisteredAt = DateTime.UtcNow
                });

                await _cacheService.SetAsync(cacheKey, tokens, TimeSpan.FromDays(30), cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UnregisterDeviceTokenAsync
        (string userId, string deviceToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"user_device_tokens:{userId}";
            var tokens = await _cacheService.GetAsync<List<DeviceTokenInfo>>(cacheKey, cancellationToken) ??
                         new List<DeviceTokenInfo>();

            tokens.RemoveAll(t => t.Token == deviceToken);

            await _cacheService.SetAsync(cacheKey, tokens, TimeSpan.FromDays(30), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SubscribeToTopicAsync
        (string userId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await GetUserDeviceTokensAsync(userId, cancellationToken);
            if (!deviceTokens.Any())
            {
                return false;
            }

            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(deviceTokens, topic);
            _logger.LogInformation("Subscribed {Count} devices to topic {Topic}", response.SuccessCount, topic);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user {UserId} to topic {Topic}", userId, topic);
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromTopicAsync
        (string userId, string topic, CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await GetUserDeviceTokensAsync(userId, cancellationToken);
            if (!deviceTokens.Any())
            {
                return false;
            }

            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(deviceTokens, topic);
            _logger.LogInformation("Unsubscribed {Count} devices from topic {Topic}", response.SuccessCount, topic);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing user {UserId} from topic {Topic}", userId, topic);
            return false;
        }
    }

    private async Task<List<string>> GetUserDeviceTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"user_device_tokens:{userId}";
        var tokens = await _cacheService.GetAsync<List<DeviceTokenInfo>>(cacheKey, cancellationToken) ??
                     new List<DeviceTokenInfo>();
        return tokens.Select(t => t.Token).ToList();
    }

    private Priority MapPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Low => Priority.Normal, NotificationPriority.High => Priority.High
            , _ => Priority.Normal
        };
    }

    private class DeviceTokenInfo
    {
        public string Token { get; set; } = string.Empty;
        public DeviceType DeviceType { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}

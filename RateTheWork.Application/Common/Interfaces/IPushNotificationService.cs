namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Push notification service interface'i
/// Infrastructure katmanında FCM, APNS gibi servislerle implemente edilir.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Push notification gönderir
    /// </summary>
    Task<bool> SendAsync(string userId, PushNotification notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Basit push notification gönderir
    /// </summary>
    Task<bool> SendAsync(string userId, string message, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Device token'a göre notification gönderir
    /// </summary>
    Task<bool> SendToDeviceAsync
        (string deviceToken, PushNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toplu push notification
    /// </summary>
    Task<Dictionary<string, bool>> SendBulkAsync
        (IEnumerable<string> userIds, PushNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Topic'e notification gönderir
    /// </summary>
    Task<bool> SendToTopicAsync
        (string topic, PushNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının device token'ını kaydeder
    /// </summary>
    Task<bool> RegisterDeviceTokenAsync
        (string userId, string deviceToken, DeviceType deviceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının device token'ını siler
    /// </summary>
    Task<bool> UnregisterDeviceTokenAsync
        (string userId, string deviceToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcıyı topic'e ekler
    /// </summary>
    Task<bool> SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcıyı topic'ten çıkarır
    /// </summary>
    Task<bool> UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
}

/// <summary>
/// Push notification modeli
/// </summary>
public class PushNotification
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public string? ClickAction { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Default;
    public string? Sound { get; set; }
    public int? Badge { get; set; }
}

/// <summary>
/// Bildirim önceliği
/// </summary>
public enum NotificationPriority
{
    Low
    , Default
    , High
}

/// <summary>
/// Cihaz tipi
/// </summary>
public enum DeviceType
{
    iOS
    , Android
    , Web
}

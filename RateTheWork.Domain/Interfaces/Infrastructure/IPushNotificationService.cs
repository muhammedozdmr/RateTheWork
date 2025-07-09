namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Push notification service interface'i
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Push notification gönderir
    /// </summary>
    Task<bool> SendAsync(string userId, PushNotification notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Toplu push notification
    /// </summary>
    Task<Dictionary<string, bool>> SendBulkAsync(IEnumerable<string> userIds, PushNotification notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Topic'e notification gönderir
    /// </summary>
    Task<bool> SendToTopicAsync(string topic, PushNotification notification, CancellationToken cancellationToken = default);
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
}
namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Bildirim gönderimi başarısız exception'ı
/// </summary>
public class NotificationDeliveryException : DomainException
{
    public string NotificationType { get; }
    public string Channel { get; }
    public string RecipientId { get; }
    public int RetryCount { get; }

    public NotificationDeliveryException(string notificationType, string channel, string recipientId, int retryCount)
        : base($"Failed to deliver {notificationType} notification via {channel} after {retryCount} attempts.")
    {
        NotificationType = notificationType;
        Channel = channel;
        RecipientId = recipientId;
        RetryCount = retryCount;
    }
}
namespace RateTheWork.Domain.Events.Notification;

/// <summary>
/// 1. Bildirim oluşturuldu event'i
/// </summary>
public record NotificationCreatedEvent(
    string? NotificationId,
    string UserId,
    string Type,
    string Title,
    string Priority,
    DateTime CreatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Bildirim okundu event'i
/// </summary>
public record NotificationReadEvent(
    string? NotificationId,
    string UserId,
    DateTime ReadAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Toplu bildirim gönderildi event'i
/// </summary>
public record BulkNotificationSentEvent(
    string[] UserIds,
    string Type,
    string Title,
    int TotalCount,
    DateTime SentAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

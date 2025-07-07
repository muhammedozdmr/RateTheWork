using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Bildirim oluşturuldu event'i
/// </summary>
public record NotificationCreatedEvent(
    string? NotificationId,
    string UserId,
    string NotificationType,
    Notification.NotificationPriority Priority,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Bildirim okundu event'i
/// </summary>
public record NotificationReadEvent(
    string? NotificationId,
    string? UserId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Bildirim silindi event'i
/// </summary>
public record NotificationDeletedEvent(
    string? NotificationId,
    string? UserId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

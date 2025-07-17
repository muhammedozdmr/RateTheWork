using RateTheWork.Domain.Enums.Notification;

namespace RateTheWork.Domain.Events.Notification;

/// <summary>
/// 1. Bildirim oluşturuldu event'i
/// </summary>
public record NotificationCreatedEvent(
    string? NotificationId
    , string UserId
    , string Type
    , string Title
    , string Priority
    , DateTime CreatedAt
) : DomainEventBase;

/// <summary>
/// 2. Bildirim okundu event'i
/// </summary>
public record NotificationReadEvent(
    string? NotificationId
    , string UserId
    , DateTime ReadAt
) : DomainEventBase;

/// <summary>
/// 3. Toplu bildirim gönderildi event'i
/// </summary>
public record BulkNotificationSentEvent(
    string[] UserIds
    , string Type
    , string Title
    , int TotalCount
    , DateTime SentAt
) : DomainEventBase;

/// <summary>
/// 4. Bildirim gönderildi event'i
/// </summary>
public record NotificationSentEvent(
    string NotificationId
    , string UserId
    , NotificationChannel Channel
    , DateTime SentAt
) : DomainEventBase;

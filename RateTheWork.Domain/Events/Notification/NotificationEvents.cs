using RateTheWork.Domain.Enums.Notification;

namespace RateTheWork.Domain.Events.Notification;

/// <summary>
/// 1. Bildirim oluşturuldu event'i
/// </summary>
public class NotificationCreatedEvent : DomainEventBase
{
    public NotificationCreatedEvent
    (
        string? notificationId
        , string userId
        , string type
        , string title
        , string priority
        , DateTime createdAt
    ) : base()
    {
        NotificationId = notificationId;
        UserId = userId;
        Type = type;
        Title = title;
        Priority = priority;
        CreatedAt = createdAt;
    }

    public string? NotificationId { get; }
    public string UserId { get; }
    public string Type { get; }
    public string Title { get; }
    public string Priority { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Bildirim okundu event'i
/// </summary>
public class NotificationReadEvent : DomainEventBase
{
    public NotificationReadEvent
    (
        string? notificationId
        , string userId
        , DateTime readAt
    ) : base()
    {
        NotificationId = notificationId;
        UserId = userId;
        ReadAt = readAt;
    }

    public string? NotificationId { get; }
    public string UserId { get; }
    public DateTime ReadAt { get; }
}

/// <summary>
/// 3. Toplu bildirim gönderildi event'i
/// </summary>
public class BulkNotificationSentEvent : DomainEventBase
{
    public BulkNotificationSentEvent
    (
        string[] userIds
        , string type
        , string title
        , int totalCount
        , DateTime sentAt
    ) : base()
    {
        UserIds = userIds;
        Type = type;
        Title = title;
        TotalCount = totalCount;
        SentAt = sentAt;
    }

    public string[] UserIds { get; }
    public string Type { get; }
    public string Title { get; }
    public int TotalCount { get; }
    public DateTime SentAt { get; }
}

/// <summary>
/// 4. Bildirim gönderildi event'i
/// </summary>
public class NotificationSentEvent : DomainEventBase
{
    public NotificationSentEvent
    (
        string notificationId
        , string userId
        , NotificationChannel channel
        , DateTime sentAt
    ) : base()
    {
        NotificationId = notificationId;
        UserId = userId;
        Channel = channel;
        SentAt = sentAt;
    }

    public string NotificationId { get; }
    public string UserId { get; }
    public NotificationChannel Channel { get; }
    public DateTime SentAt { get; }
}

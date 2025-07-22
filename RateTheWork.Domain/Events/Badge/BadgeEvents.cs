namespace RateTheWork.Domain.Events.Badge;

/// <summary>
/// 1. Rozet oluşturuldu event'i
/// </summary>
public class BadgeCreatedEvent : DomainEventBase
{
    public BadgeCreatedEvent
    (
        string? badgeId
        , string name
        , string type
        , string rarity
        , DateTime createdAt
    ) : base()
    {
        BadgeId = badgeId;
        Name = name;
        Type = type;
        Rarity = rarity;
        CreatedAt = createdAt;
    }

    public string? BadgeId { get; }
    public string Name { get; }
    public string Type { get; }
    public string Rarity { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Rozet aktifleştirildi event'i
/// </summary>
public class BadgeActivatedEvent : DomainEventBase
{
    public BadgeActivatedEvent
    (
        string? badgeId
        , string badgeName
        , string activatedBy
        , DateTime activatedAt
    ) : base()
    {
        BadgeId = badgeId;
        BadgeName = badgeName;
        ActivatedBy = activatedBy;
        ActivatedAt = activatedAt;
    }

    public string? BadgeId { get; }
    public string BadgeName { get; }
    public string ActivatedBy { get; }
    public DateTime ActivatedAt { get; }
}

/// <summary>
/// 3. Rozet deaktive edildi event'i
/// </summary>
public class BadgeDeactivatedEvent : DomainEventBase
{
    public BadgeDeactivatedEvent
    (
        string? badgeId
        , string badgeName
        , string deactivatedBy
        , string reason
        , DateTime deactivatedAt
    ) : base()
    {
        BadgeId = badgeId;
        BadgeName = badgeName;
        DeactivatedBy = deactivatedBy;
        Reason = reason;
        DeactivatedAt = deactivatedAt;
    }

    public string? BadgeId { get; }
    public string BadgeName { get; }
    public string DeactivatedBy { get; }
    public string Reason { get; }
    public DateTime DeactivatedAt { get; }
}

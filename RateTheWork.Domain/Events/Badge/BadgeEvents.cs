namespace RateTheWork.Domain.Events.Badge;

/// <summary>
/// 1. Rozet oluşturuldu event'i
/// </summary>
public record BadgeCreatedEvent(
    string? BadgeId
    , string Name
    , string Type
    , string Rarity
    , DateTime CreatedAt
) : DomainEventBase;

/// <summary>
/// 2. Rozet aktifleştirildi event'i
/// </summary>
public record BadgeActivatedEvent(
    string? BadgeId
    , string BadgeName
    , string ActivatedBy
    , DateTime ActivatedAt
) : DomainEventBase;

/// <summary>
/// 3. Rozet deaktive edildi event'i
/// </summary>
public record BadgeDeactivatedEvent(
    string? BadgeId
    , string BadgeName
    , string DeactivatedBy
    , string Reason
    , DateTime DeactivatedAt
) : DomainEventBase;

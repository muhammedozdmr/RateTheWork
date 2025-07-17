namespace RateTheWork.Domain.Events.UserBadge;

/// <summary>
/// 1. Kullanıcıya rozet verildi event'i
/// </summary>
public record BadgeAwardedEvent(
    string? UserBadgeId
    , string UserId
    , string BadgeId
    , DateTime AwardedAt
    , string? AwardReason
    , string? SpecialNote
) : DomainEventBase;

/// <summary>
/// 2. Kullanıcı rozeti görüntüledi event'i
/// </summary>
public record BadgeViewedEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , DateTime ViewedAt
) : DomainEventBase;

/// <summary>
/// 3. Kullanıcı rozeti gösterildi event'i
/// </summary>
public record BadgeDisplayedEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , DateTime DisplayedAt
) : DomainEventBase;

/// <summary>
/// 4. Kullanıcı rozeti gizlendi event'i
/// </summary>
public record BadgeHiddenEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , DateTime HiddenAt
) : DomainEventBase;

/// <summary>
/// 5. Otomatik rozet kazanıldı event'i
/// </summary>
public record AutomaticBadgeAwardedEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , string TriggerCondition
    , Dictionary<string, object>? Metadata
) : DomainEventBase;

/// <summary>
/// 6. Sezonluk rozet kazanıldı event'i
/// </summary>
public record SeasonalBadgeAwardedEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , string SeasonName
    , DateTime SeasonStart
    , DateTime SeasonEnd
) : DomainEventBase;

/// <summary>
/// 7. Manuel rozet verildi event'i
/// </summary>
public record ManualBadgeAwardedEvent(
    string? UserBadgeId
    , string? UserId
    , string? BadgeId
    , string AdminId
    , string Reason
) : DomainEventBase;

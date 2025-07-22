namespace RateTheWork.Domain.Events.UserBadge;

/// <summary>
/// 1. Kullanıcıya rozet verildi event'i
/// </summary>
public class BadgeAwardedEvent : DomainEventBase
{
    public BadgeAwardedEvent
    (
        string? userBadgeId
        , string userId
        , string badgeId
        , DateTime awardedAt
        , string? awardReason
        , string? specialNote
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        AwardedAt = awardedAt;
        AwardReason = awardReason;
        SpecialNote = specialNote;
    }

    public string? UserBadgeId { get; }
    public string UserId { get; }
    public string BadgeId { get; }
    public DateTime AwardedAt { get; }
    public string? AwardReason { get; }
    public string? SpecialNote { get; }
}

/// <summary>
/// 2. Kullanıcı rozeti görüntüledi event'i
/// </summary>
public class BadgeViewedEvent : DomainEventBase
{
    public BadgeViewedEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , DateTime viewedAt
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        ViewedAt = viewedAt;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public DateTime ViewedAt { get; }
}

/// <summary>
/// 3. Kullanıcı rozeti gösterildi event'i
/// </summary>
public class BadgeDisplayedEvent : DomainEventBase
{
    public BadgeDisplayedEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , DateTime displayedAt
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        DisplayedAt = displayedAt;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public DateTime DisplayedAt { get; }
}

/// <summary>
/// 4. Kullanıcı rozeti gizlendi event'i
/// </summary>
public class BadgeHiddenEvent : DomainEventBase
{
    public BadgeHiddenEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , DateTime hiddenAt
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        HiddenAt = hiddenAt;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public DateTime HiddenAt { get; }
}

/// <summary>
/// 5. Otomatik rozet kazanıldı event'i
/// </summary>
public class AutomaticBadgeAwardedEvent : DomainEventBase
{
    public AutomaticBadgeAwardedEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , string triggerCondition
        , Dictionary<string, object>? metadata
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        TriggerCondition = triggerCondition;
        Metadata = metadata;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public string TriggerCondition { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// 6. Sezonluk rozet kazanıldı event'i
/// </summary>
public class SeasonalBadgeAwardedEvent : DomainEventBase
{
    public SeasonalBadgeAwardedEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , string seasonName
        , DateTime seasonStart
        , DateTime seasonEnd
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        SeasonName = seasonName;
        SeasonStart = seasonStart;
        SeasonEnd = seasonEnd;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public string SeasonName { get; }
    public DateTime SeasonStart { get; }
    public DateTime SeasonEnd { get; }
}

/// <summary>
/// 7. Manuel rozet verildi event'i
/// </summary>
public class ManualBadgeAwardedEvent : DomainEventBase
{
    public ManualBadgeAwardedEvent
    (
        string? userBadgeId
        , string? userId
        , string? badgeId
        , string adminId
        , string reason
    ) : base()
    {
        UserBadgeId = userBadgeId;
        UserId = userId;
        BadgeId = badgeId;
        AdminId = adminId;
        Reason = reason;
    }

    public string? UserBadgeId { get; }
    public string? UserId { get; }
    public string? BadgeId { get; }
    public string AdminId { get; }
    public string Reason { get; }
}

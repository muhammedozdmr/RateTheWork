using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Events.Ban;

/// <summary>
/// 1. Kullanıcı banlandı event'i
/// </summary>
public class UserBannedEvent : DomainEventBase
{
    public UserBannedEvent
    (
        string? banId
        , string userId
        , string adminId
        , string reason
        , string banType
        , DateTime? unbanDate
        , bool isAppealable
        , DateTime bannedAt
    ) : base()
    {
        BanId = banId;
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
        BanType = banType;
        UnbanDate = unbanDate;
        IsAppealable = isAppealable;
        BannedAt = bannedAt;
    }

    public string? BanId { get; }
    public string UserId { get; }
    public string AdminId { get; }
    public string Reason { get; }
    public string BanType { get; }
    public DateTime? UnbanDate { get; }
    public bool IsAppealable { get; }
    public DateTime BannedAt { get; }
}

/// <summary>
/// 2. Ban kaldırıldı event'i
/// </summary>
public class UserUnbannedEvent : DomainEventBase
{
    public UserUnbannedEvent
    (
        string? banId
        , string userId
        , string liftedBy
        , string liftReason
        , DateTime liftedAt
    ) : base()
    {
        BanId = banId;
        UserId = userId;
        LiftedBy = liftedBy;
        LiftReason = liftReason;
        LiftedAt = liftedAt;
    }

    public string? BanId { get; }
    public string UserId { get; }
    public string LiftedBy { get; }
    public string LiftReason { get; }
    public DateTime LiftedAt { get; }
}

/// <summary>
/// 3. Ban'a itiraz edildi event'i
/// </summary>
public class BanAppealedEvent : DomainEventBase
{
    public BanAppealedEvent
    (
        string? banId
        , string userId
        , string appealNotes
        , DateTime appealedAt
    ) : base()
    {
        BanId = banId;
        UserId = userId;
        AppealNotes = appealNotes;
        AppealedAt = appealedAt;
    }

    public string? BanId { get; }
    public string UserId { get; }
    public string AppealNotes { get; }
    public DateTime AppealedAt { get; }
}

/// <summary>
/// 4. Otomatik ban oluştu event'i
/// </summary>
public class AutoBanCreatedEvent : DomainEventBase
{
    public AutoBanCreatedEvent
    (
        string? banId
        , string userId
        , string triggerReason
        , int warningCount
        , DateTime bannedAt
    ) : base()
    {
        BanId = banId;
        UserId = userId;
        TriggerReason = triggerReason;
        WarningCount = warningCount;
        BannedAt = bannedAt;
    }

    public string? BanId { get; }
    public string UserId { get; }
    public string TriggerReason { get; }
    public int WarningCount { get; }
    public DateTime BannedAt { get; }
}

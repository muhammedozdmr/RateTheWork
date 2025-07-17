namespace RateTheWork.Domain.Events.Ban;

/// <summary>
/// 1. Kullanıcı banlandı event'i
/// </summary>
public record UserBannedEvent(
    string? BanId
    , string UserId
    , string AdminId
    , string Reason
    , string BanType
    , DateTime? UnbanDate
    , bool IsAppealable
    , DateTime BannedAt
) : DomainEventBase;

/// <summary>
/// 2. Ban kaldırıldı event'i
/// </summary>
public record UserUnbannedEvent(
    string? BanId
    , string UserId
    , string LiftedBy
    , string LiftReason
    , DateTime LiftedAt
) : DomainEventBase;

/// <summary>
/// 3. Ban'a itiraz edildi event'i
/// </summary>
public record BanAppealedEvent(
    string? BanId
    , string UserId
    , string AppealNotes
    , DateTime AppealedAt
) : DomainEventBase;

/// <summary>
/// 4. Otomatik ban oluştu event'i
/// </summary>
public record AutoBanCreatedEvent(
    string? BanId
    , string UserId
    , string TriggerReason
    , int WarningCount
    , DateTime BannedAt
) : DomainEventBase;

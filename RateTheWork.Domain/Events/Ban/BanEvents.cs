namespace RateTheWork.Domain.Events.Ban;

/// <summary>
/// 1. Kullanıcı banlandı event'i
/// </summary>
public record UserBannedEvent(
    string? BanId,
    string UserId,
    string AdminId,
    string Reason,
    string BanType,
    DateTime? UnbanDate,
    bool IsAppealable,
    DateTime BannedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Ban kaldırıldı event'i
/// </summary>
public record UserUnbannedEvent(
    string? BanId,
    string UserId,
    string LiftedBy,
    string LiftReason,
    DateTime LiftedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Ban'a itiraz edildi event'i
/// </summary>
public record BanAppealedEvent(
    string? BanId,
    string UserId,
    string AppealNotes,
    DateTime AppealedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Otomatik ban oluştu event'i
/// </summary>
public record AutoBanCreatedEvent(
    string? BanId,
    string UserId,
    string TriggerReason,
    int WarningCount,
    DateTime BannedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

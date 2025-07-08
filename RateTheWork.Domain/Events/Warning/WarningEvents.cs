namespace RateTheWork.Domain.Events.Warning;

/// <summary>
/// 1. Kullanıcı uyarıldı event'i
/// </summary>
public record UserWarnedEvent(
    string WarningId,
    string UserId,
    string AdminId,
    string Reason,
    string WarningType,
    string Severity,
    int Points,
    int TotalWarnings,
    DateTime IssuedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Uyarı onaylandı (kullanıcı gördü) event'i
/// </summary>
public record WarningAcknowledgedEvent(
    string WarningId,
    string UserId,
    DateTime AcknowledgedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Uyarıya itiraz edildi event'i
/// </summary>
public record WarningAppealedEvent(
    string WarningId,
    string UserId,
    string AppealNotes,
    DateTime AppealedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Uyarı süresi doldu event'i
/// </summary>
public record WarningExpiredEvent(
    string WarningId,
    string UserId,
    DateTime ExpiredAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

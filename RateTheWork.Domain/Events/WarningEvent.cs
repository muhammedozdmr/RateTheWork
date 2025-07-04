using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Kullanıcı uyarıldı event'i
/// </summary>
public record UserWarnedEvent(
    string WarningId,
    string UserId,
    string AdminId,
    string Reason,
    Warning.WarningType Type,
    Warning.WarningSeverity Severity,
    int Points,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Uyarı onaylandı event'i
/// </summary>
public record WarningAcknowledgedEvent(
    string WarningId,
    string UserId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Uyarıya itiraz edildi event'i
/// </summary>
public record WarningAppealedEvent(
    string WarningId,
    string UserId,
    string AppealReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Uyarı itirazı değerlendirildi event'i
/// </summary>
public record WarningAppealReviewedEvent(
    string WarningId,
    string UserId,
    string ReviewedByAdminId,
    bool Accepted,
    string ReviewNotes,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Uyarı iptal edildi event'i
/// </summary>
public record WarningRevokedEvent(
    string WarningId,
    string UserId,
    string RevokedByAdminId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

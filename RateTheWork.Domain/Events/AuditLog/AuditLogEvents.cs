namespace RateTheWork.Domain.Events.AuditLog;

/// <summary>
/// 1. Audit log oluşturuldu event'i
/// </summary>
public record AuditLogCreatedEvent(
    string? AuditLogId,
    string AdminUserId,
    string ActionType,
    string EntityType,
    string EntityId,
    string Severity,
    DateTime PerformedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Kritik aksiyon gerçekleşti event'i
/// </summary>
public record CriticalActionPerformedEvent(
    string? AuditLogId,
    string AdminUserId,
    string ActionType,
    string EntityType,
    string EntityId,
    string Details,
    DateTime PerformedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}


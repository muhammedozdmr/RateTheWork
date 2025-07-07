namespace RateTheWork.Domain.Events;

/// <summary>
/// Şikayet oluşturuldu event'i
/// </summary>
public record ReportCreatedEvent(
    string? ReportId,
    string ReviewId,
    string ReporterUserId,
    string ReportReason,
    bool RequiresUrgentAction,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Şikayet incelemeye alındı event'i
/// </summary>
public record ReportReviewStartedEvent(
    string? ReportId,
    string? ReviewId,
    string AdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Şikayet çözümlendi event'i
/// </summary>
public record ReportResolvedEvent(
    string? ReportId,
    string? ReviewId,
    string AdminId,
    string ActionTaken,
    string? ReportReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Şikayet reddedildi event'i
/// </summary>
public record ReportDismissedEvent(
    string? ReportId,
    string? ReviewId,
    string AdminId,
    string DismissReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Şikayet üst yönetime iletildi event'i
/// </summary>
public record ReportEscalatedEvent(
    string? ReportId,
    string? ReviewId,
    string AdminId,
    string EscalationReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

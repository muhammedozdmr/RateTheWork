namespace RateTheWork.Domain.Events;

/// <summary>
/// Yorum eklendi event'i
/// </summary>
public record ReviewCreatedEvent(
    string? ReviewId,
    string UserId,
    string CompanyId,
    decimal Rating,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum düzenlendi event'i
/// </summary>
public record ReviewEditedEvent(
    string? ReviewId,
    string EditorUserId,
    string EditReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum şikayet edildi event'i
/// </summary>
public record ReviewReportedEvent(
    string? ReviewId,
    string ReporterUserId,
    string ReportReason,
    int TotalReportCount,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum gizlendi event'i
/// </summary>
public record ReviewHiddenEvent(
    string? ReviewId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum tekrar aktif edildi event'i
/// </summary>
public record ReviewActivatedEvent(
    string? ReviewId,
    string AdminId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yoruma belge eklendi event'i
/// </summary>
public record ReviewDocumentAttachedEvent(
    string? ReviewId,
    string DocumentUrl,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Yorum belgesi doğrulandı event'i
/// </summary>
public record ReviewDocumentVerifiedEvent(
    string? ReviewId,
    string AdminId,
    string DocumentUrl,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
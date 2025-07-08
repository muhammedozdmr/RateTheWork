namespace RateTheWork.Domain.Events.VerificationRequest;

/// <summary>
/// 1. Doğrulama talebi oluşturuldu event'i
/// </summary>
public record VerificationRequestCreatedEvent(
    string? RequestId,
    string UserId,
    string ReviewId,
    string DocumentType,
    string VerificationType,
    DateTime RequestedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Doğrulama talebi işleme alındı event'i
/// </summary>
public record VerificationRequestProcessingStartedEvent(
    string? RequestId,
    string AdminId,
    DateTime StartedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Doğrulama talebi onaylandı event'i
/// </summary>
public record VerificationRequestApprovedEvent(
    string? RequestId,
    string UserId,
    string ReviewId,
    string ApprovedByAdminId,
    string DocumentType,
    int ProcessingTimeHours,
    DateTime ApprovedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Doğrulama talebi reddedildi event'i
/// </summary>
public record VerificationRequestRejectedEvent(
    string? RequestId,
    string UserId,
    string ReviewId,
    string RejectedByAdminId,
    string RejectionReason,
    bool AllowResubmission,
    DateTime RejectedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Doğrulama talebi yeniden gönderildi event'i
/// </summary>
public record VerificationRequestResubmittedEvent(
    string? RequestId,
    string UserId,
    string ReviewId,
    string NewDocumentUrl,
    DateTime ResubmittedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Doğrulama talebi acil işaretlendi event'i
/// </summary>
public record VerificationRequestMarkedUrgentEvent(
    string? RequestId,
    string MarkedByAdminId,
    string Reason,
    DateTime MarkedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

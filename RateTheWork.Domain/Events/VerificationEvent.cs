namespace RateTheWork.Domain.Events;

/// <summary>
/// Doğrulama talebi oluşturuldu event'i
/// </summary>
public record VerificationRequestCreatedEvent(
    string RequestId,
    string UserId,
    string ReviewId,
    string DocumentType,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Doğrulama talebi işleme alındı event'i
/// </summary>
public record VerificationRequestProcessingStartedEvent(
    string RequestId,
    string AdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Doğrulama talebi onaylandı event'i
/// </summary>
public record VerificationRequestApprovedEvent(
    string RequestId,
    string UserId,
    string ReviewId,
    string ApprovedByAdminId,
    string DocumentType,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Doğrulama talebi reddedildi event'i
/// </summary>
public record VerificationRequestRejectedEvent(
    string RequestId,
    string UserId,
    string ReviewId,
    string RejectedByAdminId,
    string RejectionReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Doğrulama talebi yeniden gönderildi event'i
/// </summary>
public record VerificationRequestResubmittedEvent(
    string RequestId,
    string UserId,
    string ReviewId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Doğrulama talebi acil işaretlendi event'i
/// </summary>
public record VerificationRequestMarkedUrgentEvent(
    string RequestId,
    string MarkedByAdminId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

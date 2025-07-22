namespace RateTheWork.Domain.Events.VerificationRequest;

/// <summary>
/// 1. Doğrulama talebi oluşturuldu event'i
/// </summary>
public class VerificationRequestCreatedEvent : DomainEventBase
{
    public VerificationRequestCreatedEvent
    (
        string? requestId
        , string userId
        , string reviewId
        , string documentType
        , string verificationType
        , DateTime requestedAt
    ) : base()
    {
        RequestId = requestId;
        UserId = userId;
        ReviewId = reviewId;
        DocumentType = documentType;
        VerificationType = verificationType;
        RequestedAt = requestedAt;
    }

    public string? RequestId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public string DocumentType { get; }
    public string VerificationType { get; }
    public DateTime RequestedAt { get; }
}

/// <summary>
/// 2. Doğrulama talebi işleme alındı event'i
/// </summary>
public class VerificationRequestProcessingStartedEvent : DomainEventBase
{
    public VerificationRequestProcessingStartedEvent
    (
        string? requestId
        , string adminId
        , DateTime startedAt
    ) : base()
    {
        RequestId = requestId;
        AdminId = adminId;
        StartedAt = startedAt;
    }

    public string? RequestId { get; }
    public string AdminId { get; }
    public DateTime StartedAt { get; }
}

/// <summary>
/// 3. Doğrulama talebi onaylandı event'i
/// </summary>
public class VerificationRequestApprovedEvent : DomainEventBase
{
    public VerificationRequestApprovedEvent
    (
        string? requestId
        , string userId
        , string reviewId
        , string approvedByAdminId
        , string documentType
        , int processingTimeHours
        , DateTime approvedAt
    ) : base()
    {
        RequestId = requestId;
        UserId = userId;
        ReviewId = reviewId;
        ApprovedByAdminId = approvedByAdminId;
        DocumentType = documentType;
        ProcessingTimeHours = processingTimeHours;
        ApprovedAt = approvedAt;
    }

    public string? RequestId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public string ApprovedByAdminId { get; }
    public string DocumentType { get; }
    public int ProcessingTimeHours { get; }
    public DateTime ApprovedAt { get; }
}

/// <summary>
/// 4. Doğrulama talebi reddedildi event'i
/// </summary>
public class VerificationRequestRejectedEvent : DomainEventBase
{
    public VerificationRequestRejectedEvent
    (
        string? requestId
        , string userId
        , string reviewId
        , string rejectedByAdminId
        , string rejectionReason
        , bool allowResubmission
        , DateTime rejectedAt
    ) : base()
    {
        RequestId = requestId;
        UserId = userId;
        ReviewId = reviewId;
        RejectedByAdminId = rejectedByAdminId;
        RejectionReason = rejectionReason;
        AllowResubmission = allowResubmission;
        RejectedAt = rejectedAt;
    }

    public string? RequestId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public string RejectedByAdminId { get; }
    public string RejectionReason { get; }
    public bool AllowResubmission { get; }
    public DateTime RejectedAt { get; }
}

/// <summary>
/// 5. Doğrulama talebi yeniden gönderildi event'i
/// </summary>
public class VerificationRequestResubmittedEvent : DomainEventBase
{
    public VerificationRequestResubmittedEvent
    (
        string? requestId
        , string userId
        , string reviewId
        , string newDocumentUrl
        , DateTime resubmittedAt
    ) : base()
    {
        RequestId = requestId;
        UserId = userId;
        ReviewId = reviewId;
        NewDocumentUrl = newDocumentUrl;
        ResubmittedAt = resubmittedAt;
    }

    public string? RequestId { get; }
    public string UserId { get; }
    public string ReviewId { get; }
    public string NewDocumentUrl { get; }
    public DateTime ResubmittedAt { get; }
}

/// <summary>
/// 6. Doğrulama talebi acil işaretlendi event'i
/// </summary>
public class VerificationRequestMarkedUrgentEvent : DomainEventBase
{
    public VerificationRequestMarkedUrgentEvent
    (
        string? requestId
        , string markedByAdminId
        , string reason
        , DateTime markedAt
    ) : base()
    {
        RequestId = requestId;
        MarkedByAdminId = markedByAdminId;
        Reason = reason;
        MarkedAt = markedAt;
    }

    public string? RequestId { get; }
    public string MarkedByAdminId { get; }
    public string Reason { get; }
    public DateTime MarkedAt { get; }
}

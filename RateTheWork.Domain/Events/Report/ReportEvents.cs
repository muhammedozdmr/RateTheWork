namespace RateTheWork.Domain.Events.Report;

/// <summary>
/// 1. Şikayet oluşturuldu event'i
/// </summary>
public class ReportCreatedEvent : DomainEventBase
{
    public ReportCreatedEvent
    (
        string? reportId
        , string reviewId
        , string reporterUserId
        , string reportReason
        , bool isAnonymous
        , int priority
        , DateTime reportedAt
    ) : base()
    {
        ReportId = reportId;
        ReviewId = reviewId;
        ReporterUserId = reporterUserId;
        ReportReason = reportReason;
        IsAnonymous = isAnonymous;
        Priority = priority;
        ReportedAt = reportedAt;
    }

    public string? ReportId { get; }
    public string ReviewId { get; }
    public string ReporterUserId { get; }
    public string ReportReason { get; }
    public bool IsAnonymous { get; }
    public int Priority { get; }
    public DateTime ReportedAt { get; }
}

/// <summary>
/// 2. Şikayet incelemeye alındı event'i
/// </summary>
public class ReportUnderReviewEvent : DomainEventBase
{
    public ReportUnderReviewEvent
    (
        string? reportId
        , string reviewedBy
        , DateTime startedAt
    ) : base()
    {
        ReportId = reportId;
        ReviewedBy = reviewedBy;
        StartedAt = startedAt;
    }

    public string? ReportId { get; }
    public string ReviewedBy { get; }
    public DateTime StartedAt { get; }
}

/// <summary>
/// 3. Şikayet çözümlendi event'i
/// </summary>
public class ReportResolvedEvent : DomainEventBase
{
    public ReportResolvedEvent
    (
        string? reportId
        , string resolvedBy
        , string actionTaken
        , DateTime resolvedAt
    ) : base()
    {
        ReportId = reportId;
        ResolvedBy = resolvedBy;
        ActionTaken = actionTaken;
        ResolvedAt = resolvedAt;
    }

    public string? ReportId { get; }
    public string ResolvedBy { get; }
    public string ActionTaken { get; }
    public DateTime ResolvedAt { get; }
}

/// <summary>
/// 4. Şikayet reddedildi event'i
/// </summary>
public class ReportDismissedEvent : DomainEventBase
{
    public ReportDismissedEvent
    (
        string? reportId
        , string dismissedBy
        , string dismissReason
        , DateTime dismissedAt
    ) : base()
    {
        ReportId = reportId;
        DismissedBy = dismissedBy;
        DismissReason = dismissReason;
        DismissedAt = dismissedAt;
    }

    public string? ReportId { get; }
    public string DismissedBy { get; }
    public string DismissReason { get; }
    public DateTime DismissedAt { get; }
}

/// <summary>
/// 5. Şikayet üst yönetime iletildi event'i
/// </summary>
public class ReportEscalatedEvent : DomainEventBase
{
    public ReportEscalatedEvent
    (
        string? reportId
        , string escalatedBy
        , string escalationReason
        , DateTime escalatedAt
    ) : base()
    {
        ReportId = reportId;
        EscalatedBy = escalatedBy;
        EscalationReason = escalationReason;
        EscalatedAt = escalatedAt;
    }

    public string? ReportId { get; }
    public string EscalatedBy { get; }
    public string EscalationReason { get; }
    public DateTime EscalatedAt { get; }
}

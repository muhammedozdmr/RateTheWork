namespace RateTheWork.Domain.Events.Report;

/// <summary>
/// 1. Şikayet oluşturuldu event'i
/// </summary>
public record ReportCreatedEvent(
    string? ReportId
    , string ReviewId
    , string ReporterUserId
    , string ReportReason
    , bool IsAnonymous
    , int Priority
    , DateTime ReportedAt
) : DomainEventBase;

/// <summary>
/// 2. Şikayet incelemeye alındı event'i
/// </summary>
public record ReportUnderReviewEvent(
    string? ReportId
    , string ReviewedBy
    , DateTime StartedAt
) : DomainEventBase;

/// <summary>
/// 3. Şikayet çözümlendi event'i
/// </summary>
public record ReportResolvedEvent(
    string? ReportId
    , string ResolvedBy
    , string ActionTaken
    , DateTime ResolvedAt
) : DomainEventBase;

/// <summary>
/// 4. Şikayet reddedildi event'i
/// </summary>
public record ReportDismissedEvent(
    string? ReportId
    , string DismissedBy
    , string DismissReason
    , DateTime DismissedAt
) : DomainEventBase;

/// <summary>
/// 5. Şikayet üst yönetime iletildi event'i
/// </summary>
public record ReportEscalatedEvent(
    string? ReportId
    , string EscalatedBy
    , string EscalationReason
    , DateTime EscalatedAt
) : DomainEventBase;

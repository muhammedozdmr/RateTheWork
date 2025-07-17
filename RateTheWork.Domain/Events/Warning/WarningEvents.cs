namespace RateTheWork.Domain.Events.Warning;

/// <summary>
/// 1. Kullanıcı uyarıldı event'i
/// </summary>
public record UserWarnedEvent(
    string? WarningId
    , string UserId
    , string AdminId
    , string Reason
    , string WarningType
    , string Severity
    , int Points
    , int TotalWarnings
    , DateTime IssuedAt
) : DomainEventBase;

/// <summary>
/// 2. Uyarı onaylandı (kullanıcı gördü) event'i
/// </summary>
public record WarningAcknowledgedEvent(
    string? WarningId
    , string UserId
    , DateTime AcknowledgedAt
) : DomainEventBase;

/// <summary>
/// 3. Uyarıya itiraz edildi event'i
/// </summary>
public record WarningAppealedEvent(
    string? WarningId
    , string UserId
    , string AppealNotes
    , DateTime AppealedAt
) : DomainEventBase;

/// <summary>
/// 4. Uyarı süresi doldu event'i
/// </summary>
public record WarningExpiredEvent(
    string? WarningId
    , string UserId
    , DateTime ExpiredAt
) : DomainEventBase;

/// <summary>
/// 5. Otomatik uyarı verildi event'i
/// </summary>
public record AutomaticWarningIssuedEvent(
    string? WarningId
    , string UserId
    , string TriggerRule
    , string WarningType
    , string Severity
    , Dictionary<string, object>? Metadata
) : DomainEventBase;

namespace RateTheWork.Domain.Events.Warning;

/// <summary>
/// 1. Kullanıcı uyarıldı event'i
/// </summary>
public class UserWarnedEvent : DomainEventBase
{
    public UserWarnedEvent
    (
        string? warningId
        , string userId
        , string adminId
        , string reason
        , string warningType
        , string severity
        , int points
        , int totalWarnings
        , DateTime issuedAt
    ) : base()
    {
        WarningId = warningId;
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
        WarningType = warningType;
        Severity = severity;
        Points = points;
        TotalWarnings = totalWarnings;
        IssuedAt = issuedAt;
    }

    public string? WarningId { get; }
    public string UserId { get; }
    public string AdminId { get; }
    public string Reason { get; }
    public string WarningType { get; }
    public string Severity { get; }
    public int Points { get; }
    public int TotalWarnings { get; }
    public DateTime IssuedAt { get; }
}

/// <summary>
/// 2. Uyarı onaylandı (kullanıcı gördü) event'i
/// </summary>
public class WarningAcknowledgedEvent : DomainEventBase
{
    public WarningAcknowledgedEvent
    (
        string? warningId
        , string userId
        , DateTime acknowledgedAt
    ) : base()
    {
        WarningId = warningId;
        UserId = userId;
        AcknowledgedAt = acknowledgedAt;
    }

    public string? WarningId { get; }
    public string UserId { get; }
    public DateTime AcknowledgedAt { get; }
}

/// <summary>
/// 3. Uyarıya itiraz edildi event'i
/// </summary>
public class WarningAppealedEvent : DomainEventBase
{
    public WarningAppealedEvent
    (
        string? warningId
        , string userId
        , string appealNotes
        , DateTime appealedAt
    ) : base()
    {
        WarningId = warningId;
        UserId = userId;
        AppealNotes = appealNotes;
        AppealedAt = appealedAt;
    }

    public string? WarningId { get; }
    public string UserId { get; }
    public string AppealNotes { get; }
    public DateTime AppealedAt { get; }
}

/// <summary>
/// 4. Uyarı süresi doldu event'i
/// </summary>
public class WarningExpiredEvent : DomainEventBase
{
    public WarningExpiredEvent
    (
        string? warningId
        , string userId
        , DateTime expiredAt
    ) : base()
    {
        WarningId = warningId;
        UserId = userId;
        ExpiredAt = expiredAt;
    }

    public string? WarningId { get; }
    public string UserId { get; }
    public DateTime ExpiredAt { get; }
}

/// <summary>
/// 5. Otomatik uyarı verildi event'i
/// </summary>
public class AutomaticWarningIssuedEvent : DomainEventBase
{
    public AutomaticWarningIssuedEvent
    (
        string? warningId
        , string userId
        , string triggerRule
        , string warningType
        , string severity
        , Dictionary<string, object>? metadata
    ) : base()
    {
        WarningId = warningId;
        UserId = userId;
        TriggerRule = triggerRule;
        WarningType = warningType;
        Severity = severity;
        Metadata = metadata;
    }

    public string? WarningId { get; }
    public string UserId { get; }
    public string TriggerRule { get; }
    public string WarningType { get; }
    public string Severity { get; }
    public Dictionary<string, object>? Metadata { get; }
}

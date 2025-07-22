using RateTheWork.Domain.Enums.Admin;

namespace RateTheWork.Domain.Events.AuditLog;

/// <summary>
/// 1. Audit log oluşturuldu event'i
/// </summary>
public class AuditLogCreatedEvent : DomainEventBase
{
    public AuditLogCreatedEvent
    (
        string? auditLogId
        , string adminUserId
        , string actionType
        , string entityType
        , string entityId
        , string severity
        , DateTime performedAt
    ) : base()
    {
        AuditLogId = auditLogId;
        AdminUserId = adminUserId;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Severity = severity;
        PerformedAt = performedAt;
    }

    public string? AuditLogId { get; }
    public string AdminUserId { get; }
    public string ActionType { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public string Severity { get; }
    public DateTime PerformedAt { get; }
}

/// <summary>
/// 2. Kritik aksiyon gerçekleşti event'i
/// </summary>
public class CriticalActionPerformedEvent : DomainEventBase
{
    public CriticalActionPerformedEvent
    (
        string? auditLogId
        , string adminUserId
        , string actionType
        , string entityType
        , string entityId
        , string details
        , DateTime performedAt
    ) : base()
    {
        AuditLogId = auditLogId;
        AdminUserId = adminUserId;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Details = details;
        PerformedAt = performedAt;
    }

    public string? AuditLogId { get; }
    public string AdminUserId { get; }
    public string ActionType { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public string Details { get; }
    public DateTime PerformedAt { get; }
}

/// <summary>
/// Kritik audit log kaydedildi event'i
/// </summary>
public class CriticalAuditLogCreatedEvent : DomainEventBase
{
    public CriticalAuditLogCreatedEvent
    (
        string auditLogId
        , string adminUserId
        , string actionType
        , string entityType
        , string entityId
        , AuditSeverity severity
    ) : base()
    {
        AuditLogId = auditLogId;
        AdminUserId = adminUserId;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Severity = severity;
    }

    public string AuditLogId { get; }
    public string AdminUserId { get; }
    public string ActionType { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public AuditSeverity Severity { get; }
}

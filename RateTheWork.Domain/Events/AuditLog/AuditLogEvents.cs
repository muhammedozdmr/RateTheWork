using RateTheWork.Domain.Enums.Admin;

namespace RateTheWork.Domain.Events.AuditLog;

/// <summary>
/// 1. Audit log oluşturuldu event'i
/// </summary>
public record AuditLogCreatedEvent(
    string? AuditLogId
    , string AdminUserId
    , string ActionType
    , string EntityType
    , string EntityId
    , string Severity
    , DateTime PerformedAt
) : DomainEventBase;

/// <summary>
/// 2. Kritik aksiyon gerçekleşti event'i
/// </summary>
public record CriticalActionPerformedEvent(
    string? AuditLogId
    , string AdminUserId
    , string ActionType
    , string EntityType
    , string EntityId
    , string Details
    , DateTime PerformedAt
) : DomainEventBase;

/// <summary>
/// Kritik audit log kaydedildi event'i
/// </summary>
public record CriticalAuditLogCreatedEvent(
    string AuditLogId
    , string AdminUserId
    , string ActionType
    , string EntityType
    , string EntityId
    , AuditSeverity Severity
) : DomainEventBase;

using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Admin;
using RateTheWork.Domain.Events.AuditLog;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Audit log entity'si - Admin işlemlerinin kaydını tutar.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private AuditLog() : base()
    {
    }

    // Properties
    public string AdminUserId { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public DateTime PerformedAt { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public AuditSeverity Severity { get; private set; }
    public string? Details { get; private set; }
    public bool IsSuccess { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Yeni audit log oluşturur (Factory method)
    /// </summary>
    public static AuditLog Create
    (
        string adminUserId
        , string actionType
        , string entityType
        , string entityId
        , string? details = null
        , string? ipAddress = null
        , string? userAgent = null
    )
    {
        ValidateActionType(actionType);
        ValidateEntityType(entityType);

        var auditLog = new AuditLog
        {
            AdminUserId = adminUserId ?? throw new ArgumentNullException(nameof(adminUserId)), ActionType = actionType
            , EntityType = entityType, EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId))
            , PerformedAt = DateTime.UtcNow, Severity = DetermineSeverityByAction(actionType), Details = details
            , IpAddress = ipAddress, UserAgent = userAgent, IsSuccess = true
        };

        // Kritik aksiyonlar için event
        if (auditLog.Severity == AuditSeverity.Critical || auditLog.Severity == AuditSeverity.Security)
        {
            auditLog.AddDomainEvent(new CriticalActionPerformedEvent(
                auditLog.Id,
                adminUserId,
                actionType,
                entityType,
                entityId,
                details ?? string.Empty,
                auditLog.PerformedAt
            ));
        }
        else
        {
            // Normal audit log event
            auditLog.AddDomainEvent(new AuditLogCreatedEvent(
                auditLog.Id,
                adminUserId,
                actionType,
                entityType,
                entityId,
                auditLog.Severity.ToString(),
                auditLog.PerformedAt
            ));
        }

        return auditLog;
    }

    /// <summary>
    /// Değişiklik bilgilerini ekle
    /// </summary>
    public void SetChangeValues<T>(T? oldValue, T? newValue)
    {
        try
        {
            OldValues = oldValue != null ? JsonSerializer.Serialize(oldValue) : null;
            NewValues = newValue != null ? JsonSerializer.Serialize(newValue) : null;
            SetModifiedDate();
        }
        catch (Exception ex)
        {
            // Serialization hatası durumunda
            OldValues = "Serialization Error";
            NewValues = "Serialization Error";
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// İşlem başarısız oldu
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        SetModifiedDate();
    }

    /// <summary>
    /// IP adresini anonimleştir (KVKK uyumluluğu)
    /// </summary>
    public void AnonymizeIpAddress()
    {
        if (string.IsNullOrEmpty(IpAddress))
            return;

        var parts = IpAddress.Split('.');
        if (parts.Length == 4) // IPv4
        {
            IpAddress = $"{parts[0]}.{parts[1]}.xxx.xxx";
        }
        else if (IpAddress.Contains(':')) // IPv6
        {
            var v6Parts = IpAddress.Split(':');
            if (v6Parts.Length >= 4)
            {
                IpAddress = $"{v6Parts[0]}:{v6Parts[1]}:xxxx:xxxx:...";
            }
        }

        SetModifiedDate();
    }

    // Private helper methods
    private static AuditSeverity DetermineSeverityByAction(string actionType)
    {
        return actionType switch
        {
            var type when type.Contains("Deleted") => AuditSeverity.Critical
            , var type when type.Contains("Banned") => AuditSeverity.Critical
            , var type when type.Contains("Password") => AuditSeverity.Security
            , var type when type.Contains("Role") => AuditSeverity.Security
            , var type when type.Contains("Settings") => AuditSeverity.Warning
            , var type when type.Contains("Export") => AuditSeverity.Warning
            , var type when type.Contains("Import") => AuditSeverity.Warning, _ => AuditSeverity.Info
        };
    }

    // Validation methods
    private static void ValidateActionType(string actionType)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentNullException(nameof(actionType));

        if (actionType.Length > 100)
            throw new BusinessRuleException("Action type 100 karakterden uzun olamaz.");
    }

    private static void ValidateEntityType(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentNullException(nameof(entityType));

        var validTypes = new[]
        {
            "User", "Company", "Review", "Admin", "System", "Security", "Badge", "Report", "Warning", "Ban", "Settings"
        };

        if (!validTypes.Contains(entityType))
            throw new BusinessRuleException($"Geçersiz entity tipi: {entityType}");
    }

    // Action Types
    public static class ActionTypes
    {
        // User Management
        public const string UserCreated = "User.Created";
        public const string UserUpdated = "User.Updated";
        public const string UserDeleted = "User.Deleted";
        public const string UserBanned = "User.Banned";
        public const string UserUnbanned = "User.Unbanned";
        public const string UserWarned = "User.Warned";

        // Company Management
        public const string CompanyApproved = "Company.Approved";
        public const string CompanyRejected = "Company.Rejected";
        public const string CompanyUpdated = "Company.Updated";
        public const string CompanyDeleted = "Company.Deleted";

        // Review Management
        public const string ReviewHidden = "Review.Hidden";
        public const string ReviewActivated = "Review.Activated";
        public const string ReviewDeleted = "Review.Deleted";

        // Document Verification
        public const string DocumentApproved = "Document.Approved";
        public const string DocumentRejected = "Document.Rejected";

        // Report Management
        public const string ReportResolved = "Report.Resolved";
        public const string ReportDismissed = "Report.Dismissed";
        public const string ReportEscalated = "Report.Escalated";

        // Admin Actions
        public const string AdminLogin = "Admin.Login";
        public const string AdminLogout = "Admin.Logout";
        public const string AdminPasswordChanged = "Admin.PasswordChanged";
        public const string AdminRoleChanged = "Admin.RoleChanged";

        // System Actions
        public const string SettingsUpdated = "System.SettingsUpdated";
        public const string DataExported = "System.DataExported";
        public const string DataImported = "System.DataImported";
        public const string BackupCreated = "System.BackupCreated";
    }
}

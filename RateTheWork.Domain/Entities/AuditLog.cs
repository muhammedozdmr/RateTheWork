using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Denetim kaydı entity'si - Admin panelinde yapılan kritik işlemlerin izlenmesi için.
/// </summary>
public class AuditLog : BaseEntity
{
    // Action Types
    public static class ActionTypes
    {
        // Review Actions
        public const string ReviewApproved = "Review.Approved";
        public const string ReviewRejected = "Review.Rejected";
        public const string ReviewDeleted = "Review.Deleted";
        public const string ReviewRestored = "Review.Restored";
        public const string ReviewEdited = "Review.Edited";
        
        // User Actions
        public const string UserBanned = "User.Banned";
        public const string UserUnbanned = "User.Unbanned";
        public const string UserWarned = "User.Warned";
        public const string UserDeleted = "User.Deleted";
        public const string UserRestored = "User.Restored";
        public const string UserVerified = "User.Verified";
        
        // Company Actions
        public const string CompanyApproved = "Company.Approved";
        public const string CompanyRejected = "Company.Rejected";
        public const string CompanyDeleted = "Company.Deleted";
        public const string CompanyRestored = "Company.Restored";
        public const string CompanyEdited = "Company.Edited";
        
        // Admin Actions
        public const string AdminCreated = "Admin.Created";
        public const string AdminDeleted = "Admin.Deleted";
        public const string AdminRoleChanged = "Admin.RoleChanged";
        public const string AdminPasswordReset = "Admin.PasswordReset";
        
        // System Actions
        public const string SettingsChanged = "System.SettingsChanged";
        public const string DataExported = "System.DataExported";
        public const string DataImported = "System.DataImported";
        public const string BackupCreated = "System.BackupCreated";
        public const string MaintenancePerformed = "System.MaintenancePerformed";
    }

    // Severity Levels
    public enum AuditSeverity
    {
        Info,      // Bilgi amaçlı
        Warning,   // Dikkat edilmesi gereken
        Critical,  // Kritik işlemler
        Security   // Güvenlik ile ilgili
    }

    // Properties
    public string AdminUserId { get; private set; }
    public string ActionType { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public string? Details { get; private set; }
    public DateTime Timestamp { get; private set; }
    public AuditSeverity Severity { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? OldValues { get; private set; } // JSON format
    public string? NewValues { get; private set; } // JSON format
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }
    public string? AdditionalData { get; private set; } // JSON format

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private AuditLog() : base() { }

    /// <summary>
    /// Yeni audit log oluşturur
    /// </summary>
    public static AuditLog Create(
        string adminUserId,
        string actionType,
        string entityType,
        string entityId,
        AuditSeverity severity = AuditSeverity.Info,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        ValidateActionType(actionType);
        ValidateEntityType(entityType);

        var auditLog = new AuditLog
        {
            AdminUserId = adminUserId ?? throw new ArgumentNullException(nameof(adminUserId)),
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId)),
            Details = details,
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = true
        };

        // Action type'a göre severity belirleme
        if (severity == AuditSeverity.Info)
        {
            auditLog.Severity = DetermineSeverityByAction(actionType);
        }

        return auditLog;
    }

    /// <summary>
    /// Değişiklik kaydı ile audit log oluşturur
    /// </summary>
    public static AuditLog CreateWithChanges<T>(
        string adminUserId,
        string actionType,
        string entityType,
        string entityId,
        T? oldEntity,
        T? newEntity,
        string? details = null,
        string? ipAddress = null) where T : class
    {
        var auditLog = Create(adminUserId, actionType, entityType, entityId, 
            AuditSeverity.Info, details, ipAddress, null);

        if (oldEntity != null)
        {
            auditLog.OldValues = JsonSerializer.Serialize(oldEntity, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        if (newEntity != null)
        {
            auditLog.NewValues = JsonSerializer.Serialize(newEntity, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        return auditLog;
    }

    /// <summary>
    /// Başarısız işlem kaydı oluşturur
    /// </summary>
    public static AuditLog CreateFailure(
        string adminUserId,
        string actionType,
        string entityType,
        string entityId,
        string failureReason,
        string? details = null,
        string? ipAddress = null)
    {
        var auditLog = Create(adminUserId, actionType, entityType, entityId,
            AuditSeverity.Warning, details, ipAddress, null);

        auditLog.IsSuccessful = false;
        auditLog.FailureReason = failureReason;

        return auditLog;
    }

    /// <summary>
    /// Güvenlik ile ilgili audit log
    /// </summary>
    public static AuditLog CreateSecurityLog(
        string adminUserId,
        string actionType,
        string details,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog
        {
            AdminUserId = adminUserId,
            ActionType = actionType,
            EntityType = "Security",
            EntityId = Guid.NewGuid().ToString(),
            Details = details,
            Timestamp = DateTime.UtcNow,
            Severity = AuditSeverity.Security,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = true
        };
    }

    /// <summary>
    /// Ek veri ekle
    /// </summary>
    public void AddAdditionalData(object data)
    {
        AdditionalData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        SetModifiedDate();
    }

    /// <summary>
    /// Ek veri ekle (dictionary olarak)
    /// </summary>
    public void AddAdditionalData(Dictionary<string, object> data)
    {
        AdditionalData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        SetModifiedDate();
    }

    /// <summary>
    /// Detay güncelle
    /// </summary>
    public void UpdateDetails(string details)
    {
        Details = details ?? throw new ArgumentNullException(nameof(details));
        SetModifiedDate();
    }

    /// <summary>
    /// Audit log özetini döndür
    /// </summary>
    public string GetSummary()
    {
        var summary = $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {AdminUserId} - {ActionType}";
        
        if (!string.IsNullOrWhiteSpace(EntityType) && !string.IsNullOrWhiteSpace(EntityId))
        {
            summary += $" ({EntityType} #{EntityId})";
        }

        if (!IsSuccessful)
        {
            summary += " [BAŞARISIZ]";
        }

        if (Severity == AuditSeverity.Security)
        {
            summary = "🔒 " + summary;
        }
        else if (Severity == AuditSeverity.Critical)
        {
            summary = "⚠️ " + summary;
        }

        return summary;
    }

    /// <summary>
    /// Değişiklikleri karşılaştır ve özet döndür
    /// </summary>
    public List<string> GetChangeSummary()
    {
        var changes = new List<string>();

        if (string.IsNullOrWhiteSpace(OldValues) || string.IsNullOrWhiteSpace(NewValues))
            return changes;

        try
        {
            var oldDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(OldValues);
            var newDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(NewValues);

            if (oldDict == null || newDict == null)
                return changes;

            foreach (var key in newDict.Keys)
            {
                if (!oldDict.ContainsKey(key))
                {
                    changes.Add($"{key}: [YENİ] -> {newDict[key]}");
                    continue;
                }

                var oldValue = oldDict[key].ToString();
                var newValue = newDict[key].ToString();

                if (oldValue != newValue)
                {
                    changes.Add($"{key}: {oldValue} -> {newValue}");
                }
            }

            foreach (var key in oldDict.Keys.Where(k => !newDict.ContainsKey(k)))
            {
                changes.Add($"{key}: {oldDict[key]} -> [SİLİNDİ]");
            }
        }
        catch
        {
            changes.Add("Değişiklikler karşılaştırılamadı");
        }

        return changes;
    }

    /// <summary>
    /// IP adresini maskele (GDPR uyumluluğu için)
    /// </summary>
    public void MaskIpAddress()
    {
        if (string.IsNullOrWhiteSpace(IpAddress))
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
            var type when type.Contains("Deleted") => AuditSeverity.Critical,
            var type when type.Contains("Banned") => AuditSeverity.Critical,
            var type when type.Contains("Password") => AuditSeverity.Security,
            var type when type.Contains("Role") => AuditSeverity.Security,
            var type when type.Contains("Settings") => AuditSeverity.Warning,
            var type when type.Contains("Export") => AuditSeverity.Warning,
            var type when type.Contains("Import") => AuditSeverity.Warning,
            _ => AuditSeverity.Info
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

        var validTypes = new[] { "User", "Company", "Review", "Admin", "System", "Security", 
            "Badge", "Report", "Warning", "Ban", "Settings" };

        if (!validTypes.Contains(entityType))
            throw new BusinessRuleException($"Geçersiz entity tipi: {entityType}");
    }
}

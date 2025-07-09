namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// Audit logging interface'i
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Audit log kaydı oluşturur
    /// </summary>
    Task LogAsync(string userId, string action, string entityType, string entityId, 
        Dictionary<string, object>? additionalData = null);
    
    /// <summary>
    /// Güvenlik olayı kaydeder
    /// </summary>
    Task LogSecurityEventAsync(string eventType, string ipAddress, string? userId = null,
        Dictionary<string, object>? context = null);
    
    /// <summary>
    /// Veri erişim kaydı oluşturur
    /// </summary>
    Task LogDataAccessAsync(string userId, string dataType, string operation, 
        string[] accessedFields);
}


namespace RateTheWork.Domain.Enums;

/// <summary>
/// Audit log seviye
/// </summary>
public enum AuditSeverity
{
    /// <summary>
    /// Bilgi
    /// </summary>
    Info,
    
    /// <summary>
    /// Uyarı
    /// </summary>
    Warning,
    
    /// <summary>
    /// Kritik
    /// </summary>
    Critical,
    
    /// <summary>
    /// Güvenlik
    /// </summary>
    Security
}

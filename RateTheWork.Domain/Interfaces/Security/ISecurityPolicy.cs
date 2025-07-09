namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// Güvenlik politikası interface'i
/// </summary>
public interface ISecurityPolicy
{
    /// <summary>
    /// Kullanıcının işlem yapma yetkisi var mı?
    /// </summary>
    Task<bool> IsAuthorizedAsync(string userId, string action, object resource);
    
    /// <summary>
    /// IP adresi güvenli mi?
    /// </summary>
    Task<bool> IsIpAddressSafeAsync(string ipAddress);
    
    /// <summary>
    /// İşlem rate limit içinde mi?
    /// </summary>
    Task<bool> IsWithinRateLimitAsync(string userId, string action);
    
    /// <summary>
    /// Şüpheli aktivite var mı?
    /// </summary>
    Task<bool> IsSuspiciousActivityAsync(string userId, string action, Dictionary<string, object> context);
}


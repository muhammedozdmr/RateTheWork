namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// GDPR uyumluluk service interface'i
/// </summary>
public interface IGdprComplianceService
{
    /// <summary>
    /// Kullanıcı verilerini anonimleştirir
    /// </summary>
    Task AnonymizeUserDataAsync(string userId);
    
    /// <summary>
    /// Kullanıcı verilerini dışa aktarır
    /// </summary>
    Task<byte[]> ExportUserDataAsync(string userId);
    
    /// <summary>
    /// Kullanıcı verilerini siler (Right to be forgotten)
    /// </summary>
    Task DeleteUserDataAsync(string userId);
    
    /// <summary>
    /// Veri işleme izni kontrolü
    /// </summary>
    Task<bool> HasDataProcessingConsentAsync(string userId, string dataType);
    
    /// <summary>
    /// Veri saklama süresi kontrolü
    /// </summary>
    Task<bool> IsDataRetentionPeriodExpiredAsync(string dataType, DateTime createdDate);
}

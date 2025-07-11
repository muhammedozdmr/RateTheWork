namespace RateTheWork.Domain.Enums.Verification;

/// <summary>
/// Doğrulama durumları
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending,
    
    /// <summary>
    /// İşleme alındı
    /// </summary>
    Processing,
    
    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Süresi doldu
    /// </summary>
    Expired
}

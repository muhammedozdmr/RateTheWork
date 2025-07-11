namespace RateTheWork.Domain.Enums;

/// <summary>
/// Şikayet durumları
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending,
    
    /// <summary>
    /// İnceleniyor
    /// </summary>
    UnderReview,
    
    /// <summary>
    /// Çözümlendi
    /// </summary>
    Resolved,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Dismissed,
    
    /// <summary>
    /// Üst yönetime iletildi
    /// </summary>
    Escalated
}

namespace RateTheWork.Domain.Enums.Report;

/// <summary>
/// Şikayet durumları
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// İnceleme bekliyor
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// İnceleniyor
    /// </summary>
    InReview = 2,
    
    /// <summary>
    /// İnceleniyor (eski kod uyumluluğu)
    /// </summary>
    UnderReview = 2,
    
    /// <summary>
    /// Çözümlendi
    /// </summary>
    Resolved = 3,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Yükseltildi (üst yönetime)
    /// </summary>
    Escalated = 5
}
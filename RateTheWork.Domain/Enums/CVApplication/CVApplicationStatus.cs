namespace RateTheWork.Domain.Enums.CVApplication;

/// <summary>
/// CV başvuru durumları
/// </summary>
public enum CVApplicationStatus
{
    /// <summary>
    /// Beklemede - İlk gönderildiğinde
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Görüntülendi - Şirket tarafından görüntülendi
    /// </summary>
    Viewed = 1,
    
    /// <summary>
    /// İndirildi - Şirket tarafından indirildi
    /// </summary>
    Downloaded = 2,
    
    /// <summary>
    /// Kabul edildi
    /// </summary>
    Accepted = 3,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Beklemede tutuluyor
    /// </summary>
    OnHold = 5,
    
    /// <summary>
    /// Süresi doldu - 90 gün sonra
    /// </summary>
    Expired = 6,
    
    /// <summary>
    /// Silindi
    /// </summary>
    Deleted = 7
}
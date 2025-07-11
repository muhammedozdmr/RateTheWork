namespace RateTheWork.Domain.Enums;

/// <summary>
/// Uyarı seviyesi
/// </summary>
public enum WarningSeverity
{
    /// <summary>
    /// Düşük - Bilgilendirme amaçlı
    /// </summary>
    Low,
    
    /// <summary>
    /// Orta - Dikkat edilmesi gereken
    /// </summary>
    Medium,
    
    /// <summary>
    /// Yüksek - Ciddi ihlal
    /// </summary>
    High,
    
    /// <summary>
    /// Kritik - Ban'a yakın
    /// </summary>
    Critical
}

namespace RateTheWork.Domain.Enums.Report;

/// <summary>
/// Şikayet sonuçlandırma türleri
/// </summary>
public enum ReportResolution
{
    /// <summary>
    /// Onaylandı - Şikayet haklı bulundu
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// Reddedildi - Şikayet haksız bulundu
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// Daha fazla bilgi gerekli
    /// </summary>
    NeedsMoreInfo = 3,
    
    /// <summary>
    /// Uyarı verildi
    /// </summary>
    WarningIssued = 4,
    
    /// <summary>
    /// İçerik kaldırıldı
    /// </summary>
    ContentRemoved = 5,
    
    /// <summary>
    /// Kullanıcı yasaklandı
    /// </summary>
    UserBanned = 6
}
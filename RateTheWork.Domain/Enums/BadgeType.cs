namespace RateTheWork.Domain.Enums;

/// <summary>
/// Rozet türleri
/// </summary>
public enum BadgeType
{
    /// <summary>
    /// İlk yorum rozeti
    /// </summary>
    FirstReview,
    
    /// <summary>
    /// Aktif yorumcu (10+ yorum)
    /// </summary>
    ActiveReviewer,
    
    /// <summary>
    /// Güvenilir yorumcu (5+ doğrulanmış yorum)
    /// </summary>
    TrustedReviewer,
    
    /// <summary>
    /// En çok katkıda bulunan (50+ yorum)
    /// </summary>
    TopContributor,
    
    /// <summary>
    /// Şirket kaşifi (10+ farklı şirket)
    /// </summary>
    CompanyExplorer,
    
    /// <summary>
    /// Detaylı yorumcu (ortalama 500+ karakter)
    /// </summary>
    DetailedReviewer,
    
    /// <summary>
    /// Faydalı yorumcu (upvote oranı %80+)
    /// </summary>
    HelpfulReviewer,
    
    /// <summary>
    /// Platform yıldönümü
    /// </summary>
    Anniversary,
    
    /// <summary>
    /// Özel etkinlik rozeti
    /// </summary>
    SpecialEvent
}

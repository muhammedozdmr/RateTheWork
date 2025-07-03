namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Şirket detay sayfası için genişletilmiş DTO
/// </summary>
public record CompanyDetailDto : CompanyDto
{
    /// <summary>
    /// Vergi numarası (maskelenmiş gösterim için)
    /// </summary>
    public string MaskedTaxId { get; init; } = string.Empty;
    
    /// <summary>
    /// Adres
    /// </summary>
    public string Address { get; init; } = string.Empty;
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Web sitesi
    /// </summary>
    public string WebsiteUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// LinkedIn profili
    /// </summary>
    public string? LinkedInUrl { get; init; }
    
    /// <summary>
    /// X (Twitter) profili
    /// </summary>
    public string? XUrl { get; init; }
    
    /// <summary>
    /// Instagram profili
    /// </summary>
    public string? InstagramUrl { get; init; }
    
    /// <summary>
    /// Facebook profili
    /// </summary>
    public string? FacebookUrl { get; init; }
    
    /// <summary>
    /// Şirket doğrulandı mı?
    /// </summary>
    public bool IsVerified { get; init; }
    
    /// <summary>
    /// Doğrulama tarihi
    /// </summary>
    public DateTime? VerificationDate { get; init; }
    
    /// <summary>
    /// Toplam yorum sayısı (override edilecek)
    /// </summary>
    public int TotalReviews { get; init; }
    
    /// <summary>
    /// Puan dağılımı (1-5 yıldız için ayrı ayrı sayılar)
    /// </summary>
    public Dictionary<int, int> RatingBreakdown { get; init; } = new();
    
    /// <summary>
    /// Yorum türü dağılımı
    /// </summary>
    public Dictionary<string, int> CommentTypeBreakdown { get; init; } = new();
    
    /// <summary>
    /// Son 3 aydaki yorum sayısı
    /// </summary>
    public int RecentReviewsCount { get; init; }
    
    /// <summary>
    /// Doğrulanmış yorum yüzdesi
    /// </summary>
    public double VerifiedReviewsPercentage { get; init; }
    
    /// <summary>
    /// Kullanıcının bu şirkete yaptığı yorum türleri (giriş yapmışsa)
    /// </summary>
    public List<string> UserReviewedTypes { get; init; } = new();
}

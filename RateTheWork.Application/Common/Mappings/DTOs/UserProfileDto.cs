namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Kullanıcı profil sayfası için detaylı DTO.
/// Kullanıcı kendi profilini görüntülerken kullanılır.
/// </summary>
public record UserProfileDto : UserDto
{
    /// <summary>
    /// Decrypt edilmiş isim (sadece kendi profilinde görünür)
    /// </summary>
    public string? FirstName { get; init; }
    
    /// <summary>
    /// Decrypt edilmiş soyisim (sadece kendi profilinde görünür)
    /// </summary>
    public string? LastName { get; init; }
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Cinsiyet
    /// </summary>
    public string Gender { get; init; } = string.Empty;
    
    /// <summary>
    /// Son çalışılan şirket
    /// </summary>
    public string? LastCompanyWorked { get; init; }
    
    /// <summary>
    /// Üyelik başlangıç tarihi
    /// </summary>
    public DateTime MemberSince { get; init; }
    
    /// <summary>
    /// Toplam yorum sayısı
    /// </summary>
    public int TotalReviews { get; init; }
    
    /// <summary>
    /// Kazanılan rozet sayısı
    /// </summary>
    public int TotalBadges { get; init; }
    
    /// <summary>
    /// Tüm doğrulamalar tamamlandı mı?
    /// </summary>
    public bool IsVerified { get; init; }
    
    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public bool IsEmailVerified { get; init; }
    
    /// <summary>
    /// Telefon doğrulandı mı?
    /// </summary>
    public bool IsPhoneVerified { get; init; }
    
    /// <summary>
    /// TC kimlik doğrulandı mı?
    /// </summary>
    public bool IsTcIdentityVerified { get; init; }
}


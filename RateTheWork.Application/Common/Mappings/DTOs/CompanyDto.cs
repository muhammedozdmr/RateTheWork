namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Şirket listesi için özet DTO
/// </summary>
public record CompanyDto
{
    /// <summary>
    /// Şirket ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket adı
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Sektör
    /// </summary>
    public string Sector { get; init; } = string.Empty;
    
    /// <summary>
    /// Logo URL'i
    /// </summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>
    /// Ortalama puan (1-5 arası)
    /// </summary>
    public decimal Rating { get; init; }
    
    /// <summary>
    /// Toplam yorum sayısı
    /// </summary>
    public int ReviewCount { get; init; }
    
    /// <summary>
    /// Şehir
    /// </summary>
    public string City { get; init; } = string.Empty;
}

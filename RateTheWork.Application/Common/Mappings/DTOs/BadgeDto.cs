namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Rozet DTO'su
/// </summary>
public record BadgeDto
{
    /// <summary>
    /// Rozet ID'si
    /// </summary>
    public string BadgeId { get; init; } = string.Empty;
    
    /// <summary>
    /// Rozet adı
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Rozet açıklaması
    /// </summary>
    public string Description { get; init; } = string.Empty;
    
    /// <summary>
    /// Rozet ikonu URL'i
    /// </summary>
    public string IconUrl { get; init; } = string.Empty;
}

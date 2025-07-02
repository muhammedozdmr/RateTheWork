namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Kullanıcı rozeti DTO'su
/// </summary>
public record UserBadgeDto
{
    /// <summary>
    /// Rozet adı
    /// </summary>
    public string BadgeName { get; init; } = string.Empty;
    
    /// <summary>
    /// Rozet açıklaması
    /// </summary>
    public string BadgeDescription { get; init; } = string.Empty;
    
    /// <summary>
    /// Rozet ikonu URL'i
    /// </summary>
    public string BadgeIconUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Kazanılma tarihi
    /// </summary>
    public DateTime AwardedAt { get; init; }
}
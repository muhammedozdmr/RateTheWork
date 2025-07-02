namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Doğrulama talebi DTO'su (Admin panel için)
/// </summary>
public record VerificationRequestDto
{
    /// <summary>
    /// Talep ID'si
    /// </summary>
    public string RequestId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorumu yapan kullanıcının adı
    /// </summary>
    public string ReviewerUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket adı
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum tarihi
    /// </summary>
    public DateTime ReviewDate { get; init; }
    
    /// <summary>
    /// Belge adı
    /// </summary>
    public string DocumentName { get; init; } = string.Empty;
    
    /// <summary>
    /// Talep tarihi
    /// </summary>
    public DateTime RequestedAt { get; init; }
    
    /// <summary>
    /// Durum (Pending, Approved, Rejected)
    /// </summary>
    public string Status { get; init; } = string.Empty;
}
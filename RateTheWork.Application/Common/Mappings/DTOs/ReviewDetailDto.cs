using RateTheWork.Application.Features.Reviews.Queries.GetReviewDetails;

namespace RateTheWork.Application.Common.Mappings.DTOs;

/// <summary>
/// Yorum detay sayfası için genişletilmiş DTO
/// </summary>
public record ReviewDetailDto : ReviewDto
{
    /// <summary>
    /// Tam yorum metni
    /// </summary>
    public string FullComment { get; init; } = string.Empty;
    
    /// <summary>
    /// Belge yüklendi mi?
    /// </summary>
    public bool HasDocument { get; init; }
    
    /// <summary>
    /// Son düzenleme tarihi
    /// </summary>
    public DateTime? LastEditDate { get; init; }
    
    /// <summary>
    /// Şikayet sayısı
    /// </summary>
    public int ReportCount { get; init; }
    
    /// <summary>
    /// Mevcut kullanıcı bu yorumu oyladı mı?
    /// </summary>
    public bool? CurrentUserVote { get; init; } // true: upvote, false: downvote, null: oylamadı
    
    /// <summary>
    /// Mevcut kullanıcı bu yorumu şikayet etti mi?
    /// </summary>
    public bool HasUserReported { get; init; }
    
    /// <summary>
    /// Kullanıcının kendi yorumu mu?
    /// </summary>
    public bool IsOwnReview { get; init; }
    
    /// <summary>
    /// Yorum yazarı bilgileri
    /// </summary>
    public ReviewAuthorInfo? AuthorInfo { get; init; }
    
    /// <summary>
    /// Şirket bilgileri
    /// </summary>
    public ReviewCompanyInfo? CompanyInfo { get; init; }
    
    /// <summary>
    /// Admin bilgileri (sadece admin görür)
    /// </summary>
    public ReviewAdminInfo? AdminInfo { get; init; }
}

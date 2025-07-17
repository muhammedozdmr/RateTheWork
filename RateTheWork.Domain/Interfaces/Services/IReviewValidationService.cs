namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Yorum validasyon işlemleri için service interface'i
/// </summary>
public interface IReviewValidationService
{
    /// <summary>
    /// Kullanıcının belirli bir şirkete yorum yapıp yapamayacağını kontrol eder
    /// </summary>
    Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, string commentType);

    /// <summary>
    /// Spam kontrolü yapar
    /// </summary>
    Task<bool> IsSpamReviewAsync(string userId, string commentText);

    /// <summary>
    /// Benzer yorumları bulur (duplicate kontrolü için)
    /// </summary>
    Task<List<string?>> FindSimilarReviewsAsync(string userId, string commentText);

    /// <summary>
    /// Yorumun içerik validasyonunu yapar
    /// </summary>
    Task<bool> ValidateReviewContentAsync(string commentText, string commentType);
}

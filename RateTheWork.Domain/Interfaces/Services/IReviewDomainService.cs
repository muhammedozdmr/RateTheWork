using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Yorum işlemleri için domain service interface'i
/// </summary>
public interface IReviewDomainService
{
    /// <summary>
    /// Kullanıcının belirli bir şirkete yorum yapıp yapamayacağını kontrol eder
    /// </summary>
    Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, string commentType);

    /// <summary>
    /// Yorumun faydalılık skorunu hesaplar
    /// </summary>
    decimal CalculateHelpfulnessScore(int upvotes, int downvotes, bool isVerified);

    /// <summary>
    /// Şirketin ortalama puanını günceller
    /// </summary>
    Task UpdateCompanyRatingAsync(string companyId);

    /// <summary>
    /// Spam kontrolü yapar
    /// </summary>
    Task<bool> IsSpamReviewAsync(string userId, string commentText);
    
    /// <summary>
    /// Yorum kalite skorunu hesaplar
    /// </summary>
    Task<ReviewQualityScore> CalculateReviewQualityAsync(string reviewId);
    
    /// <summary>
    /// Benzer yorumları bulur (duplicate kontrolü için)
    /// </summary>
    Task<List<string?>> FindSimilarReviewsAsync(string userId, string commentText);
    
    /// <summary>
    /// Yorum trendlerini analiz eder
    /// </summary>
    Task<ReviewTrends> AnalyzeReviewTrendsAsync(string companyId, DateTime startDate, DateTime endDate);
}


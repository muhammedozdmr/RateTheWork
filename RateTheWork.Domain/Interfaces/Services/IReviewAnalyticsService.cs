using RateTheWork.Domain.ValueObjects.Review;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Yorum analitik işlemleri için service interface'i
/// </summary>
public interface IReviewAnalyticsService
{
    /// <summary>
    /// Yorum trendlerini analiz eder
    /// </summary>
    Task<ReviewTrends> AnalyzeReviewTrendsAsync(string companyId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Şirketin ortalama puanını günceller
    /// </summary>
    Task UpdateCompanyRatingAsync(string companyId);

    /// <summary>
    /// Kategori bazlı yorum dağılımını analiz eder
    /// </summary>
    Task<Dictionary<string, decimal>> AnalyzeCategoryDistributionAsync(string companyId);

    /// <summary>
    /// Zaman bazlı yorum dağılımını analiz eder
    /// </summary>
    Task<Dictionary<DateTime, int>> AnalyzeTimeDistributionAsync(string companyId, TimeSpan period);

    /// <summary>
    /// Sentiment analizi yapar
    /// </summary>
    Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string companyId, DateTime? startDate = null);
}

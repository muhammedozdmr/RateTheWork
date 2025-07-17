using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Yorum analitik işlemleri için service implementasyonu
/// </summary>
public class ReviewAnalyticsService : IReviewAnalyticsService
{
    private readonly IReviewDomainService _reviewDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewAnalyticsService(IUnitOfWork unitOfWork, IReviewDomainService reviewDomainService)
    {
        _unitOfWork = unitOfWork;
        _reviewDomainService = reviewDomainService;
    }

    public async Task<ReviewTrends> AnalyzeReviewTrendsAsync(string companyId, DateTime startDate, DateTime endDate)
    {
        return await _reviewDomainService.AnalyzeReviewTrendsAsync(companyId, startDate, endDate);
    }

    public async Task UpdateCompanyRatingAsync(string companyId)
    {
        await _reviewDomainService.UpdateCompanyRatingAsync(companyId);
    }

    public async Task<Dictionary<string, decimal>> AnalyzeCategoryDistributionAsync(string companyId)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);

        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
            return new Dictionary<string, decimal>();

        var categoryDistribution = activeReviews
            .GroupBy(r => r.CommentType)
            .ToDictionary(
                g => g.Key.ToString(),
                g => Math.Round(g.Average(r => r.OverallRating), 2)
            );

        return categoryDistribution;
    }

    public async Task<Dictionary<DateTime, int>> AnalyzeTimeDistributionAsync(string companyId, TimeSpan period)
    {
        var startDate = DateTime.UtcNow.Subtract(period);
        var reviews = await _unitOfWork.Repository<Review>()
            .GetAsync(r => r.CompanyId == companyId &&
                           r.CreatedAt >= startDate &&
                           r.IsActive);

        var timeDistribution = reviews
            .GroupBy(r => r.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        return timeDistribution;
    }

    public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string companyId, DateTime? startDate = null)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);

        var activeReviews = reviews.Where(r => r.IsActive);

        if (startDate.HasValue)
            activeReviews = activeReviews.Where(r => r.CreatedAt >= startDate.Value);

        var reviewList = activeReviews.ToList();

        if (!reviewList.Any())
            return new SentimentAnalysisResult();

        var result = new SentimentAnalysisResult();

        // Basit sentiment analizi - puan bazlı
        var positiveReviews = reviewList.Count(r => r.OverallRating >= 4);
        var negativeReviews = reviewList.Count(r => r.OverallRating <= 2);
        var neutralReviews = reviewList.Count(r => r.OverallRating == 3);

        var total = reviewList.Count;

        result.PositiveScore = (double)positiveReviews / total;
        result.NegativeScore = (double)negativeReviews / total;
        result.NeutralScore = (double)neutralReviews / total;

        // Dominant sentiment belirleme
        if (result.PositiveScore > result.NegativeScore && result.PositiveScore > result.NeutralScore)
            result.DominantSentiment = "Positive";
        else if (result.NegativeScore > result.PositiveScore && result.NegativeScore > result.NeutralScore)
            result.DominantSentiment = "Negative";
        else
            result.DominantSentiment = "Neutral";

        // Emotion skorları (basit simülasyon)
        result.EmotionScores["satisfaction"] = result.PositiveScore * 0.8;
        result.EmotionScores["frustration"] = result.NegativeScore * 0.7;
        result.EmotionScores["neutrality"] = result.NeutralScore * 0.9;

        return result;
    }
}

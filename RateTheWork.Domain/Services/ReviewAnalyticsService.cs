using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Extensions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects.Review;

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
            return SentimentAnalysisResult.CreateEmpty();

        var result = SentimentAnalysisResult.CreateEmpty();

        // Basit sentiment analizi - puan bazlı
        var positiveReviews = reviewList.Count(r => r.OverallRating >= 4);
        var negativeReviews = reviewList.Count(r => r.OverallRating <= 2);
        var neutralReviews = reviewList.Count(r => r.OverallRating == 3);

        var total = reviewList.Count;

        var positiveScore = (double)positiveReviews / total;
        var negativeScore = (double)negativeReviews / total;
        var neutralScore = (double)neutralReviews / total;

        // Emotion skorları (basit simülasyon)
        var emotionScores = new Dictionary<string, double>
        {
            ["satisfaction"] = positiveScore * 0.8, ["frustration"] = negativeScore * 0.7
            , ["neutrality"] = neutralScore * 0.9
        };

        return SentimentAnalysisResult.Create(positiveScore, negativeScore, neutralScore, emotionScores);
    }
}

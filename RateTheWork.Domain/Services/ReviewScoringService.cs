using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Yorum skorlama işlemleri için service implementasyonu
/// </summary>
public class ReviewScoringService : IReviewScoringService
{
    private readonly IReviewDomainService _reviewDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewScoringService(IUnitOfWork unitOfWork, IReviewDomainService reviewDomainService)
    {
        _unitOfWork = unitOfWork;
        _reviewDomainService = reviewDomainService;
    }

    public decimal CalculateHelpfulnessScore(int upvotes, int downvotes, bool isVerified)
    {
        return _reviewDomainService.CalculateHelpfulnessScore(upvotes, downvotes, isVerified);
    }

    public async Task<ReviewQualityScore> CalculateReviewQualityAsync(string reviewId)
    {
        return await _reviewDomainService.CalculateReviewQualityAsync(reviewId);
    }

    public decimal CalculateCredibilityScore(bool isVerified, int userReviewCount, decimal userAverageScore)
    {
        decimal score = 0;

        // Doğrulanmış kullanıcı bonusu
        if (isVerified)
            score += 40;

        // Deneyim bonusu (yorum sayısı)
        if (userReviewCount >= 50)
            score += 30;
        else if (userReviewCount >= 20)
            score += 20;
        else if (userReviewCount >= 10)
            score += 15;
        else if (userReviewCount >= 5)
            score += 10;

        // Kalite bonusu (ortalama puan)
        if (userAverageScore >= 4.5m)
            score += 30;
        else if (userAverageScore >= 4.0m)
            score += 25;
        else if (userAverageScore >= 3.5m)
            score += 20;
        else if (userAverageScore >= 3.0m)
            score += 15;
        else if (userAverageScore >= 2.5m)
            score += 10;

        return Math.Min(score, 100);
    }

    public decimal CalculateEngagementScore(int upvotes, int downvotes, int viewCount, int shareCount)
    {
        var totalVotes = upvotes + downvotes;
        var engagementRate = viewCount > 0 ? (decimal)totalVotes / viewCount : 0;

        // Temel engagement skoru
        var score = engagementRate * 50;

        // Upvote oranı bonusu
        if (totalVotes > 0)
        {
            var upvoteRatio = (decimal)upvotes / totalVotes;
            score += upvoteRatio * 30;
        }

        // Paylaşım bonusu
        if (shareCount > 0)
        {
            score += Math.Min(shareCount * 5, 20);
        }

        // Minimum etkileşim penaltısı
        if (totalVotes < 3)
            score *= 0.5m;

        return Math.Min(score, 100);
    }
}

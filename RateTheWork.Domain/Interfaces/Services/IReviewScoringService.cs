using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Yorum skorlama işlemleri için service interface'i
/// </summary>
public interface IReviewScoringService
{
    /// <summary>
    /// Yorumun faydalılık skorunu hesaplar
    /// </summary>
    decimal CalculateHelpfulnessScore(int upvotes, int downvotes, bool isVerified);

    /// <summary>
    /// Yorum kalite skorunu hesaplar
    /// </summary>
    Task<ReviewQualityScore> CalculateReviewQualityAsync(string reviewId);

    /// <summary>
    /// Yorumun güvenilirlik skorunu hesaplar
    /// </summary>
    decimal CalculateCredibilityScore(bool isVerified, int userReviewCount, decimal userAverageScore);

    /// <summary>
    /// Yorumun etkileşim skorunu hesaplar
    /// </summary>
    decimal CalculateEngagementScore(int upvotes, int downvotes, int viewCount, int shareCount);
}

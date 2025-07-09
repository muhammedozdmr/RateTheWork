namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// Puanlama strategy interface'i
/// </summary>
public interface IScoringStrategy
{
    /// <summary>
    /// Strategy adı
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Genel puan hesaplama
    /// </summary>
    decimal CalculateOverallScore(Dictionary<string, decimal> categoryScores);
    
    /// <summary>
    /// Ağırlıklı puan hesaplama
    /// </summary>
    decimal CalculateWeightedScore(Dictionary<string, decimal> categoryScores, 
        Dictionary<string, decimal> weights);
    
    /// <summary>
    /// Güvenilirlik skoru hesaplama
    /// </summary>
    decimal CalculateReliabilityScore(int totalReviews, int verifiedReviews, 
        TimeSpan averageReviewAge);
}

namespace RateTheWork.Domain.ValueObject;

/// <summary>
/// Yorum kalite skoru
/// </summary>
public class ReviewQualityScore
{
    public decimal OverallScore { get; set; }
    public decimal LengthScore { get; set; }
    public decimal DetailScore { get; set; }
    public decimal ObjectivityScore { get; set; }
    public decimal HelpfulnessScore { get; set; }
    public List<string> ImprovementSuggestions { get; set; } = new();
}


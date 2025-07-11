namespace RateTheWork.Domain.ValueObject;

/// <summary>
/// Duygu analizi sonucu
/// </summary>
public class SentimentAnalysisResult
{
    public double PositiveScore { get; set; }
    public double NegativeScore { get; set; }
    public double NeutralScore { get; set; }
    public string DominantSentiment { get; set; } = string.Empty;
    public Dictionary<string, double> EmotionScores { get; set; } = new();
}

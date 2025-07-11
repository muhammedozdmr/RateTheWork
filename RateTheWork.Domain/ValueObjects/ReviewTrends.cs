namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// Yorum trendleri
/// </summary>
public class ReviewTrends
{
    public Dictionary<string, decimal> CategoryAverages { get; set; } = new();
    public Dictionary<DateTime, int> ReviewCountByDate { get; set; } = new();
    public List<string> MostMentionedPositives { get; set; } = new();
    public List<string> MostMentionedNegatives { get; set; } = new();
    public decimal SentimentTrend { get; set; } // -1 to 1
}

using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Review;

/// <summary>
/// Yorum trendleri analiz sonucu
/// </summary>
public sealed class ReviewTrends : ValueObject
{
    private ReviewTrends
    (
        Dictionary<string, decimal> categoryAverages
        , Dictionary<DateTime, int> reviewCountByDate
        , List<string> mostMentionedPositives
        , List<string> mostMentionedNegatives
        , decimal sentimentTrend
    )
    {
        CategoryAverages = categoryAverages ?? new Dictionary<string, decimal>();
        ReviewCountByDate = reviewCountByDate ?? new Dictionary<DateTime, int>();
        MostMentionedPositives = mostMentionedPositives ?? new List<string>();
        MostMentionedNegatives = mostMentionedNegatives ?? new List<string>();
        SentimentTrend = Math.Clamp(sentimentTrend, -1, 1);
    }

    /// <summary>
    /// Kategori bazında ortalama puanlar
    /// </summary>
    public Dictionary<string, decimal> CategoryAverages { get; }

    /// <summary>
    /// Tarih bazında yorum sayıları
    /// </summary>
    public Dictionary<DateTime, int> ReviewCountByDate { get; }

    /// <summary>
    /// En çok bahsedilen pozitif konular
    /// </summary>
    public List<string> MostMentionedPositives { get; }

    /// <summary>
    /// En çok bahsedilen negatif konular
    /// </summary>
    public List<string> MostMentionedNegatives { get; }

    /// <summary>
    /// Sentiment trend skoru (-1 ile 1 arasında)
    /// </summary>
    public decimal SentimentTrend { get; }

    /// <summary>
    /// Yorum trendlerini oluşturur
    /// </summary>
    public static ReviewTrends Create
    (
        Dictionary<string, decimal> categoryAverages
        , Dictionary<DateTime, int> reviewCountByDate
        , List<string> mostMentionedPositives
        , List<string> mostMentionedNegatives
        , decimal sentimentTrend
    )
    {
        return new ReviewTrends(
            categoryAverages,
            reviewCountByDate,
            mostMentionedPositives,
            mostMentionedNegatives,
            sentimentTrend);
    }

    /// <summary>
    /// Boş trend analizi oluşturur
    /// </summary>
    public static ReviewTrends CreateEmpty()
    {
        return new ReviewTrends(
            new Dictionary<string, decimal>(),
            new Dictionary<DateTime, int>(),
            new List<string>(),
            new List<string>(),
            0);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return string.Join(",", CategoryAverages.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
        yield return string.Join(","
            , ReviewCountByDate.OrderBy(x => x.Key).Select(x => $"{x.Key:yyyy-MM-dd}:{x.Value}"));
        yield return string.Join(",", MostMentionedPositives.OrderBy(x => x));
        yield return string.Join(",", MostMentionedNegatives.OrderBy(x => x));
        yield return SentimentTrend;
    }
}

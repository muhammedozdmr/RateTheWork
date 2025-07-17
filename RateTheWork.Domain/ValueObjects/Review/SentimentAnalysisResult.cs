using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Review;

/// <summary>
/// Duygu analizi sonucu
/// </summary>
public sealed class SentimentAnalysisResult : ValueObject
{
    private SentimentAnalysisResult
    (
        double positiveScore
        , double negativeScore
        , double neutralScore
        , string dominantSentiment
        , Dictionary<string, double> emotionScores
    )
    {
        PositiveScore = Math.Clamp(positiveScore, 0, 1);
        NegativeScore = Math.Clamp(negativeScore, 0, 1);
        NeutralScore = Math.Clamp(neutralScore, 0, 1);
        DominantSentiment = dominantSentiment ?? "Neutral";
        EmotionScores = emotionScores ?? new Dictionary<string, double>();
    }

    /// <summary>
    /// Pozitif duygu skoru (0-1 arası)
    /// </summary>
    public double PositiveScore { get; }

    /// <summary>
    /// Negatif duygu skoru (0-1 arası)
    /// </summary>
    public double NegativeScore { get; }

    /// <summary>
    /// Nötr duygu skoru (0-1 arası)
    /// </summary>
    public double NeutralScore { get; }

    /// <summary>
    /// Dominant duygu durumu
    /// </summary>
    public string DominantSentiment { get; }

    /// <summary>
    /// Detaylı duygu skorları
    /// </summary>
    public Dictionary<string, double> EmotionScores { get; }

    /// <summary>
    /// Duygu analizi sonucu oluşturur
    /// </summary>
    public static SentimentAnalysisResult Create
    (
        double positiveScore
        , double negativeScore
        , double neutralScore
        , Dictionary<string, double>? emotionScores = null
    )
    {
        // Dominant sentiment belirleme
        string dominantSentiment;
        if (positiveScore > negativeScore && positiveScore > neutralScore)
            dominantSentiment = "Positive";
        else if (negativeScore > positiveScore && negativeScore > neutralScore)
            dominantSentiment = "Negative";
        else
            dominantSentiment = "Neutral";

        return new SentimentAnalysisResult(
            positiveScore,
            negativeScore,
            neutralScore,
            dominantSentiment,
            emotionScores ?? new Dictionary<string, double>());
    }

    /// <summary>
    /// Boş analiz sonucu oluşturur
    /// </summary>
    public static SentimentAnalysisResult CreateEmpty()
    {
        return new SentimentAnalysisResult(
            0, 0, 1, "Neutral", new Dictionary<string, double>());
    }

    /// <summary>
    /// Genel duygu skorunu hesaplar (-1 ile 1 arasında)
    /// </summary>
    public double GetOverallSentimentScore()
    {
        return PositiveScore - NegativeScore;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PositiveScore;
        yield return NegativeScore;
        yield return NeutralScore;
        yield return DominantSentiment;
        yield return string.Join(",", EmotionScores.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
    }
}

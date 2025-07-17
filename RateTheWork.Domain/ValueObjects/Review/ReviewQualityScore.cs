using RateTheWork.Domain.Constants;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Review;

/// <summary>
/// Yorum kalite skoru
/// </summary>
public sealed class ReviewQualityScore : ValueObject
{
    private ReviewQualityScore
    (
        decimal lengthScore
        , decimal detailScore
        , decimal objectivityScore
        , decimal helpfulnessScore
        , List<string> improvementSuggestions
    )
    {
        LengthScore = Math.Clamp(lengthScore, DomainConstants.QualityScore.MinQualityScore
            , DomainConstants.QualityScore.MaxQualityScore);
        DetailScore = Math.Clamp(detailScore, DomainConstants.QualityScore.MinQualityScore
            , DomainConstants.QualityScore.MaxQualityScore);
        ObjectivityScore = Math.Clamp(objectivityScore, DomainConstants.QualityScore.MinQualityScore
            , DomainConstants.QualityScore.MaxQualityScore);
        HelpfulnessScore = Math.Clamp(helpfulnessScore, DomainConstants.QualityScore.MinQualityScore
            , DomainConstants.QualityScore.MaxQualityScore);
        ImprovementSuggestions = improvementSuggestions ?? new List<string>();

        // Genel skoru hesapla
        OverallScore = Math.Round(
            (LengthScore * DomainConstants.QualityScore.LengthScoreWeight +
             DetailScore * DomainConstants.QualityScore.DetailScoreWeight +
             ObjectivityScore * DomainConstants.QualityScore.ObjectivityScoreWeight +
             HelpfulnessScore * DomainConstants.QualityScore.HelpfulnessScoreWeight), 2);
    }

    /// <summary>
    /// Genel kalite skoru (0-100 arası)
    /// </summary>
    public decimal OverallScore { get; }

    /// <summary>
    /// Uzunluk skoru (0-100 arası)
    /// </summary>
    public decimal LengthScore { get; }

    /// <summary>
    /// Detay skoru (0-100 arası)
    /// </summary>
    public decimal DetailScore { get; }

    /// <summary>
    /// Objektiflik skoru (0-100 arası)
    /// </summary>
    public decimal ObjectivityScore { get; }

    /// <summary>
    /// Yararlılık skoru (0-100 arası)
    /// </summary>
    public decimal HelpfulnessScore { get; }

    /// <summary>
    /// İyileştirme önerileri
    /// </summary>
    public List<string> ImprovementSuggestions { get; }

    /// <summary>
    /// Yorum kalite skoru oluşturur
    /// </summary>
    public static ReviewQualityScore Create
    (
        decimal lengthScore
        , decimal detailScore
        , decimal objectivityScore
        , decimal helpfulnessScore
        , List<string>? improvementSuggestions = null
    )
    {
        return new ReviewQualityScore(
            lengthScore,
            detailScore,
            objectivityScore,
            helpfulnessScore,
            improvementSuggestions ?? new List<string>());
    }

    /// <summary>
    /// Düşük kalite skoru oluşturur
    /// </summary>
    public static ReviewQualityScore CreateLowQuality(List<string> suggestions)
    {
        return new ReviewQualityScore(
            DomainConstants.QualityScore.LowLengthScore,
            DomainConstants.QualityScore.LowDetailScore,
            DomainConstants.QualityScore.LowObjectivityScore,
            DomainConstants.QualityScore.LowHelpfulnessScore,
            suggestions);
    }

    /// <summary>
    /// Yüksek kalite skoru oluşturur
    /// </summary>
    public static ReviewQualityScore CreateHighQuality()
    {
        return new ReviewQualityScore(
            DomainConstants.QualityScore.HighLengthScore,
            DomainConstants.QualityScore.HighDetailScore,
            DomainConstants.QualityScore.HighObjectivityScore,
            DomainConstants.QualityScore.HighHelpfulnessScore,
            new List<string>());
    }

    /// <summary>
    /// Kalite seviyesini döndürür
    /// </summary>
    public string GetQualityLevel()
    {
        return OverallScore switch
        {
            >= DomainConstants.QualityScore.ExcellentThreshold => "Excellent"
            , >= DomainConstants.QualityScore.GoodThreshold => "Good"
            , >= DomainConstants.QualityScore.FairThreshold => "Fair"
            , >= DomainConstants.QualityScore.PoorThreshold => "Poor", _ => "Very Poor"
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LengthScore;
        yield return DetailScore;
        yield return ObjectivityScore;
        yield return HelpfulnessScore;
        yield return string.Join(",", ImprovementSuggestions.OrderBy(x => x));
    }
}

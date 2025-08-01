using RateTheWork.Domain.Constants;
using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Moderation;

/// <summary>
/// İçerik moderasyon sonucunu temsil eden value object
/// </summary>
public sealed class ModerationResult : ValueObject
{
    public ModerationResult
    (
        bool isApproved
        , string reason
        , List<string> flaggedWords
        , double confidenceScore
        , Dictionary<string, double> categoryScores
        , ModerationDetails details
        , List<string>? suggestedCorrections = null
        , string? moderationType = null
    )
    {
        IsApproved = isApproved;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Details = details ?? throw new ArgumentNullException(nameof(details));
        FlaggedWords = flaggedWords ?? new List<string>();
        ConfidenceScore = Math.Clamp(confidenceScore, DomainConstants.ModerationScore.MinConfidenceScore
            , DomainConstants.ModerationScore.MaxConfidenceScore);
        CategoryScores = categoryScores ?? new Dictionary<string, double>();
        SuggestedCorrections = suggestedCorrections ?? new List<string>();
    }

    /// <summary>
    /// İçerik onaylandı mı?
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Moderasyon sonuç nedeni
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Tespit edilen sorunlu kelimeler
    /// </summary>
    public List<string> FlaggedWords { get; }

    public double ToxicityScore { get; set; }

    /// <summary>
    /// Moderasyon güven skoru (0-1 arası)
    /// </summary>
    public double ConfidenceScore { get; }

    /// <summary>
    /// Kategori bazlı skorlar (örn: "toxicity": 0.8, "spam": 0.2)
    /// </summary>
    public Dictionary<string, double> CategoryScores { get; }

    /// <summary>
    /// Önerilen düzeltmeler
    /// </summary>
    public List<string> SuggestedCorrections { get; }
    
    /// <summary>
    /// Red nedenleri (Application katmanı uyumluluğu için)
    /// </summary>
    public List<string> RejectionReasons => !IsApproved ? new List<string> { Reason ?? "Bilinmeyen neden" } : new List<string>();

    /// <summary>
    /// Moderasyon detayları
    /// </summary>
    public ModerationDetails Details { get; }

    /// <summary>
    /// Moderasyon zamanını döndürür (geriye uyumluluk)
    /// </summary>
    public DateTime ModeratedAt => Details.ModeratedAt;

    /// <summary>
    /// Moderasyon tipini döndürür (geriye uyumluluk)
    /// </summary>
    public string ModerationType => Details.ModerationType;

    /// <summary>
    /// Onaylanmış içerik için factory method
    /// </summary>
    public static ModerationResult CreateApproved
    (
        string reason = "İçerik uygun"
        , double confidenceScore = DomainConstants.ModerationScore.MaxConfidenceScore
        , Dictionary<string, double>? categoryScores = null
    )
    {
        var details = ModerationDetails.CreateAutomatic(
            new List<string> { "Content passed all checks" },
            "AI"
        );

        return new ModerationResult(
            true,
            reason,
            new List<string>(),
            confidenceScore,
            categoryScores ?? new Dictionary<string, double>(),
            details
        );
    }

    /// <summary>
    /// Reddedilmiş içerik için factory method
    /// </summary>
    public static ModerationResult CreateRejected
    (
        string reason
        , List<string> flaggedWords
        , double confidenceScore = DomainConstants.ModerationScore.MinConfidenceScore
        , Dictionary<string, double>? categoryScores = null
        , List<string>? suggestedCorrections = null
    )
    {
        var details = ModerationDetails.CreateAutomatic(
            flaggedWords.Select(w => $"Flagged word: {w}").ToList(),
            "AI"
        );

        return new ModerationResult(
            false,
            reason,
            flaggedWords,
            confidenceScore,
            categoryScores ?? new Dictionary<string, double>(),
            details,
            suggestedCorrections
        );
    }

    /// <summary>
    /// Manuel inceleme gerektiren içerik için factory method
    /// </summary>
    public static ModerationResult CreatePendingReview
    (
        string reason
        , List<string> flaggedWords
        , double confidenceScore = DomainConstants.ModerationScore.DefaultMediumConfidence
        , Dictionary<string, double>? categoryScores = null
        , List<string>? suggestedCorrections = null
    )
    {
        var details = ModerationDetails.CreateAutomatic(
            new List<string> { "Requires manual review" },
            "AI",
            reason
        );

        return new ModerationResult(
            false,
            $"[Manuel İnceleme Gerekli] {reason}",
            flaggedWords,
            confidenceScore,
            categoryScores ?? new Dictionary<string, double>(),
            details,
            suggestedCorrections
        );
    }

    /// <summary>
    /// Otomatik düzeltme önerileri ile factory method
    /// </summary>
    public static ModerationResult CreateWithSuggestions
    (
        bool isApproved
        , string reason
        , List<string> flaggedWords
        , List<string> suggestedCorrections
        , ModerationDetails details
        , double confidenceScore = DomainConstants.ModerationScore.DefaultHighConfidence
        , Dictionary<string, double>? categoryScores = null
    )
    {
        return new ModerationResult(
            isApproved,
            reason,
            flaggedWords,
            confidenceScore,
            categoryScores ?? new Dictionary<string, double>(),
            details
        );
    }

    /// <summary>
    /// Moderasyon sonucunun ciddiyet seviyesini hesaplar
    /// </summary>
    public string GetSeverityLevel()
    {
        if (IsApproved) return "None";

        var maxScore = CategoryScores.Any() ? CategoryScores.Values.Max() : ConfidenceScore;

        return maxScore switch
        {
            >= DomainConstants.ModerationScore.CriticalSeverityThreshold => "Critical"
            , >= DomainConstants.ModerationScore.HighSeverityThreshold => "High"
            , >= DomainConstants.ModerationScore.MediumSeverityThreshold => "Medium"
            , >= DomainConstants.ModerationScore.LowSeverityThreshold => "Low", _ => "Minimal"
        };
    }

    /// <summary>
    /// Önerilen düzeltmeleri formatlanmış string olarak döndürür
    /// </summary>
    public string GetFormattedSuggestions()
    {
        if (!SuggestedCorrections.Any())
            return "Düzeltme önerisi bulunmamaktadır.";

        return "Önerilen düzeltmeler:\n" + string.Join("\n- ", SuggestedCorrections);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ModerationResult other)
            return false;

        return IsApproved == other.IsApproved &&
               Reason == other.Reason &&
               ConfidenceScore == other.ConfidenceScore &&
               ModerationType == other.ModerationType &&
               FlaggedWords.SequenceEqual(other.FlaggedWords) &&
               CategoryScores.SequenceEqual(other.CategoryScores) &&
               SuggestedCorrections.SequenceEqual(other.SuggestedCorrections);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = DomainConstants.ModerationScore.HashMultiplier;
            hash = hash * DomainConstants.ModerationScore.HashBase + IsApproved.GetHashCode();
            hash = hash * DomainConstants.ModerationScore.HashBase + Reason.GetHashCode();
            hash = hash * DomainConstants.ModerationScore.HashBase + ConfidenceScore.GetHashCode();
            hash = hash * DomainConstants.ModerationScore.HashBase + ModerationType.GetHashCode();

            foreach (var word in FlaggedWords)
            {
                hash = hash * DomainConstants.ModerationScore.HashBase + word.GetHashCode();
            }

            foreach (var score in CategoryScores)
            {
                hash = hash * DomainConstants.ModerationScore.HashBase + score.Key.GetHashCode();
                hash = hash * DomainConstants.ModerationScore.HashBase + score.Value.GetHashCode();
            }

            foreach (var correction in SuggestedCorrections)
            {
                hash = hash * DomainConstants.ModerationScore.HashBase + correction.GetHashCode();
            }

            return hash;
        }
    }

    public static bool operator ==(ModerationResult? left, ModerationResult? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ModerationResult? left, ModerationResult? right)
    {
        return !(left == right);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsApproved;
        yield return Reason;
        yield return string.Join(",", FlaggedWords.OrderBy(x => x));
        yield return ConfidenceScore;
        yield return string.Join(",", CategoryScores.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}"));
        yield return Details;
        yield return string.Join(",", SuggestedCorrections.OrderBy(x => x));
    }
}

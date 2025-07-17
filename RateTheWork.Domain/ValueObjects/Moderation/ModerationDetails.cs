using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.Moderation;

/// <summary>
/// Detaylı moderasyon analizi
/// </summary>
public class ModerationDetails : ValueObject
{
    private ModerationDetails
    (
        string moderatedBy
        , string moderationType
        , DateTime moderatedAt
        , List<string> appliedRules
        , string source
        , string? additionalNotes = null
    )
    {
        ModeratedBy = moderatedBy ?? throw new ArgumentNullException(nameof(moderatedBy));
        ModerationType = moderationType ?? throw new ArgumentNullException(nameof(moderationType));
        ModeratedAt = moderatedAt;
        AppliedRules = appliedRules ?? new List<string>();
        Source = source ?? throw new ArgumentNullException(nameof(source));
        AdditionalNotes = additionalNotes;
    }

    public double ProfanityScore { get; set; }
    public double HateSpeechScore { get; set; }
    public double PersonalAttackScore { get; set; }
    public double SpamScore { get; set; }
    public double ConfidentialInfoScore { get; set; }
    public List<string> DetectedPersonalInfo { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public Dictionary<string, double> CategoryScores { get; set; } = new();

    /// <summary>
    /// Moderasyonu yapan kişi/sistem
    /// </summary>
    public string ModeratedBy { get; }

    /// <summary>
    /// Moderasyon tipi (Auto, Manual, Hybrid)
    /// </summary>
    public string ModerationType { get; }

    /// <summary>
    /// Moderasyon zamanı
    /// </summary>
    public DateTime ModeratedAt { get; }

    /// <summary>
    /// Moderasyon kuralları
    /// </summary>
    public List<string> AppliedRules { get; }

    /// <summary>
    /// Moderasyon kaynağı (örn: AI, Keyword Filter, Manual Review)
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Ek notlar
    /// </summary>
    public string? AdditionalNotes { get; }

    /// <summary>
    /// Otomatik moderasyon detayları oluşturur
    /// </summary>
    public static ModerationDetails CreateAutomatic
    (
        List<string> appliedRules
        , string source = "AI"
        , string? additionalNotes = null
    )
    {
        return new ModerationDetails(
            "SYSTEM",
            "Auto",
            DateTime.UtcNow,
            appliedRules,
            source,
            additionalNotes
        );
    }

    /// <summary>
    /// Manuel moderasyon detayları oluşturur
    /// </summary>
    public static ModerationDetails CreateManual
    (
        string moderatorId
        , List<string> appliedRules
        , string? additionalNotes = null
    )
    {
        return new ModerationDetails(
            moderatorId,
            "Manual",
            DateTime.UtcNow,
            appliedRules,
            "Manual Review",
            additionalNotes
        );
    }

    /// <summary>
    /// Hibrit moderasyon detayları oluşturur (önce otomatik, sonra manuel)
    /// </summary>
    public static ModerationDetails CreateHybrid
    (
        string moderatorId
        , List<string> appliedRules
        , string initialSource = "AI"
        , string? additionalNotes = null
    )
    {
        return new ModerationDetails(
            moderatorId,
            "Hybrid",
            DateTime.UtcNow,
            appliedRules,
            $"{initialSource} + Manual Review",
            additionalNotes
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ModeratedBy;
        yield return ModerationType;
        yield return ModeratedAt;
        yield return string.Join(",", AppliedRules.OrderBy(x => x));
        yield return Source;
        yield return AdditionalNotes ?? string.Empty;
    }
}

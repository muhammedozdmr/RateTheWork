namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// Detaylı moderasyon analizi
/// </summary>
public class ModerationDetails
{
    public double ProfanityScore { get; set; }
    public double HateSpeechScore { get; set; }
    public double PersonalAttackScore { get; set; }
    public double SpamScore { get; set; }
    public double ConfidentialInfoScore { get; set; }
    public List<string> DetectedPersonalInfo { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

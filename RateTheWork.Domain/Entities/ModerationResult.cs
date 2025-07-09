namespace RateTheWork.Domain.Entities;

/// <summary>
/// Moderasyon sonucu
/// </summary>
public class ModerationResult
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public double ToxicityScore { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
    public ModerationDetails? Details { get; set; }
    public List<string> SuggestedCorrections { get; set; } = new();
}

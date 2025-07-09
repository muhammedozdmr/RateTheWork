namespace RateTheWork.Domain.Entities;

/// <summary>
/// Rozet ilerleme bilgisi
/// </summary>
public class BadgeProgress
{
    public string BadgeId { get; set; } = string.Empty;
    public string BadgeName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public string NextRequirement { get; set; } = string.Empty;
    public bool IsEarned { get; set; }
}

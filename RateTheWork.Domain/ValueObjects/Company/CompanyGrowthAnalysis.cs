namespace RateTheWork.Domain.ValueObjects.Company;

/// <summary>
/// Şirket büyüme analizi
/// </summary>
public class CompanyGrowthAnalysis
{
    public decimal ReviewGrowthRate { get; set; }
    public decimal RatingTrend { get; set; }
    public Dictionary<string, decimal> CategoryGrowthRates { get; set; } = new();
    public List<string> GrowthIndicators { get; set; } = new();
    public string GrowthPhase { get; set; } = string.Empty;
}

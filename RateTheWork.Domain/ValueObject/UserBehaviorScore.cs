namespace RateTheWork.Domain.ValueObject;

/// <summary>
/// Kullanıcı davranış skoru
/// </summary>
public class UserBehaviorScore
{
    public decimal OverallScore { get; set; }
    public decimal ConsistencyScore { get; set; }
    public decimal ObjectivityScore { get; set; }
    public decimal EngagementScore { get; set; }
    public decimal TrustworthinessScore { get; set; }
    public List<string> PositiveBehaviors { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

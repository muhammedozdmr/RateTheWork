using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.User;

public sealed class UserBehaviorScore : ValueObject
{
    private UserBehaviorScore
    (
        decimal overallScore
        , decimal consistencyScore
        , decimal objectivityScore
        , decimal engagementScore
        , decimal trustworthinessScore
        , decimal activityScore
        , decimal qualityScore
        , List<string> positiveBehaviors
        , List<string> improvementAreas
    )
    {
        OverallScore = overallScore;
        ConsistencyScore = consistencyScore;
        ObjectivityScore = objectivityScore;
        EngagementScore = engagementScore;
        TrustworthinessScore = trustworthinessScore;
        ActivityScore = activityScore;
        QualityScore = qualityScore;
        PositiveBehaviors = positiveBehaviors ?? new List<string>();
        ImprovementAreas = improvementAreas ?? new List<string>();
    }

    public decimal OverallScore { get; }
    public decimal ConsistencyScore { get; }
    public decimal ObjectivityScore { get; }
    public decimal EngagementScore { get; }
    public decimal TrustworthinessScore { get; }
    public decimal ActivityScore { get; }
    public decimal QualityScore { get; }
    public List<string> PositiveBehaviors { get; }
    public List<string> ImprovementAreas { get; }

    public static UserBehaviorScore Create
    (
        decimal overallScore
        , decimal consistencyScore
        , decimal objectivityScore
        , decimal engagementScore
        , decimal trustworthinessScore
        , decimal activityScore
        , decimal qualityScore
        , List<string> positiveBehaviors
        , List<string> improvementAreas
    )
    {
        return new UserBehaviorScore(overallScore, consistencyScore, objectivityScore, engagementScore
            , trustworthinessScore, activityScore, qualityScore, positiveBehaviors, improvementAreas);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OverallScore;
        yield return ConsistencyScore;
        yield return ObjectivityScore;
        yield return EngagementScore;
        yield return TrustworthinessScore;
        yield return ActivityScore;
        yield return QualityScore;
        yield return string.Join(",", PositiveBehaviors);
        yield return string.Join(",", ImprovementAreas);
    }
}

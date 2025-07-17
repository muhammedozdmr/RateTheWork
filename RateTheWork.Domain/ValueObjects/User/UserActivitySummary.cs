using RateTheWork.Domain.ValueObjects.Common;

namespace RateTheWork.Domain.ValueObjects.User;

public sealed class UserActivitySummary : ValueObject
{
    private UserActivitySummary
    (
        int totalReviews
        , int verifiedReviews
        , int helpfulVotes
        , int unhelpfulVotes
        , decimal averageRating
        , List<string> mostReviewedSectors
        , Dictionary<DateTime, int> activityByMonth
        , int consecutiveActiveDays
    )
    {
        TotalReviews = totalReviews;
        VerifiedReviews = verifiedReviews;
        HelpfulVotes = helpfulVotes;
        UnhelpfulVotes = unhelpfulVotes;
        AverageRating = averageRating;
        MostReviewedSectors = mostReviewedSectors ?? new List<string>();
        ActivityByMonth = activityByMonth ?? new Dictionary<DateTime, int>();
        ConsecutiveActiveDays = consecutiveActiveDays;
    }

    public int TotalReviews { get; }
    public int VerifiedReviews { get; }
    public int HelpfulVotes { get; }
    public int UnhelpfulVotes { get; }
    public decimal AverageRating { get; }
    public List<string> MostReviewedSectors { get; }
    public Dictionary<DateTime, int> ActivityByMonth { get; }
    public int ConsecutiveActiveDays { get; }

    public static UserActivitySummary Create
    (
        int totalReviews
        , int verifiedReviews
        , int helpfulVotes
        , int unhelpfulVotes
        , decimal averageRating
        , List<string> mostReviewedSectors
        , Dictionary<DateTime, int> activityByMonth
        , int consecutiveActiveDays
    )
    {
        return new UserActivitySummary(totalReviews, verifiedReviews, helpfulVotes, unhelpfulVotes, averageRating
            , mostReviewedSectors, activityByMonth, consecutiveActiveDays);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalReviews;
        yield return VerifiedReviews;
        yield return HelpfulVotes;
        yield return UnhelpfulVotes;
        yield return AverageRating;
        yield return string.Join(",", MostReviewedSectors);
        yield return ConsecutiveActiveDays;
    }
}

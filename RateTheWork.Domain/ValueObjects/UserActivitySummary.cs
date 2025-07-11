namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// Kullanıcı aktivite özeti
/// </summary>
public class UserActivitySummary
{
    public int TotalReviews { get; set; }
    public int VerifiedReviews { get; set; }
    public int HelpfulVotes { get; set; }
    public int UnhelpfulVotes { get; set; }
    public decimal AverageRating { get; set; }
    public List<string> MostReviewedSectors { get; set; } = new();
    public Dictionary<DateTime, int> ActivityByMonth { get; set; } = new();
    public int ConsecutiveActiveDays { get; set; }
}

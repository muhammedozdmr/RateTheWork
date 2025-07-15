namespace RateTheWork.Domain.ValueObjects;

public class CompanyReviewStatistics
{
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public int VerifiedReviews { get; set; }
    public DateTime? LastReviewDate { get; set; }
}

namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Değerlendirme limiti aşıldı exception'ı
/// </summary>
public class ReviewLimitExceededException : DomainException
{
    public int CurrentReviewCount { get; }
    public int MaxAllowedReviews { get; }
    public TimeSpan Period { get; }

    public ReviewLimitExceededException(int currentCount, int maxAllowed, TimeSpan period)
        : base($"Review limit exceeded. You have already submitted {currentCount} reviews in the last {period.TotalDays} days. Maximum allowed: {maxAllowed}.")
    {
        CurrentReviewCount = currentCount;
        MaxAllowedReviews = maxAllowed;
        Period = period;
    }
}

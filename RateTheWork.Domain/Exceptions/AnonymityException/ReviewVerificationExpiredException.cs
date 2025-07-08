namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Değerlendirme doğrulama süresi doldu exception'ı
/// </summary>
public class ReviewVerificationExpiredException : DomainException
{
    public Guid ReviewId { get; }
    public DateTime ExpiredAt { get; }

    public ReviewVerificationExpiredException(Guid reviewId, DateTime expiredAt)
        : base($"Review verification period has expired at {expiredAt:yyyy-MM-dd HH:mm}.")
    {
        ReviewId = reviewId;
        ExpiredAt = expiredAt;
    }
}

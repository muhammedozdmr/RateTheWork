namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Değerlendirme düzenleme limiti aşıldı exception'ı
/// </summary>
public class ReviewEditLimitExceededException : DomainException
{
    public Guid ReviewId { get; }
    public int EditCount { get; }
    public int MaxEditCount { get; }

    public ReviewEditLimitExceededException(Guid reviewId, int editCount, int maxEditCount)
        : base($"Review edit limit exceeded. This review has been edited {editCount} times. Maximum allowed: {maxEditCount}.")
    {
        ReviewId = reviewId;
        EditCount = editCount;
        MaxEditCount = maxEditCount;
    }
}

namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Geçersiz değerlendirme skoru exception'ı
/// </summary>
public class InvalidReviewScoreException : DomainException
{
    public decimal ProvidedScore { get; }
    public decimal MinScore { get; }
    public decimal MaxScore { get; }
    public string CategoryName { get; }

    public InvalidReviewScoreException(string categoryName, decimal providedScore, decimal minScore, decimal maxScore)
        : base($"Invalid score {providedScore} for category '{categoryName}'. Score must be between {minScore} and {maxScore}.")
    {
        CategoryName = categoryName;
        ProvidedScore = providedScore;
        MinScore = minScore;
        MaxScore = maxScore;
    }
}


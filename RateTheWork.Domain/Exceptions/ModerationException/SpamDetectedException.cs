namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Spam tespit edildi exception'ı
/// </summary>
public class SpamDetectedException : DomainException
{
    public string ContentType { get; }
    public decimal SpamScore { get; }
    public string[] SpamIndicators { get; }

    public SpamDetectedException(string contentType, decimal spamScore, string[] spamIndicators)
        : base($"Spam detected in {contentType}. Spam score: {spamScore}/100. Indicators: {string.Join(", ", spamIndicators)}")
    {
        ContentType = contentType;
        SpamScore = spamScore;
        SpamIndicators = spamIndicators;
    }
}


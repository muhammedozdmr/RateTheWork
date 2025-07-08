namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Uygunsuz içerik tespit edildi exception'ı
/// </summary>
public class InappropriateContentException : DomainException
{
    public string ContentType { get; }
    public string[] DetectedIssues { get; }
    public decimal ConfidenceScore { get; }

    public InappropriateContentException(string contentType, string[] detectedIssues, decimal confidenceScore)
        : base($"Inappropriate content detected in {contentType}. Issues: {string.Join(", ", detectedIssues)}.")
    {
        ContentType = contentType;
        DetectedIssues = detectedIssues;
        ConfidenceScore = confidenceScore;
    }
}

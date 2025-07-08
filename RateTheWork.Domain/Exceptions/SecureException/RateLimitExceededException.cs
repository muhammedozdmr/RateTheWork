namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Rate limiting exception'ı
/// </summary>
public class RateLimitExceededException : DomainException
{
    public string LimitType { get; }
    public int CurrentAttempts { get; }
    public int MaxAttempts { get; }
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(string limitType, int currentAttempts, int maxAttempts, TimeSpan retryAfter)
        : base($"Rate limit exceeded for '{limitType}'. Current attempts: {currentAttempts}, Max allowed: {maxAttempts}. Retry after: {retryAfter.TotalSeconds} seconds.")
    {
        LimitType = limitType;
        CurrentAttempts = currentAttempts;
        MaxAttempts = maxAttempts;
        RetryAfter = retryAfter;
    }
}

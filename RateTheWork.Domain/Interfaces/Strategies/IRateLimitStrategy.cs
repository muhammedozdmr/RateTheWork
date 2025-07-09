namespace RateTheWork.Domain.Interfaces.Strategies;

/// <summary>
/// Rate limiting strategy interface'i
/// </summary>
public interface IRateLimitStrategy
{
    /// <summary>
    /// Rate limit kontrolü
    /// </summary>
    Task<(bool IsAllowed, TimeSpan? RetryAfter)> CheckRateLimitAsync(
        string identifier, 
        string action);
    
    /// <summary>
    /// Rate limit ayarları
    /// </summary>
    RateLimitSettings GetSettings(string action);
    
    /// <summary>
    /// Kalan deneme hakkı
    /// </summary>
    Task<int> GetRemainingAttemptsAsync(string identifier, string action);
}

/// <summary>
/// Rate limit ayarları
/// </summary>
public record RateLimitSettings
{
    public int MaxAttempts { get; init; }
    public TimeSpan Period { get; init; }
    public bool SlidingWindow { get; init; }
}


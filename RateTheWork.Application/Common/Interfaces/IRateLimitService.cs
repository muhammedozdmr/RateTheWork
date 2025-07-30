namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Rate limiting service interface
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Check if request is allowed
    /// </summary>
    Task<RateLimitResult> IsAllowedAsync
        (string key, RateLimitOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current usage statistics
    /// </summary>
    Task<RateLimitStatistics> GetStatisticsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset rate limit for a key
    /// </summary>
    Task ResetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset all rate limits
    /// </summary>
    Task ResetAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Rate limit options
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Maximum number of requests
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Time window for the limit
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Rate limit algorithm
    /// </summary>
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;

    /// <summary>
    /// Enable distributed rate limiting
    /// </summary>
    public bool Distributed { get; set; } = true;

    /// <summary>
    /// Custom error message
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Rate limit algorithm
/// </summary>
public enum RateLimitAlgorithm
{
    /// <summary>
    /// Fixed window algorithm
    /// </summary>
    FixedWindow

    ,

    /// <summary>
    /// Sliding window algorithm
    /// </summary>
    SlidingWindow

    ,

    /// <summary>
    /// Token bucket algorithm
    /// </summary>
    TokenBucket

    ,

    /// <summary>
    /// Leaky bucket algorithm
    /// </summary>
    LeakyBucket
}

/// <summary>
/// Rate limit statistics
/// </summary>
public class RateLimitStatistics
{
    public string Key { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int AllowedRequests { get; set; }
    public int BlockedRequests { get; set; }
    public DateTime FirstRequestAt { get; set; }
    public DateTime LastRequestAt { get; set; }
    public Dictionary<string, int> HourlyRequests { get; set; } = new();
}

/// <summary>
/// Rate limit attribute for declarative rate limiting
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RateLimitAttribute : Attribute
{
    public RateLimitAttribute(string key, int limit = 100, int windowSeconds = 60)
    {
        Key = key;
        Limit = limit;
        WindowSeconds = windowSeconds;
        Algorithm = RateLimitAlgorithm.SlidingWindow;
    }

    public string Key { get; set; }
    public int Limit { get; set; }
    public int WindowSeconds { get; set; }
    public RateLimitAlgorithm Algorithm { get; set; }
}

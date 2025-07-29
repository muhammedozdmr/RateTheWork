namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Rate limiting servisi
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Rate limit kontrolü yapar
    /// </summary>
    /// <param name="key">Unique key</param>
    /// <param name="limit">Limit sayısı</param>
    /// <param name="period">Zaman periyodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Rate limit sonucu</returns>
    Task<RateLimitResult> CheckRateLimitAsync
    (
        string key
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sliding window rate limiting
    /// </summary>
    Task<RateLimitResult> CheckSlidingWindowRateLimitAsync
    (
        string key
        , int limit
        , TimeSpan window
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Rate limit sayacını sıfırlar
    /// </summary>
    Task ResetRateLimitAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// IP bazlı rate limit kontrolü
    /// </summary>
    Task<RateLimitResult> CheckIpRateLimitAsync
    (
        string ipAddress
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Kullanıcı bazlı rate limit kontrolü
    /// </summary>
    Task<RateLimitResult> CheckUserRateLimitAsync
    (
        Guid userId
        , string action
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Rate limit sonucu
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTimeOffset ResetAt { get; set; }
}

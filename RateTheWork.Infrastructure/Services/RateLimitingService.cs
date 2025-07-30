using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

/// <summary>
/// Rate limiting servisi
/// API çağrılarını sınırlamak için kullanılır
/// </summary>
public class RateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService
    (
        IDistributedCache cache
        , ILogger<RateLimitingService> logger
    )
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Rate limit sayacını sıfırlar
    /// </summary>
    public async Task ResetRateLimitAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"ratelimit:{key}";
            var slidingCacheKey = $"ratelimit:sliding:{key}";

            await _cache.RemoveAsync(cacheKey, cancellationToken);
            await _cache.RemoveAsync(slidingCacheKey, cancellationToken);

            _logger.LogInformation("Rate limit sıfırlandı: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limit sıfırlama hatası: {Key}", key);
        }
    }

    /// <summary>
    /// Rate limit kontrolü yapar
    /// </summary>
    public async Task<RateLimitResult> CheckRateLimitAsync
    (
        string key
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    )
    {
        var cacheKey = $"ratelimit:{key}";
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.Subtract(period);

        try
        {
            // Mevcut sayacı al
            var countStr = await _cache.GetStringAsync(cacheKey, cancellationToken);
            var currentCount = 0;

            if (!string.IsNullOrEmpty(countStr))
            {
                if (int.TryParse(countStr, out var count))
                {
                    currentCount = count;
                }
            }

            // Limit kontrolü
            if (currentCount >= limit)
            {
                _logger.LogWarning("Rate limit aşıldı: {Key}, Limit: {Limit}, Mevcut: {Count}",
                    key, limit, currentCount);

                return new RateLimitResult
                {
                    IsAllowed = false, Limit = limit, Remaining = 0, ResetAt = now.Add(period)
                };
            }

            // Sayacı artır
            currentCount++;
            await _cache.SetStringAsync(
                cacheKey,
                currentCount.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = period
                },
                cancellationToken);

            return new RateLimitResult
            {
                IsAllowed = true, Limit = limit, Remaining = limit - currentCount, ResetAt = now.Add(period)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limit kontrolü sırasında hata: {Key}", key);

            // Hata durumunda güvenli tarafta kal ve izin ver
            return new RateLimitResult
            {
                IsAllowed = true, Limit = limit, Remaining = limit, ResetAt = now.Add(period)
            };
        }
    }

    /// <summary>
    /// Sliding window rate limiting
    /// </summary>
    public async Task<RateLimitResult> CheckSlidingWindowRateLimitAsync
    (
        string key
        , int limit
        , TimeSpan window
        , CancellationToken cancellationToken = default
    )
    {
        var cacheKey = $"ratelimit:sliding:{key}";
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.Subtract(window);

        try
        {
            // Zaman damgalarını sakla
            var timestampsJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
            var timestamps = new List<DateTimeOffset>();

            if (!string.IsNullOrEmpty(timestampsJson))
            {
                timestamps = JsonSerializer.Deserialize<List<DateTimeOffset>>(timestampsJson)
                             ?? new List<DateTimeOffset>();
            }

            // Eski zaman damgalarını temizle
            timestamps = timestamps.Where(t => t > windowStart).ToList();

            // Limit kontrolü
            if (timestamps.Count >= limit)
            {
                var oldestTimestamp = timestamps.Min();
                var resetAt = oldestTimestamp.Add(window);

                return new RateLimitResult
                {
                    IsAllowed = false, Limit = limit, Remaining = 0, ResetAt = resetAt
                };
            }

            // Yeni zaman damgası ekle
            timestamps.Add(now);

            // Cache'e kaydet
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(timestamps),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = window
                },
                cancellationToken);

            return new RateLimitResult
            {
                IsAllowed = true, Limit = limit, Remaining = limit - timestamps.Count, ResetAt = now.Add(window)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sliding window rate limit kontrolü sırasında hata: {Key}", key);

            return new RateLimitResult
            {
                IsAllowed = true, Limit = limit, Remaining = limit, ResetAt = now.Add(window)
            };
        }
    }

    /// <summary>
    /// IP bazlı rate limit kontrolü
    /// </summary>
    public async Task<RateLimitResult> CheckIpRateLimitAsync
    (
        string ipAddress
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    )
    {
        return await CheckRateLimitAsync($"ip:{ipAddress}", limit, period, cancellationToken);
    }

    /// <summary>
    /// Kullanıcı bazlı rate limit kontrolü
    /// </summary>
    public async Task<RateLimitResult> CheckUserRateLimitAsync
    (
        Guid userId
        , string action
        , int limit
        , TimeSpan period
        , CancellationToken cancellationToken = default
    )
    {
        return await CheckRateLimitAsync($"user:{userId}:{action}", limit, period, cancellationToken);
    }
}

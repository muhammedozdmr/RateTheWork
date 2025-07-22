using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Application.Common.Behaviors;

/// <summary>
/// Caching pipeline behavior
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle
    (
        TRequest request
        , RequestHandlerDelegate<TResponse> next
        , CancellationToken cancellationToken
    )
    {
        // Check if request is cacheable
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = GenerateCacheKey(request, cacheableQuery.CacheKey);

        // Try to get from cache
        var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cachedResponse;
        }

        // Execute request
        var response = await next();

        // Cache the response
        if (response != null)
        {
            await _cache.SetAsync(cacheKey, response, cacheableQuery.CacheExpiration, cancellationToken);
            _logger.LogInformation("Cached response for {CacheKey} with expiration {Expiration}",
                cacheKey, cacheableQuery.CacheExpiration);
        }

        return response;
    }

    private static string GenerateCacheKey(TRequest request, string? baseKey)
    {
        var requestName = request.GetType().Name;

        if (!string.IsNullOrWhiteSpace(baseKey))
        {
            return $"{requestName}:{baseKey}";
        }

        // Generate cache key from request properties
        var requestJson = JsonSerializer.Serialize(request);
        var requestHash = requestJson.GetHashCode();

        return $"{requestName}:{requestHash}";
    }
}

/// <summary>
/// Interface for cacheable queries
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Cache key
    /// </summary>
    string? CacheKey { get; }

    /// <summary>
    /// Cache expiration time
    /// </summary>
    TimeSpan? CacheExpiration { get; }
}

/// <summary>
/// Cache invalidation interface
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Cache keys to invalidate
    /// </summary>
    string[] CacheKeysToInvalidate { get; }
}

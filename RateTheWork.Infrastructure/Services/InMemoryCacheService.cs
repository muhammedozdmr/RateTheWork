using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly HashSet<string> _keys = new();
    private readonly SemaphoreSlim _keysSemaphore = new(1, 1);
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheService
    (
        IMemoryCache memoryCache
        , ILogger<InMemoryCacheService> logger
    )
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public async Task<T> GetOrCreateAsync<T>
        (string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    public async Task SetAsync<T>
        (string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                options.SetSlidingExpiration(TimeSpan.FromMinutes(30));
            }

            options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                _keysSemaphore.Wait();
                try
                {
                    _keys.Remove(evictedKey.ToString()!);
                }
                finally
                {
                    _keysSemaphore.Release();
                }
            });

            _memoryCache.Set(key, value, options);

            await _keysSemaphore.WaitAsync(cancellationToken);
            try
            {
                _keys.Add(key);
            }
            finally
            {
                _keysSemaphore.Release();
            }

            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            throw;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);

            _keysSemaphore.Wait();
            try
            {
                _keys.Remove(key);
            }
            finally
            {
                _keysSemaphore.Release();
            }

            _logger.LogDebug("Cache removed for key: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        await _keysSemaphore.WaitAsync(cancellationToken);
        try
        {
            var keysToRemove = _keys.Where(k => k.Contains(pattern)).ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.Remove(key);
            }

            _logger.LogDebug("Cache removed {Count} keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        finally
        {
            _keysSemaphore.Release();
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _keysSemaphore.WaitAsync(cancellationToken);
        try
        {
            foreach (var key in _keys.ToList())
            {
                _memoryCache.Remove(key);
            }

            _keys.Clear();

            _logger.LogWarning("All cache entries have been cleared");
        }
        finally
        {
            _keysSemaphore.Release();
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key, cancellationToken);
        }
    }

    public Task<bool> RefreshAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out var value))
        {
            _memoryCache.Set(key, value, expiry);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}

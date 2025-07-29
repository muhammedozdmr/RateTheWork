using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

/// <summary>
/// Dağıtık sistemlerde senkronizasyon için lock servisi
/// Redis varsa Redis üzerinden, yoksa in-memory cache üzerinden çalışır
/// </summary>
public class DistributedLockService : IDistributedLockService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _defaultLockTimeout = TimeSpan.FromMinutes(5);
    private readonly ILogger<DistributedLockService> _logger;

    public DistributedLockService
    (
        IDistributedCache cache
        , ILogger<DistributedLockService> logger
    )
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen key için lock almaya çalışır
    /// </summary>
    public async Task<IDistributedLock?> AcquireLockAsync
    (
        string key
        , TimeSpan? timeout = null
        , CancellationToken cancellationToken = default
    )
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var lockTimeout = timeout ?? _defaultLockTimeout;

        try
        {
            // Kilidi almaya çalış
            var existingValue = await _cache.GetStringAsync(lockKey, cancellationToken);
            if (!string.IsNullOrEmpty(existingValue))
            {
                // Kilit zaten alınmış
                return null;
            }

            // Kilidi ayarla
            await _cache.SetStringAsync(
                lockKey,
                lockValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = lockTimeout
                },
                cancellationToken);

            // Kilidin başarıyla alındığını doğrula
            var confirmedValue = await _cache.GetStringAsync(lockKey, cancellationToken);
            if (confirmedValue == lockValue)
            {
                _logger.LogDebug("Lock acquired for key: {Key}", key);
                return new DistributedLock(lockKey, lockValue, this);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Lock bırakılıncaya kadar bekler ve lock'u alır
    /// </summary>
    public async Task<IDistributedLock> WaitForLockAsync
    (
        string key
        , TimeSpan? timeout = null
        , TimeSpan? waitTimeout = null
        , CancellationToken cancellationToken = default
    )
    {
        var waitTime = waitTimeout ?? TimeSpan.FromMinutes(1);
        var endTime = DateTime.UtcNow.Add(waitTime);

        while (DateTime.UtcNow < endTime)
        {
            var lockResult = await AcquireLockAsync(key, timeout, cancellationToken);
            if (lockResult != null)
            {
                return lockResult;
            }

            // Kısa bir süre bekle
            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Could not acquire lock for key '{key}' within timeout period");
    }

    /// <summary>
    /// Lock süresini uzatır
    /// </summary>
    public async Task<bool> ExtendLockAsync
    (
        string key
        , string lockValue
        , TimeSpan extension
        , CancellationToken cancellationToken = default
    )
    {
        var lockKey = $"lock:{key}";

        try
        {
            var currentValue = await _cache.GetStringAsync(lockKey, cancellationToken);
            if (currentValue == lockValue)
            {
                await _cache.SetStringAsync(
                    lockKey,
                    lockValue,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = extension
                    },
                    cancellationToken);

                _logger.LogDebug("Lock extended for key: {Key}", key);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending lock for key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Lock'ı serbest bırakır
    /// </summary>
    public async Task ReleaseLockAsync(string lockKey, string lockValue)
    {
        try
        {
            var currentValue = await _cache.GetStringAsync(lockKey);
            if (currentValue == lockValue)
            {
                await _cache.RemoveAsync(lockKey);
                _logger.LogDebug("Lock released for key: {Key}", lockKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for key: {Key}", lockKey);
        }
    }

    /// <summary>
    /// Lock implement classı
    /// </summary>
    private class DistributedLock : IDistributedLock
    {
        private readonly string _lockKey;
        private readonly DistributedLockService _lockService;
        private readonly string _lockValue;
        private bool _disposed;

        public DistributedLock(string lockKey, string lockValue, DistributedLockService lockService)
        {
            _lockKey = lockKey;
            _lockValue = lockValue;
            _lockService = lockService;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _lockService.ReleaseLockAsync(_lockKey, _lockValue);
                _disposed = true;
            }
        }
    }
}

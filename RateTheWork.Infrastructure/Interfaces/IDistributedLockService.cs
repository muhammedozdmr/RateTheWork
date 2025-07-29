namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Dağıtık kilit servisi arayüzü
/// Redis, ZooKeeper veya benzeri sistemlerle uygulanır.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Kilit alır
    /// </summary>
    Task<IDistributedLock?> AcquireLockAsync
        (string resource, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kilit almayı dener (engellemez)
    /// </summary>
    Task<IDistributedLock?> TryAcquireLockAsync
        (string resource, TimeSpan expiry, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kilidi serbest bırakır
    /// </summary>
    Task ReleaseLockAsync(IDistributedLock distributedLock);

    /// <summary>
    /// Kilit süresini uzatır
    /// </summary>
    Task<bool> ExtendLockAsync(IDistributedLock distributedLock, TimeSpan extension);
}

/// <summary>
/// Dağıtık kilit arayüzü
/// </summary>
public interface IDistributedLock : IDisposable
{
    string Resource { get; }
    string LockId { get; }
    DateTime AcquiredAt { get; }
    TimeSpan Duration { get; }
    bool IsAcquired { get; }
}

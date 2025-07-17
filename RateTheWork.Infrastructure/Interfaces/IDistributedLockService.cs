namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Distributed lock service interface'i
/// Redis, ZooKeeper veya benzeri sistemlerle implemente edilir.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Lock alır
    /// </summary>
    Task<IDistributedLock?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lock almayı dener (bloklamaz)
    /// </summary>
    Task<IDistributedLock?> TryAcquireLockAsync(string resource, TimeSpan expiry, TimeSpan timeout, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lock'ı serbest bırakır
    /// </summary>
    Task ReleaseLockAsync(IDistributedLock distributedLock);
    
    /// <summary>
    /// Lock süresini uzatır
    /// </summary>
    Task<bool> ExtendLockAsync(IDistributedLock distributedLock, TimeSpan extension);
}

/// <summary>
/// Distributed lock interface'i
/// </summary>
public interface IDistributedLock : IDisposable
{
    string Resource { get; }
    string LockId { get; }
    DateTime AcquiredAt { get; }
    TimeSpan Duration { get; }
    bool IsAcquired { get; }
}
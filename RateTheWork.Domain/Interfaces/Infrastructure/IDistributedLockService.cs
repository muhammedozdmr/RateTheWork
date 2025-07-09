using RateTheWork.Domain.Interfaces.Common;

namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Distributed lock interface'i
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Lock alır
    /// </summary>
    Task<IDistributedLock?> AcquireLockAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lock'ı serbest bırakır
    /// </summary>
    Task ReleaseLockAsync(IDistributedLock distributedLock);
}



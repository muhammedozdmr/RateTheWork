namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Dağıtık sistemlerde senkronizasyon için lock servisi
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Belirtilen key için lock almaya çalışır
    /// </summary>
    /// <param name="key">Lock key</param>
    /// <param name="timeout">Lock timeout süresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Lock alındıysa IDistributedLock instance'ı, alınamadıysa null</returns>
    Task<IDistributedLock?> AcquireLockAsync
    (
        string key
        , TimeSpan? timeout = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lock alınana kadar bekler
    /// </summary>
    /// <param name="key">Lock key</param>
    /// <param name="timeout">Lock timeout süresi</param>
    /// <param name="waitTimeout">Bekleme timeout süresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Lock instance'ı</returns>
    Task<IDistributedLock> WaitForLockAsync
    (
        string key
        , TimeSpan? timeout = null
        , TimeSpan? waitTimeout = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lock süresini uzatır
    /// </summary>
    /// <param name="key">Lock key</param>
    /// <param name="lockValue">Lock değeri</param>
    /// <param name="extension">Uzatma süresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> ExtendLockAsync
    (
        string key
        , string lockValue
        , TimeSpan extension
        , CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Distributed lock instance'ı
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
}

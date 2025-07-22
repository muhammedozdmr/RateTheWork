namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Cache servisi interface'i
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den veri okur
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'e veri yazar
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'den veri siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pattern'e göre cache temizler
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'de key var mı kontrol eder
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache'i tamamen temizler
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create pattern
    /// </summary>
    Task<T> GetOrCreateAsync<T>
        (string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Distributed cache servisi interface'i
/// </summary>
public interface IDistributedCacheService : ICacheService
{
    /// <summary>
    /// Lock mekanizması
    /// </summary>
    Task<bool> AcquireLockAsync
        (string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lock'u serbest bırakır
    /// </summary>
    Task<bool> ReleaseLockAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pub/Sub - Mesaj yayınlar
    /// </summary>
    Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pub/Sub - Kanal dinler
    /// </summary>
    Task SubscribeAsync<T>(string channel, Func<T, Task> handler, CancellationToken cancellationToken = default);
}

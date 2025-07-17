namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Cache service interface'i
/// Redis, Memcached veya In-Memory cache ile implemente edilir.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den veri okur
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'den veri okur veya yoksa factory metodunu çalıştırır
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'e veri yazar
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'den veri siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Pattern'e göre cache temizleme
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Birden fazla key'i aynı anda siler
    /// </summary>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'de key var mı kontrolü
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache süresi uzatma
    /// </summary>
    Task<bool> RefreshAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tüm cache'i temizler (dikkatli kullanılmalı)
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
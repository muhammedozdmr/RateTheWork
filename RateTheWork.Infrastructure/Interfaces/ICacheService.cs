namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Önbellek servisi arayüzü
/// Redis, Memcached veya Bellek içi önbellek ile uygulanır.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Önbellekten veri okur
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Önbellekten veri okur veya yoksa üretici metodunu çalıştırır
    /// </summary>
    Task<T> GetOrSetAsync<T>
        (string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Önbelleğe veri yazar
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Önbellekten veri siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desene göre önbellek temizleme
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla anahtarı aynı anda siler
    /// </summary>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Önbellekte anahtar var mı kontrolü
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Önbellek süresi uzatma
    /// </summary>
    Task<bool> RefreshAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm önbelleği temizler (dikkatli kullanılmalı)
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}

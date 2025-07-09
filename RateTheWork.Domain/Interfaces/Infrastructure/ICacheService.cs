namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Cache service interface'i
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
    /// Cache'de key var mı kontrolü
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache süresi uzatma
    /// </summary>
    Task<bool> RefreshAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}

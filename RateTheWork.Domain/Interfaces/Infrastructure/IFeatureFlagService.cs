namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Feature flag service interface'i
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Feature aktif mi kontrolü
    /// </summary>
    Task<bool> IsEnabledAsync(string feature, string? userId = null);
    
    /// <summary>
    /// Feature variant'ını getirir
    /// </summary>
    Task<T?> GetVariantAsync<T>(string feature, string? userId = null);
    
    /// <summary>
    /// Tüm aktif feature'ları getirir
    /// </summary>
    Task<Dictionary<string, bool>> GetAllFeaturesAsync(string? userId = null);
}

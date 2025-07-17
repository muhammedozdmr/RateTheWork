namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Feature flag service interface'i
/// LaunchDarkly, ConfigCat veya custom implementation ile kullanılır.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Feature aktif mi kontrolü
    /// </summary>
    Task<bool> IsEnabledAsync(string feature, string? userId = null);
    
    /// <summary>
    /// Feature aktif mi kontrolü (context ile)
    /// </summary>
    Task<bool> IsEnabledAsync(string feature, FeatureContext context);
    
    /// <summary>
    /// Feature variant'ını getirir
    /// </summary>
    Task<T?> GetVariantAsync<T>(string feature, string? userId = null);
    
    /// <summary>
    /// Feature variant'ını getirir (context ile)
    /// </summary>
    Task<T?> GetVariantAsync<T>(string feature, FeatureContext context);
    
    /// <summary>
    /// Tüm aktif feature'ları getirir
    /// </summary>
    Task<Dictionary<string, bool>> GetAllFeaturesAsync(string? userId = null);
    
    /// <summary>
    /// Feature evaluation detaylarını getirir
    /// </summary>
    Task<FeatureEvaluation> EvaluateAsync(string feature, FeatureContext context);
}

/// <summary>
/// Feature flag context
/// </summary>
public class FeatureContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}

/// <summary>
/// Feature evaluation sonucu
/// </summary>
public class FeatureEvaluation
{
    public string Feature { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public object? Variant { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
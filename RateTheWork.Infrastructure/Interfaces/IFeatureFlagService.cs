namespace RateTheWork.Infrastructure.Interfaces;

/// <summary>
/// Özellik bayrağı servisi arayüzü
/// LaunchDarkly, ConfigCat veya özel uygulama ile kullanılır.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Özellik aktif mi kontrolü
    /// </summary>
    Task<bool> IsEnabledAsync(string feature, string? userId = null);

    /// <summary>
    /// Özellik aktif mi kontrolü (bağlam ile)
    /// </summary>
    Task<bool> IsEnabledAsync(string feature, FeatureContext context);

    /// <summary>
    /// Özellik varyantını getirir
    /// </summary>
    Task<T?> GetVariantAsync<T>(string feature, string? userId = null);

    /// <summary>
    /// Özellik varyantını getirir (bağlam ile)
    /// </summary>
    Task<T?> GetVariantAsync<T>(string feature, FeatureContext context);

    /// <summary>
    /// Tüm aktif özellikleri getirir
    /// </summary>
    Task<Dictionary<string, bool>> GetAllFeaturesAsync(string? userId = null);

    /// <summary>
    /// Özellik değerlendirme detaylarını getirir
    /// </summary>
    Task<FeatureEvaluation> EvaluateAsync(string feature, FeatureContext context);
}

/// <summary>
/// Özellik bayrağı bağlamı
/// </summary>
public class FeatureContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}

/// <summary>
/// Özellik değerlendirme sonucu
/// </summary>
public class FeatureEvaluation
{
    public string Feature { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public object? Variant { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

/// <summary>
/// Feature flag yönetimi servisi
/// Özellik açıp kapama, A/B testing ve kademeli yayınlama için kullanılır
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private const string CacheKeyPrefix = "featureflag:";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly ISecretService _secretService;

    public FeatureFlagService
    (
        IConfiguration configuration
        , ICacheService cacheService
        , ISecretService secretService
        , ILogger<FeatureFlagService> logger
    )
    {
        _configuration = configuration;
        _cacheService = cacheService;
        _secretService = secretService;
        _logger = logger;
    }

    /// <summary>
    /// Feature flag'in aktif olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsEnabledAsync(string featureName, string? userId = null)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{featureName}";

            // Önce önbelleğe bak
            var cachedValue = await _cacheService.GetAsync<bool?>(cacheKey);
            if (cachedValue.HasValue)
            {
                return cachedValue.Value;
            }

            // Önce Cloudflare KV'den kontrol et (üretim için)
            var kvValue = await _secretService.GetSecretAsync($"feature_{featureName}");
            if (!string.IsNullOrEmpty(kvValue))
            {
                var isEnabled = bool.Parse(kvValue);
                await _cacheService.SetAsync(cacheKey, isEnabled, _cacheExpiration);
                return isEnabled;
            }

            // Sonra yapılandırmadan kontrol et
            var configValue = _configuration[$"FeatureFlags:{featureName}"];
            if (!string.IsNullOrEmpty(configValue))
            {
                var isEnabled = bool.Parse(configValue);
                await _cacheService.SetAsync(cacheKey, isEnabled, _cacheExpiration);
                return isEnabled;
            }

            // Varsayılan olarak kapalı
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag: {FeatureName}", featureName);
            return false; // Hata durumunda güvenli tarafı seç
        }
    }

    /// <summary>
    /// Feature flag'in belirli bir kullanıcı için aktif olup olmadığını kontrol eder
    /// A/B testing için kullanılır
    /// </summary>
    public async Task<bool> IsEnabledForUserAsync(string featureName, string userId)
    {
        try
        {
            // Önce genel bayrağı kontrol et
            var isGloballyEnabled = await IsEnabledAsync(featureName);
            if (!isGloballyEnabled)
            {
                return false;
            }

            // Kullanıcı bazlı geçersiz kılma kontrolü
            var userOverrideKey = $"feature_{featureName}_user_{userId}";
            var userOverride = await _secretService.GetSecretAsync(userOverrideKey);
            if (!string.IsNullOrEmpty(userOverride))
            {
                return bool.Parse(userOverride);
            }

            // Yüzdelik dağılım kontrolü
            var percentageKey = $"feature_{featureName}_percentage";
            var percentageStr = await _secretService.GetSecretAsync(percentageKey);
            if (!string.IsNullOrEmpty(percentageStr) && int.TryParse(percentageStr, out var percentage))
            {
                // Kullanıcı kimliğine göre tutarlı özet hesapla
                var hash = userId.GetHashCode();
                var bucket = Math.Abs(hash) % 100;
                return bucket < percentage;
            }

            return true; // Genel olarak açıksa ve özel kural yoksa açık
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag for user: {FeatureName}, {UserId}", featureName, userId);
            return false;
        }
    }

    /// <summary>
    /// Tüm feature flag'leri listeler
    /// </summary>
    public async Task<Dictionary<string, bool>> GetAllFlagsAsync()
    {
        var flags = new Dictionary<string, bool>();

        try
        {
            // Yapılandırmadan oku
            var featureFlagsSection = _configuration.GetSection("FeatureFlags");
            foreach (var flag in featureFlagsSection.GetChildren())
            {
                if (bool.TryParse(flag.Value, out var isEnabled))
                {
                    flags[flag.Key] = isEnabled;
                }
            }

            // Cloudflare KV'den oku ve üzerine yaz
            // Bu kısım üretimde tüm bayrakları listelemek için özel bir uç nokta gerektirebilir

            return flags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all feature flags");
            return flags;
        }
    }

    /// <summary>
    /// Feature flag'i günceller
    /// </summary>
    public async Task<bool> SetFlagAsync(string featureName, bool isEnabled)
    {
        try
        {
            // Cloudflare KV'ye yaz
            await _secretService.SetSecretAsync($"feature_{featureName}", isEnabled.ToString());

            // Önbelleği temizle
            var cacheKey = $"{CacheKeyPrefix}{featureName}";
            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Feature flag updated: {FeatureName} = {IsEnabled}", featureName, isEnabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature flag: {FeatureName}", featureName);
            return false;
        }
    }

    /// <summary>
    /// Kullanıcı bazlı override ekler
    /// </summary>
    public async Task<bool> SetUserOverrideAsync(string featureName, string userId, bool isEnabled)
    {
        try
        {
            var userOverrideKey = $"feature_{featureName}_user_{userId}";
            await _secretService.SetSecretAsync(userOverrideKey, isEnabled.ToString());

            _logger.LogInformation("User override set: {FeatureName}, {UserId} = {IsEnabled}",
                featureName, userId, isEnabled);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user override: {FeatureName}, {UserId}",
                featureName, userId);
            return false;
        }
    }

    /// <summary>
    /// Yüzdelik dağılım ayarlar
    /// </summary>
    public async Task<bool> SetPercentageAsync(string featureName, int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percentage));
        }

        try
        {
            var percentageKey = $"feature_{featureName}_percentage";
            await _secretService.SetSecretAsync(percentageKey, percentage.ToString());

            _logger.LogInformation("Feature percentage set: {FeatureName} = {Percentage}%",
                featureName, percentage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting feature percentage: {FeatureName}", featureName);
            return false;
        }
    }
}

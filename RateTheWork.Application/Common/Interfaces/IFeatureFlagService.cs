namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Feature flag yönetimi servisi
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Feature flag'in aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="featureName">Özellik adı</param>
    /// <param name="userId">Kullanıcı ID (opsiyonel)</param>
    /// <returns>Aktif ise true</returns>
    Task<bool> IsEnabledAsync(string featureName, string? userId = null);

    /// <summary>
    /// Feature flag'in belirli bir kullanıcı için aktif olup olmadığını kontrol eder
    /// </summary>
    /// <param name="featureName">Özellik adı</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Aktif ise true</returns>
    Task<bool> IsEnabledForUserAsync(string featureName, string userId);

    /// <summary>
    /// Tüm feature flag'leri listeler
    /// </summary>
    /// <returns>Feature flag listesi</returns>
    Task<Dictionary<string, bool>> GetAllFlagsAsync();

    /// <summary>
    /// Feature flag'i günceller
    /// </summary>
    /// <param name="featureName">Özellik adı</param>
    /// <param name="isEnabled">Aktif durumu</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> SetFlagAsync(string featureName, bool isEnabled);

    /// <summary>
    /// Kullanıcı bazlı override ekler
    /// </summary>
    /// <param name="featureName">Özellik adı</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="isEnabled">Aktif durumu</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> SetUserOverrideAsync(string featureName, string userId, bool isEnabled);

    /// <summary>
    /// Yüzdelik dağılım ayarlar
    /// </summary>
    /// <param name="featureName">Özellik adı</param>
    /// <param name="percentage">Yüzde (0-100)</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> SetPercentageAsync(string featureName, int percentage);
}

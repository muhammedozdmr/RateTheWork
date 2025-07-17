namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Lokalizasyon service interface'i
/// Infrastructure katmanında resource dosyaları veya veritabanı ile implemente edilir.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Çeviri getirir
    /// </summary>
    string GetString(string key, string? culture = null);

    /// <summary>
    /// Parametreli çeviri
    /// </summary>
    string GetString(string key, object[] args, string? culture = null);

    /// <summary>
    /// Çeviri varsa getirir, yoksa null döner
    /// </summary>
    string? GetStringOrNull(string key, string? culture = null);

    /// <summary>
    /// Tüm çevirileri getirir
    /// </summary>
    Dictionary<string, string> GetAllStrings(string? culture = null);

    /// <summary>
    /// Belirli bir prefix ile başlayan tüm çevirileri getirir
    /// </summary>
    Dictionary<string, string> GetStringsWithPrefix(string prefix, string? culture = null);

    /// <summary>
    /// Desteklenen dilleri getirir
    /// </summary>
    IEnumerable<string> GetSupportedCultures();

    /// <summary>
    /// Varsayılan dili getirir
    /// </summary>
    string GetDefaultCulture();

    /// <summary>
    /// Mevcut dili getirir
    /// </summary>
    string GetCurrentCulture();

    /// <summary>
    /// Dil değiştirir
    /// </summary>
    void SetCulture(string culture);
}

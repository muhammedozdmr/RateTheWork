namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Lokalizasyon service interface'i
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
    /// Desteklenen dilleri getirir
    /// </summary>
    IEnumerable<string> GetSupportedCultures();
    
    /// <summary>
    /// Varsayılan dili getirir
    /// </summary>
    string GetDefaultCulture();
}

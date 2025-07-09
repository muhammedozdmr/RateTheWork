namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// Anonimlik seviyesi strategy interface'i
/// </summary>
public interface IAnonymityLevelStrategy
{
    /// <summary>
    /// Anonimlik seviyesi belirleme
    /// </summary>
    AnonymityLevel DetermineAnonymityLevel(string userId, string companyId, string reviewType);
    
    /// <summary>
    /// Görüntülenebilir bilgiler
    /// </summary>
    string[] GetVisibleFields(AnonymityLevel level);
    
    /// <summary>
    /// Maskeleme kuralları
    /// </summary>
    Dictionary<string, Func<string, string>> GetMaskingRules(AnonymityLevel level);
}

/// <summary>
/// Anonimlik seviyeleri
/// </summary>
public enum AnonymityLevel
{
    /// <summary>
    /// Tam anonim
    /// </summary>
    Full,
    
    /// <summary>
    /// Kısmi anonim (bazı bilgiler görünür)
    /// </summary>
    Partial,
    
    /// <summary>
    /// Doğrulanmış anonim
    /// </summary>
    VerifiedAnonymous,
    
    /// <summary>
    /// Açık kimlik (anonim değil)
    /// </summary>
    Public
}
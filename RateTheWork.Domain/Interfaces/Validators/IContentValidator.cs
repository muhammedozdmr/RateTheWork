namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// İçerik validatör interface'i
/// </summary>
public interface IContentValidator : IDomainValidator<string>
{
    /// <summary>
    /// Minimum/maksimum uzunluk kontrolü
    /// </summary>
    bool IsValidLength(string content, int minLength, int maxLength);
    
    /// <summary>
    /// Yasaklı kelime kontrolü
    /// </summary>
    bool ContainsProhibitedWords(string content);
    
    /// <summary>
    /// Spam pattern kontrolü
    /// </summary>
    bool HasSpamPatterns(string content);
    
    /// <summary>
    /// Kişisel bilgi kontrolü
    /// </summary>
    bool ContainsPersonalInfo(string content);
}

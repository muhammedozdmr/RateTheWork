namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// Veri anonimleştirme service interface'i
/// </summary>
public interface IAnonymizationService
{
    /// <summary>
    /// Kullanıcı adını anonimleştirir
    /// </summary>
    string AnonymizeUsername(string originalUsername, string userId);
    
    /// <summary>
    /// Email adresini anonimleştirir
    /// </summary>
    string AnonymizeEmail(string email);
    
    /// <summary>
    /// IP adresini anonimleştirir
    /// </summary>
    string AnonymizeIpAddress(string ipAddress);
    
    /// <summary>
    /// Metin içindeki hassas bilgileri maskeler
    /// </summary>
    string MaskSensitiveData(string text);
    
    /// <summary>
    /// TC kimlik numarasını maskeler
    /// </summary>
    string MaskTcIdentity(string tcIdentity);
    
    /// <summary>
    /// Telefon numarasını maskeler
    /// </summary>
    string MaskPhoneNumber(string phoneNumber);
    
    /// <summary>
    /// Benzersiz anonim ID üretir
    /// </summary>
    string GenerateAnonymousId(string seed);
}

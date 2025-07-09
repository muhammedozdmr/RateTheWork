namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// Veri şifreleme service interface'i
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Veriyi şifreler
    /// </summary>
    Task<string> EncryptAsync(string plainText);
    
    /// <summary>
    /// Şifrelenmiş veriyi çözer
    /// </summary>
    Task<string> DecryptAsync(string cipherText);
    
    /// <summary>
    /// Hash oluşturur (geri döndürülemez)
    /// </summary>
    string CreateHash(string input);
    
    /// <summary>
    /// Hash doğrulaması yapar
    /// </summary>
    bool VerifyHash(string input, string hash);
    
    /// <summary>
    /// Güvenli rastgele token üretir
    /// </summary>
    string GenerateSecureToken(int length = 32);
}

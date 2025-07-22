namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Veri şifreleme servisi
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Veriyi şifreler
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Şifreli veriyi çözer
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// Hassas veriyi güvenli şekilde siler
    /// </summary>
    void SecureDelete(ref string data);
}

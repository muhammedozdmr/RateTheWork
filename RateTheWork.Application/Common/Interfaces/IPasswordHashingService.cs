namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Şifre hashleme servisi
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Şifreyi hashler
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Şifreyi doğrular
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Güçlü şifre oluşturur
    /// </summary>
    string GenerateStrongPassword(int length = 16);

    /// <summary>
    /// Şifre gücünü kontrol eder
    /// </summary>
    bool IsPasswordStrong(string password);
}

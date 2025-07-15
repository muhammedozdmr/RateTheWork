using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Enums.User;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Şifre hashleme servisi - Infrastructure'da implemente edilecek
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Şifreyi hashler
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Şifre doğrulaması yapar
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
    
    /// <summary>
    /// Hash'in yenilenmesi gerekiyor mu kontrol eder
    /// </summary>
    bool NeedsRehash(string hashedPassword);
    
    /// <summary>
    /// Şifre gücünü kontrol eder
    /// </summary>
    PasswordStrength CheckPasswordStrength(string password);
}

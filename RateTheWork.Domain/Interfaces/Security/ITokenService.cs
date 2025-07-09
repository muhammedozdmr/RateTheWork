namespace RateTheWork.Domain.Interfaces.Security;

/// <summary>
/// Token service interface'i
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Email doğrulama token'ı oluşturur
    /// </summary>
    Task<string> GenerateEmailVerificationTokenAsync(string userId, string email);
    
    /// <summary>
    /// Şifre sıfırlama token'ı oluşturur
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    
    /// <summary>
    /// API erişim token'ı oluşturur
    /// </summary>
    Task<string> GenerateApiTokenAsync(string userId, string[] scopes);
    
    /// <summary>
    /// Token doğrulaması yapar
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, string tokenType);
    
    /// <summary>
    /// Token'ı iptal eder
    /// </summary>
    Task RevokeTokenAsync(string token);
}
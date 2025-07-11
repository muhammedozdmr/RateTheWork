namespace RateTheWork.Domain.Enums.Verification;

/// <summary>
/// Doğrulama türleri
/// </summary>
public enum VerificationType
{
    /// <summary>
    /// Yorum belgesi doğrulama
    /// </summary>
    ReviewDocument,
    
    /// <summary>
    /// Şirket belgesi doğrulama
    /// </summary>
    CompanyDocument,
    
    /// <summary>
    /// Kullanıcı kimlik doğrulama
    /// </summary>
    UserIdentity,
    
    /// <summary>
    /// Çalışma belgesi doğrulama
    /// </summary>
    EmploymentProof
}

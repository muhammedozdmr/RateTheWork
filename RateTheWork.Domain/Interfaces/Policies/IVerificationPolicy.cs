using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Enums.Verification;

namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// Doğrulama policy interface'i
/// </summary>
public interface IVerificationPolicy : IDomainPolicy
{
    /// <summary>
    /// Doğrulama yöntemi seçimi
    /// </summary>
    VerificationMethod SelectVerificationMethod(string companyType, string userType);
    
    /// <summary>
    /// Doğrulama gerekli mi?
    /// </summary>
    bool IsVerificationRequired(string reviewType, decimal rating);
    
    /// <summary>
    /// Doğrulama süresi
    /// </summary>
    TimeSpan GetVerificationDeadline(VerificationMethod method);
    
    /// <summary>
    /// Otomatik doğrulama kriterleri
    /// </summary>
    Task<bool> CanAutoVerifyAsync(string userId, string companyId);
}

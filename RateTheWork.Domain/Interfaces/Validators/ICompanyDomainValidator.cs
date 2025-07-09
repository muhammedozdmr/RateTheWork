namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// Şirket domain validatör interface'i
/// </summary>
public interface ICompanyDomainValidator : IDomainValidator<string>
{
    /// <summary>
    /// Email domain'inin şirkete ait olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsCompanyDomainAsync(string emailDomain, string companyId);
    
    /// <summary>
    /// Domain sahipliği doğrulaması
    /// </summary>
    Task<bool> VerifyDomainOwnershipAsync(string domain, string verificationCode);
    
    /// <summary>
    /// Bilinen kurumsal email sağlayıcıları kontrolü
    /// </summary>
    bool IsPublicEmailProvider(string domain);
}

namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// İstihdam doğrulama hatası exception'ı
/// </summary>
public class EmploymentVerificationException : DomainException
{
    public Guid UserId { get; }
    public Guid CompanyId { get; }
    public string VerificationMethod { get; }

    public EmploymentVerificationException(Guid userId, Guid companyId, string verificationMethod)
        : base($"Employment verification failed. User cannot be verified as employee using method '{verificationMethod}'.")
    {
        UserId = userId;
        CompanyId = companyId;
        VerificationMethod = verificationMethod;
    }
}

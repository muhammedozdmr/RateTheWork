namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Şirket aktif değil exception'ı
/// </summary>
public class CompanyNotActiveException : DomainException
{
    public Guid CompanyId { get; }
    public string CompanyStatus { get; }

    public CompanyNotActiveException(Guid companyId, string companyStatus)
        : base($"Company is not active. Current status: '{companyStatus}'.")
    {
        CompanyId = companyId;
        CompanyStatus = companyStatus;
    }
}

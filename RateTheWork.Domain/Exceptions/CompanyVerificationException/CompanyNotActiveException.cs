namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Şirket aktif değil exception'ı
/// </summary>
public class CompanyNotActiveException : DomainException
{
    public string CompanyId { get; }
    public string CompanyStatus { get; }

    public CompanyNotActiveException(string message)
        : base(message)
    {
        CompanyId = string.Empty;
        CompanyStatus = "Unknown";
    }

    public CompanyNotActiveException(string companyId, string companyStatus)
        : base($"Company '{companyId}' is not active. Current status: {companyStatus}")
    {
        CompanyId = companyId;
        CompanyStatus = companyStatus;
    }

    public CompanyNotActiveException(string companyId, string companyStatus, string message)
        : base(message)
    {
        CompanyId = companyId;
        CompanyStatus = companyStatus;
    }
}

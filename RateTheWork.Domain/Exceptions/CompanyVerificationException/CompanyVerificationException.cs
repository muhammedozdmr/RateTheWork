namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Şirket doğrulama hatası exception'ı
/// </summary>
public class CompanyVerificationException : DomainException
{
    public string VerificationType { get; }
    public string CompanyName { get; }
    public string Reason { get; }

    public CompanyVerificationException(string verificationType, string companyName, string reason)
        : base($"Company verification failed for '{companyName}'. Type: {verificationType}. Reason: {reason}")
    {
        VerificationType = verificationType;
        CompanyName = companyName;
        Reason = reason;
    }
}

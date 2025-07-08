namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Email domain doğrulama hatası exception'ı
/// </summary>
public class EmailDomainVerificationException : DomainException
{
    public string Email { get; }
    public string ExpectedDomain { get; }
    public string ActualDomain { get; }

    public EmailDomainVerificationException(string email, string expectedDomain)
        : base($"Email domain verification failed. Email '{email}' does not belong to domain '{expectedDomain}'.")
    {
        Email = email;
        ExpectedDomain = expectedDomain;
        ActualDomain = email.Split('@').LastOrDefault() ?? "Unknown";
    }
}

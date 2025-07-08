namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Geçersiz vergi numarası exception'ı
/// </summary>
public class InvalidTaxNumberException : DomainException
{
    public string TaxNumber { get; }
    public string Country { get; }

    public InvalidTaxNumberException(string taxNumber, string country)
        : base($"Invalid tax number '{taxNumber}' for country '{country}'.")
    {
        TaxNumber = taxNumber;
        Country = country;
    }
}

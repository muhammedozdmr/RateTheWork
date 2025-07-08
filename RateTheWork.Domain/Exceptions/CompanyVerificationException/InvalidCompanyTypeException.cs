namespace RateTheWork.Domain.Exceptions.CompanyVerificationException;

/// <summary>
/// Geçersiz şirket tipi exception'ı
/// </summary>
public class InvalidCompanyTypeException : DomainException
{
    public string ProvidedType { get; }
    public string[] AllowedTypes { get; }

    public InvalidCompanyTypeException(string providedType, string[] allowedTypes)
        : base($"Invalid company type '{providedType}'. Allowed types: {string.Join(", ", allowedTypes)}.")
    {
        ProvidedType = providedType;
        AllowedTypes = allowedTypes;
    }
}

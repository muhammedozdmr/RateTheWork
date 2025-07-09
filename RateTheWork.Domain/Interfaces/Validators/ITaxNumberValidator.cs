namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// Vergi numarası validatör interface'i
/// </summary>
public interface ITaxNumberValidator : IDomainValidator<string>
{
    /// <summary>
    /// Vergi numarası format kontrolü
    /// </summary>
    bool IsValidFormat(string taxNumber, string countryCode);
    
    /// <summary>
    /// Online doğrulama
    /// </summary>
    Task<bool> VerifyOnlineAsync(string taxNumber, string companyName);
}


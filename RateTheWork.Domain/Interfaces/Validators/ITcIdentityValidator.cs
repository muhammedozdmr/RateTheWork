namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// TC Kimlik validatör interface'i
/// </summary>
public interface ITcIdentityValidator : IDomainValidator<string>
{
    /// <summary>
    /// TC Kimlik numarası algoritma kontrolü
    /// </summary>
    bool IsValidFormat(string tcIdentity);
    
    /// <summary>
    /// MERNİS doğrulaması (opsiyonel)
    /// </summary>
    Task<bool> VerifyWithMernisAsync(string tcIdentity, string name, string surname, int birthYear);
}

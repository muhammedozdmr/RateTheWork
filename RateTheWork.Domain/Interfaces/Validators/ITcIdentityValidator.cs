namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// TC Kimlik validatör interface'i
/// </summary>
public interface ITcIdentityValidator
{
    /// <summary>
    /// TC Kimlik numarası algoritma kontrolü
    /// </summary>
    bool IsValidFormat(string tcIdentity);
}

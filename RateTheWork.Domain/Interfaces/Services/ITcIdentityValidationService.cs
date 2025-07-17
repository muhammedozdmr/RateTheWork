namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// TC Kimlik doğrulama servisi
/// </summary>
public interface ITcIdentityValidationService
{
    /// <summary>
    /// TC Kimlik algoritma kontrolü (ITcIdentityValidator'ı kullanır)
    /// </summary>
    bool IsValidTcIdentity(string tcIdentity);

    /// <summary>
    /// MERNİS üzerinden doğrulama
    /// </summary>
    Task<bool> ValidateWithMernisAsync(string tcIdentity, string firstName, string lastName, DateTime birthDate);

    /// <summary>
    /// TC Kimlik bilgilerini maskeler
    /// </summary>
    string MaskTcIdentity(string tcIdentity);

    /// <summary>
    /// Yabancı kimlik numarası kontrolü
    /// </summary>
    bool IsValidForeignIdentity(string identityNumber);
}

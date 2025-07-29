namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// TC Kimlik doğrulama servisi
/// </summary>
public interface ITcIdentityValidationService
{
    /// <summary>
    /// TC Kimlik numarasını doğrular
    /// </summary>
    Task<bool> ValidateAsync
    (
        string tcIdentityNumber
        , string firstName
        , string lastName
        , int birthYear
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// TC Kimlik numarasının formatını kontrol eder
    /// </summary>
    bool IsValidFormat(string tcIdentityNumber);
    
    /// <summary>
    /// Devlet servisinden TC kimlik doğrulaması yapar
    /// </summary>
    Task<bool> ValidateWithGovernmentServiceAsync(
        string tcIdentityNumber,
        string firstName,
        string lastName,
        int birthYear,
        CancellationToken cancellationToken = default);
}

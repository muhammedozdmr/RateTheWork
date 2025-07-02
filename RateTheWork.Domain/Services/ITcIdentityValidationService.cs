namespace RateTheWork.Domain.Services;

public interface ITcIdentityValidationService
{
    bool IsValidTcIdentity(string tcIdentity);
    Task<bool> ValidateWithGovernmentServiceAsync(string tcIdentity, string firstName, string lastName, DateTime birthDate);
}

namespace RateTheWork.Domain.Interfaces.ValueObjects;

public interface IPhoneNumber
{
    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }
}

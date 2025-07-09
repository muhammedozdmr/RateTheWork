namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Adres value object interface'i
/// </summary>
public interface IAddress : IValueObject
{
    string Street { get; }
    string City { get; }
    string State { get; }
    string Country { get; }
    string PostalCode { get; }
    ICoordinate? Coordinates { get; }
    string FullAddress { get; }
}

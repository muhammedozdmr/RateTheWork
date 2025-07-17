using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Adres value object
/// </summary>
public record Address : IAddress
{
    private Address
    (
        string street
        , string city
        , string state
        , string country
        , string postalCode
        , ICoordinate? coordinates = null
    )
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentNullException(nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));

        Street = street;
        City = city;
        State = state ?? string.Empty;
        Country = country ?? "Türkiye";
        PostalCode = postalCode ?? string.Empty;
        Coordinates = coordinates;
    }

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string PostalCode { get; }
    public ICoordinate? Coordinates { get; }

    public string FullAddress =>
        $"{Street}, {City} {PostalCode}, {State}, {Country}"
            .Replace("  ", " ")
            .Replace(", ,", ",")
            .Trim(' ', ',');

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }

    public static Address Create
    (
        string street
        , string city
        , string state = ""
        , string country = "Türkiye"
        , string postalCode = ""
        , ICoordinate? coordinates = null
    )
        => new(street, city, state, country, postalCode, coordinates);
}

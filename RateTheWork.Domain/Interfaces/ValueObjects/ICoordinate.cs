namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Koordinat value object interface'i
/// </summary>
public interface ICoordinate : IValueObject
{
    double Latitude { get; }
    double Longitude { get; }
    bool IsValid { get; }
    double DistanceTo(ICoordinate other);
}

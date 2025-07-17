namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Coğrafi koordinat bilgisi
/// </summary>
public sealed class Coordinate : ValueObject
{
    private Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Enlem
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Boylam
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// Koordinat oluşturur
    /// </summary>
    public static Coordinate Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Enlem -90 ile 90 arasında olmalıdır.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Boylam -180 ile 180 arasında olmalıdır.");

        return new Coordinate(latitude, longitude);
    }

    /// <summary>
    /// İki koordinat arasındaki mesafeyi hesaplar (kilometre cinsinden)
    /// </summary>
    public double DistanceTo(Coordinate other)
    {
        const double earthRadius = 6371; // km

        var lat1Rad = ToRadians(Latitude);
        var lat2Rad = ToRadians(other.Latitude);
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString()
    {
        return $"{Latitude:F6}, {Longitude:F6}";
    }
}

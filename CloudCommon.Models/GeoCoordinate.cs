using System.Globalization;
namespace AngryMonkey.CloudCommon.Models;

public sealed record GeoCoordinate
{
    public double Latitude { get; }
    public double Longitude { get; }
    public GeoCoordinate(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceKilometersTo(GeoCoordinate other)
    {
        double deltaLatitude = Radians(other.Latitude - Latitude);
        double deltaLongitude = Radians(other.Longitude - Longitude);
        double a = Math.Pow(Math.Sin(deltaLatitude / 2), 2) + Math.Cos(Radians(Latitude)) * Math.Cos(Radians(other.Latitude)) * Math.Pow(Math.Sin(deltaLongitude / 2), 2);
        return 6371.0088 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
    public override string ToString() => FormattableString.Invariant($"{Latitude:F6}, {Longitude:F6}");
    private static double Radians(double value) => value * Math.PI / 180;
}


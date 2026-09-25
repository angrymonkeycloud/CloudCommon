using AngryMonkey.CloudCommon.Models;
using LegacyMoney = AngryMonkey.Cloud.Geography.Money;
using LegacyCoordinate = AngryMonkey.Cloud.Geography.Coordinate;
namespace AngryMonkey.CloudCommon.Geography;

public static class GeographyAdapters
{
    public static Money? ToCommon(this LegacyMoney value) => value.IsEmpty ? null : Money.FromUnits(value.Currency, value.Units, value.Nanos);
    public static LegacyMoney ToGeography(this Money value) => new(value.Currency, value.Units, value.Nanos);
    public static GeoCoordinate ToCommon(this LegacyCoordinate value) => new(value.Latitude, value.Longitude);
    public static LegacyCoordinate ToGeography(this GeoCoordinate value) => new(value.Latitude, value.Longitude);
}


using System.Text.Json;
using AngryMonkey.CloudCommon.Models;
using AngryMonkey.CloudCommon.Geography;
using Xunit;
namespace AngryMonkey.CloudCommon.Tests;

public class ModelTests
{
    [Theory]
    [InlineData("-1.000000001")]
    [InlineData("123.456789123")]
    [InlineData("0")]
    public void MoneyRoundTripsLegacyUnitsAndJsonWithoutRounding(string amount)
    {
        Money money = new("usd", decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(money, money.ToGeography().ToCommon());
        Assert.Equal(money, JsonSerializer.Deserialize<Money>(JsonSerializer.Serialize(money)));
    }

    [Fact]
    public void EmptyLegacyMoneyRemainsAbsent() => Assert.Null(new AngryMonkey.Cloud.Geography.Money("USD").ToCommon());

    [Fact]
    public void MoneyRejectsCurrencyMixingAndSilentPrecisionLoss()
    {
        Assert.Throws<InvalidOperationException>(() => new Money("USD", 2).Add(new("EUR", 3)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money("USD", .0000000001m));
        Assert.Equal(1.24m, new Money("USD", 1.235m).Round(2).Amount);
        Assert.Equal(2m, Money.FromUnits("USD", 1, 1_000_000_000).Amount);
    }

    [Fact]
    public void CoordinateRejectsNonFiniteAndOutOfRangeValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(91, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(0, 181));
        GeoCoordinate beirut = new(33.8938, 35.5018);
        Assert.Equal(0, beirut.DistanceKilometersTo(beirut), 5);
        Assert.Equal(beirut, beirut.ToGeography().ToCommon());
    }

    [Fact]
    public void AdjacentIntervalsDoNotOverlapAndEndIsExcluded()
    {
        DateTimeOffset start = DateTimeOffset.Parse("2026-09-25T10:00:00Z");
        DateTimeRange first = new(start, start.AddHours(1));
        DateTimeRange next = new(start.AddHours(1), start.AddHours(2));
        Assert.False(first.Overlaps(next));
        Assert.False(first.Contains(first.End));
        Assert.True(first.Contains(first.Start));
        Assert.Throws<ArgumentException>(() => new DateTimeRange(start, start));
    }

    [Fact]
    public void AddressPreservesPartialAndGeographicData()
    {
        PostalAddress address = new() { CountryCode = "LB", SubdivisionCode = "BA", SubdivisionChildCode = "child", Locality = "Beirut", Line1 = "12 Garden Street", Location = new(33.8938, 35.5018) };
        Assert.Equal(address, JsonSerializer.Deserialize<PostalAddress>(JsonSerializer.Serialize(address)));
        Assert.True(new PostalAddress().IsEmpty);
        Assert.False(new PostalAddress { Location = new(0, 0) }.IsEmpty);
    }
}


using System.Text.Json.Serialization;
namespace AngryMonkey.CloudCommon.Models;

public sealed record PostalAddress
{
    public string? CountryCode { get; init; }
    public string? SubdivisionCode { get; init; }
    public string? SubdivisionChildCode { get; init; }
    public string? Locality { get; init; }
    public string? PostalCode { get; init; }
    public string? Line1 { get; init; }
    public string? Line2 { get; init; }
    public GeoCoordinate? Location { get; init; }
    [JsonIgnore] public bool IsEmpty => Parts.All(string.IsNullOrWhiteSpace) && Location is null;
    private string?[] Parts => [Line1, Line2, Locality, SubdivisionChildCode, SubdivisionCode, PostalCode, CountryCode];
    public override string ToString() => string.Join(", ", Parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));
}


using System.Globalization;
using System.Text.Json.Serialization;

namespace AngryMonkey.CloudCommon.Models;

public sealed record Money
{
    public string Currency { get; }
    public decimal Amount { get; }
    [JsonIgnore] public long Units => checked((long)decimal.Truncate(Amount));
    [JsonIgnore] public int Nanos => checked((int)((Amount - decimal.Truncate(Amount)) * 1_000_000_000m));

    [JsonConstructor]
    public Money(string currency, decimal amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        string normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("Use a three-letter currency code.", nameof(currency));
        if (decimal.Round(amount, 9) != amount || decimal.Truncate(amount) < long.MinValue || decimal.Truncate(amount) > long.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(amount), "Money supports signed 64-bit units and up to nine decimal places.");
        Currency = normalized;
        Amount = amount;
    }

    public static Money FromUnits(string currency, long units, int nanos) => new(currency, units + nanos / 1_000_000_000m);
    public Money Add(Money other) => new(Currency, checked(Amount + SameCurrency(other).Amount));
    public Money Subtract(Money other) => new(Currency, checked(Amount - SameCurrency(other).Amount));
    public Money Round(int decimalPlaces, MidpointRounding rounding = MidpointRounding.ToEven) => new(Currency, decimal.Round(Amount, decimalPlaces, rounding));
    public string Format(IFormatProvider? provider = null, int decimalPlaces = 2) => $"{Currency} {Amount.ToString($"N{decimalPlaces}", provider ?? CultureInfo.InvariantCulture)}";
    private Money SameCurrency(Money other) => other.Currency == Currency ? other : throw new InvalidOperationException("Currency conversion requires an explicit exchange rate.");
}


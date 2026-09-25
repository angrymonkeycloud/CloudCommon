using System.Globalization;
using System.Text.RegularExpressions;
namespace AngryMonkey.CloudCommon.Theming;

public readonly record struct ThemeColor(byte Red, byte Green, byte Blue)
{
    public static ThemeColor Parse(string hex)
    {
        if (Regex.IsMatch(hex ?? "", "^#[0-9a-fA-F]{3}$"))
            hex = "#" + string.Concat(hex![1..].Select(character => new string(character, 2)));
        if (!Regex.IsMatch(hex ?? "", "^#[0-9a-fA-F]{6}$"))
            throw new ArgumentException("Colors must use six-digit hex notation, such as #6750a4.", nameof(hex));
        return new(byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber), byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber), byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber));
    }
    public ThemeColor Mix(ThemeColor other, double amount) => new(Blend(Red, other.Red, amount), Blend(Green, other.Green, amount), Blend(Blue, other.Blue, amount));
    public double Luminance => .2126 * Linear(Red) + .7152 * Linear(Green) + .0722 * Linear(Blue);
    public double Contrast(ThemeColor other) => (Math.Max(Luminance, other.Luminance) + .05) / (Math.Min(Luminance, other.Luminance) + .05);
    public string Foreground => Contrast(Parse("#ffffff")) >= Contrast(Parse("#000000")) ? "#ffffff" : "#000000";
    public override string ToString() => $"#{Red:x2}{Green:x2}{Blue:x2}";
    private static byte Blend(byte first, byte second, double amount) => (byte)Math.Round(first * (1 - amount) + second * amount);
    private static double Linear(byte channel) => channel / 255d <= .04045 ? channel / 255d / 12.92 : Math.Pow((channel / 255d + .055) / 1.055, 2.4);
}


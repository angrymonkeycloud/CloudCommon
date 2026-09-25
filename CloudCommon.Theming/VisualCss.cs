using System.Text.RegularExpressions;
namespace AngryMonkey.CloudCommon.Theming;

public static class VisualCss
{
    public static IReadOnlyList<string> Properties { get; } = Array.AsReadOnly(new[]
    {
        "background", "color", "border-color", "border-radius", "border-width", "border-style",
        "box-shadow", "font-family", "font-size", "font-weight", "line-height", "letter-spacing", "text-decoration",
        "opacity", "outline", "outline-offset", "transition", "filter", "animation"
    });

    public static IReadOnlyDictionary<string, string> Parse(string? css)
    {
        Dictionary<string, string> result = [];
        if (string.IsNullOrWhiteSpace(css))
            return result;
        foreach (string declaration in css.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int colon = declaration.IndexOf(':');
            if (colon < 1)
                throw new ArgumentException("CSS overrides must contain property: value declarations.");
            string property = declaration[..colon].Trim().ToLowerInvariant();
            string value = declaration[(colon + 1)..].Trim();
            if (!Properties.Contains(property) || property == "animation")
                throw new ArgumentException($"'{property}' is not a supported visual override. Use the typed Animation property for animations.");
            ValidateValue(value);
            result[property] = value;
        }
        return result;
    }

    public static void ValidateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || Regex.IsMatch(value, @"[{}<>;@!\\]|/\*|\*/|url\s*\(|expression\s*\(", RegexOptions.IgnoreCase) || value.Any(char.IsControl))
            throw new ArgumentException("Use a visual CSS value without rules, URLs, comments, escapes or !important.");
        int depth = 0;
        foreach (char character in value)
        {
            if (character == '(') depth++;
            if (character == ')' && --depth < 0) throw new ArgumentException("Unbalanced CSS value.");
        }
        if (depth != 0)
            throw new ArgumentException("Unbalanced CSS value.");
    }

    internal static void Identifier(string value)
    {
        if (!Regex.IsMatch(value, "^[a-z][a-z0-9-]{0,79}$"))
            throw new ArgumentException("Identifiers must start with a lowercase letter and contain only lowercase letters, digits and hyphens.");
    }
}


namespace AngryMonkey.CloudCommon.Theming;

public enum ThemeModes { Light, Dark }
public enum ThemeElements { Button, SecondaryButton, DangerButton, Input, Link, Card, Dialog, Badge, Table, Navigation }
public enum ThemeStates { Normal, Hover, Active, Focus, Disabled, Selected, Invalid }

public sealed class ThemeDefinition
{
    public ThemeColors Colors { get; set; } = new();
    public ThemeTypography Typography { get; set; } = new();
    public ThemeVisuals Visuals { get; set; } = new();
    public Dictionary<ThemeElements, ElementTheme> Elements { get; set; } = [];
    public Dictionary<string, ThemeAnimation> Animations { get; set; } = [];
    public Dictionary<ThemeModes, Dictionary<string, string>> ModeTokens { get; set; } = [];
    public ElementTheme Element(ThemeElements element)
    {
        if (!Elements.TryGetValue(element, out ElementTheme? result))
            Elements[element] = result = new();
        return result;
    }
}

public sealed class ThemeColors
{
    public string Primary { get; set; } = "#525252";
    public string? Secondary { get; set; }
    public string Neutral { get; set; } = "#737373";
    public string? Success { get; set; }
    public string? Warning { get; set; }
    public string? Danger { get; set; }
    public Dictionary<int, string> PrimaryShades { get; set; } = [];
    public Dictionary<int, string> SecondaryShades { get; set; } = [];
    public Dictionary<int, string> NeutralShades { get; set; } = [];
    public Dictionary<int, string> SuccessShades { get; set; } = [];
    public Dictionary<int, string> WarningShades { get; set; } = [];
    public Dictionary<int, string> DangerShades { get; set; } = [];
}

public sealed class ThemeTypography
{
    public string Family { get; set; } = "Inter, 'Segoe UI', system-ui, sans-serif";
    public string Small { get; set; } = ".875rem";
    public string Body { get; set; } = "1rem";
    public string Heading { get; set; } = "1.5rem";
    public string Weight { get; set; } = "400";
    public string StrongWeight { get; set; } = "600";
    public string LineHeight { get; set; } = "1.5";
}

public sealed class ThemeVisuals
{
    public string ControlRadius { get; set; } = ".5rem";
    public string SurfaceRadius { get; set; } = ".75rem";
    public string BorderWidth { get; set; } = "1px";
    public string BorderStyle { get; set; } = "solid";
    public string Shadow { get; set; } = "0 8px 24px #00000012";
    public string Duration { get; set; } = "160ms";
    public string Easing { get; set; } = "ease";
}

public sealed class ElementTheme
{
    public VisualStyle Normal { get; set; } = new();
    public VisualStyle Hover { get; set; } = new();
    public VisualStyle Active { get; set; } = new();
    public VisualStyle Focus { get; set; } = new();
    public VisualStyle Disabled { get; set; } = new();
    public VisualStyle Selected { get; set; } = new();
    public VisualStyle Invalid { get; set; } = new();
    public VisualStyle State(ThemeStates state) => state switch
    {
        ThemeStates.Normal => Normal, ThemeStates.Hover => Hover, ThemeStates.Active => Active,
        ThemeStates.Focus => Focus, ThemeStates.Disabled => Disabled, ThemeStates.Selected => Selected, _ => Invalid
    };
}

public sealed class VisualStyle
{
    public string? Background { get; set; }
    public string? Color { get; set; }
    public string? BorderColor { get; set; }
    public string? BorderRadius { get; set; }
    public string? Shadow { get; set; }
    public string? Css { get; set; }
    public string? Animation { get; set; }
}

public sealed class ThemeAnimation
{
    public string From { get; set; } = "opacity: .65";
    public string To { get; set; } = "opacity: 1";
    public string Duration { get; set; } = "1s";
}

public static class CloudThemes
{
    public static ThemeDefinition Grayscale() => new();
    public static ThemeDefinition Color(string primary, Action<ThemeDefinition>? configure = null)
    {
        ThemeDefinition theme = new();
        theme.Colors.Primary = primary;
        configure?.Invoke(theme);
        return theme;
    }
    public static ThemeDefinition Design(Action<ThemeDefinition> configure)
    {
        ThemeDefinition theme = new();
        configure(theme);
        return theme;
    }
}


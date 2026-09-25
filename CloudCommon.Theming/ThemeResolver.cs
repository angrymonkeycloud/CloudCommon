using System.Collections.ObjectModel;
namespace AngryMonkey.CloudCommon.Theming;

public sealed record ResolvedTheme(ThemeModes Mode, IReadOnlyDictionary<string, string> Tokens);

public static class ThemeResolver
{
    public static IReadOnlyList<int> Shades { get; } = Array.AsReadOnly(new[] { 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950 });

    public static ResolvedTheme Resolve(ThemeDefinition? definition = null, ThemeModes mode = ThemeModes.Light)
    {
        ThemeDefinition theme = definition ?? CloudThemes.Grayscale();
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        Dictionary<string, string> tokens = [];
        Palette(tokens, "primary", theme.Colors.Primary, theme.Colors.PrimaryShades);
        Palette(tokens, "secondary", theme.Colors.Secondary ?? theme.Colors.Primary, theme.Colors.SecondaryShades);
        Palette(tokens, "neutral", theme.Colors.Neutral, theme.Colors.NeutralShades);
        Palette(tokens, "success", theme.Colors.Success ?? theme.Colors.Primary, theme.Colors.SuccessShades);
        Palette(tokens, "warning", theme.Colors.Warning ?? theme.Colors.Primary, theme.Colors.WarningShades);
        Palette(tokens, "danger", theme.Colors.Danger ?? theme.Colors.Primary, theme.Colors.DangerShades);
        bool dark = mode == ThemeModes.Dark;
        string Neutral(int light, int night) => tokens[$"neutral-{(dark ? night : light)}"];
        tokens["color-background"] = Neutral(50, 950);
        tokens["color-surface"] = dark ? tokens["neutral-900"] : "#ffffff";
        tokens["color-surface-subtle"] = Neutral(100, 800);
        tokens["color-text"] = Neutral(950, 50);
        tokens["color-muted"] = Neutral(700, 200);
        tokens["color-border"] = Neutral(300, 700);
        tokens["color-control-border"] = Neutral(500, 400);
        tokens["color-overlay"] = "#00000080";
        tokens["color-inverse-surface"] = "#171717";
        tokens["color-inverse-text"] = "#ffffff";
        foreach (string name in new[] { "primary", "secondary", "success", "warning", "danger" })
        {
            tokens[$"color-{name}"] = tokens[$"{name}-{(dark ? 300 : 500)}"];
            tokens[$"color-{name}-hover"] = tokens[$"{name}-{(dark ? 200 : 600)}"];
            tokens[$"color-{name}-active"] = tokens[$"{name}-{(dark ? 100 : 700)}"];
            tokens[$"color-{name}-soft"] = tokens[$"{name}-{(dark ? 900 : 50)}"];
        }
        tokens["color-focus"] = tokens[$"primary-{(dark ? 300 : 600)}"];
        tokens["color-selected"] = tokens[$"primary-{(dark ? 800 : 100)}"];
        tokens["color-link"] = AccessibleInk(tokens["color-primary"], tokens["color-surface"]);
        tokens["color-link-hover"] = AccessibleInk(tokens["color-primary-hover"], tokens["color-surface"]);
        tokens["font-family"] = theme.Typography.Family;
        tokens["font-small"] = theme.Typography.Small;
        tokens["font-body"] = theme.Typography.Body;
        tokens["font-heading"] = theme.Typography.Heading;
        tokens["font-weight"] = theme.Typography.Weight;
        tokens["font-strong"] = theme.Typography.StrongWeight;
        tokens["line-height"] = theme.Typography.LineHeight;
        tokens["radius-control"] = theme.Visuals.ControlRadius;
        tokens["radius-surface"] = theme.Visuals.SurfaceRadius;
        tokens["border-width"] = theme.Visuals.BorderWidth;
        tokens["border-style"] = theme.Visuals.BorderStyle;
        tokens["shadow"] = theme.Visuals.Shadow;
        tokens["duration"] = theme.Visuals.Duration;
        tokens["easing"] = theme.Visuals.Easing;

        if (theme.ModeTokens.TryGetValue(mode, out Dictionary<string, string>? overrides))
            foreach ((string key, string value) in overrides)
            {
                if (!tokens.ContainsKey(key) || key.StartsWith("primary-") || key.StartsWith("secondary-") || key.StartsWith("neutral-"))
                    throw new ArgumentException($"Unknown or palette token '{key}'. Use shade overrides for palettes.");
                VisualCss.ValidateValue(value);
                tokens[key] = value;
            }
        foreach (string name in new[] { "primary", "secondary", "success", "warning", "danger" })
            foreach (string suffix in new[] { "", "-hover", "-active" })
                tokens[$"color-on-{name}{suffix}"] = ThemeColor.Parse(tokens[$"color-{name}{suffix}"]).Foreground;
        tokens["color-link"] = AccessibleInk(tokens["color-link"], tokens["color-surface"]);
        tokens["color-link-hover"] = AccessibleInk(tokens["color-link-hover"], tokens["color-surface"]);

        foreach ((string name, ThemeAnimation animation) in theme.Animations)
        {
            VisualCss.Identifier(name);
            VisualCss.Parse(animation.From);
            VisualCss.Parse(animation.To);
            VisualCss.ValidateValue(animation.Duration);
        }
        foreach (ThemeElements element in Enum.GetValues<ThemeElements>())
        {
            string name = Name(element);
            Dictionary<string, string> normal = Defaults(element, tokens);
            if (theme.Elements.TryGetValue(element, out ElementTheme? settings))
                Apply(normal, settings.Normal, theme);
            foreach (ThemeStates state in Enum.GetValues<ThemeStates>())
            {
                Dictionary<string, string> style = new(normal);
                string? colorFamily = element switch { ThemeElements.Button => "primary", ThemeElements.SecondaryButton => "secondary", ThemeElements.DangerButton => "danger", _ => null };
                if (state is ThemeStates.Hover or ThemeStates.Active)
                {
                    string suffix = state == ThemeStates.Hover ? "hover" : "active";
                    if (colorFamily is not null)
                    {
                        style["background"] = $"var(--amc-color-{colorFamily}-{suffix})";
                        style["color"] = $"var(--amc-color-on-{colorFamily}-{suffix})";
                    }
                    else if (element == ThemeElements.Link)
                        style["color"] = "var(--amc-color-link-hover)";
                    else
                        style["background"] = "var(--amc-color-surface-subtle)";
                }
                if (state == ThemeStates.Focus)
                {
                    style["outline"] = "2px solid var(--amc-color-focus)";
                    style["outline-offset"] = "3px";
                }
                if (state == ThemeStates.Disabled) style["opacity"] = ".5";
                if (state == ThemeStates.Selected)
                {
                    style["background"] = "var(--amc-color-selected)";
                    style["color"] = ThemeColor.Parse(tokens["color-selected"]).Foreground;
                }
                if (state == ThemeStates.Invalid) style["border-color"] = "var(--amc-color-danger)";
                if (settings is not null && state != ThemeStates.Normal)
                    Apply(style, settings.State(state), theme);
                foreach ((string property, string value) in style)
                    tokens[$"{name}-{state.ToString().ToLowerInvariant()}-{property}"] = value;
            }
        }
        foreach (string value in tokens.Values)
            VisualCss.ValidateValue(value);
        return new(mode, new ReadOnlyDictionary<string, string>(tokens));
    }

    public static string Name(ThemeElements element) => element switch { ThemeElements.SecondaryButton => "secondary-button", ThemeElements.DangerButton => "danger-button", _ => element.ToString().ToLowerInvariant() };

    private static void Palette(Dictionary<string, string> tokens, string name, string seed, Dictionary<int, string> overrides)
    {
        ThemeColor color = ThemeColor.Parse(seed);
        double[] mixes = [.96, .9, .75, .55, .3, 0, .15, .3, .5, .7, .85];
        foreach (int shade in overrides.Keys)
            if (!Shades.Contains(shade))
                throw new ArgumentException($"Unsupported shade {shade}.");
        for (int index = 0; index < Shades.Count; index++)
        {
            int shade = Shades[index];
            tokens[$"{name}-{shade}"] = overrides.TryGetValue(shade, out string? value) ? ThemeColor.Parse(value).ToString() : color.Mix(ThemeColor.Parse(shade < 500 ? "#ffffff" : "#000000"), mixes[index]).ToString();
        }
    }

    private static string AccessibleInk(string foreground, string background)
    {
        ThemeColor ink = ThemeColor.Parse(foreground);
        ThemeColor surface = ThemeColor.Parse(background);
        ThemeColor target = ThemeColor.Parse(surface.Foreground);
        for (int step = 0; step <= 100; step++)
        {
            ThemeColor candidate = ink.Mix(target, step / 100d);
            if (candidate.Contrast(surface) >= 4.5) return candidate.ToString();
        }
        return target.ToString();
    }

    private static Dictionary<string, string> Defaults(ThemeElements element, Dictionary<string, string> tokens)
    {
        string? family = element switch { ThemeElements.Button => "primary", ThemeElements.SecondaryButton => "secondary", ThemeElements.DangerButton => "danger", _ => null };
        return new()
        {
            ["background"] = family is not null ? $"var(--amc-color-{family})" : element == ThemeElements.Link ? "transparent" : "var(--amc-color-surface)",
            ["color"] = family is not null ? $"var(--amc-color-on-{family})" : element == ThemeElements.Link ? "var(--amc-color-link)" : "var(--amc-color-text)",
            ["border-color"] = family is not null ? $"var(--amc-color-{family})" : element is ThemeElements.Card or ThemeElements.Dialog or ThemeElements.Table or ThemeElements.Navigation ? "var(--amc-color-border)" : "var(--amc-color-control-border)",
            ["border-width"] = element == ThemeElements.Link ? "0" : "var(--amc-border-width)", ["border-style"] = "var(--amc-border-style)",
            ["border-radius"] = element is ThemeElements.Card or ThemeElements.Dialog ? "var(--amc-radius-surface)" : "var(--amc-radius-control)",
            ["box-shadow"] = element is ThemeElements.Card or ThemeElements.Dialog ? "var(--amc-shadow)" : "none",
            ["font-family"] = "var(--amc-font-family)", ["font-size"] = "var(--amc-font-body)", ["font-weight"] = "var(--amc-font-weight)",
            ["line-height"] = "var(--amc-line-height)", ["letter-spacing"] = "normal",
            ["text-decoration"] = element == ThemeElements.Link ? "underline" : "none",
            ["opacity"] = "1", ["outline"] = "revert", ["outline-offset"] = "0",
            ["transition"] = "background var(--amc-duration) var(--amc-easing), color var(--amc-duration) var(--amc-easing), box-shadow var(--amc-duration) var(--amc-easing)",
            ["filter"] = "none", ["animation"] = "none"
        };
    }

    private static void Apply(Dictionary<string, string> style, VisualStyle settings, ThemeDefinition theme)
    {
        Dictionary<string, string?> typed = new() { ["background"] = settings.Background, ["color"] = settings.Color, ["border-color"] = settings.BorderColor, ["border-radius"] = settings.BorderRadius, ["box-shadow"] = settings.Shadow };
        foreach ((string property, string? value) in typed)
            if (value is not null) style[property] = value;
        foreach ((string property, string value) in VisualCss.Parse(settings.Css))
            style[property] = value;
        if (settings.Animation is not null)
        {
            if (!theme.Animations.TryGetValue(settings.Animation, out ThemeAnimation? animation))
                throw new ArgumentException($"Unknown animation '{settings.Animation}'.");
            style["animation"] = $"cloud-animation-{settings.Animation} {animation.Duration} ease-in-out infinite alternate";
        }
    }
}


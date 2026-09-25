using AngryMonkey.CloudCommon.Theming;
using Xunit;
namespace AngryMonkey.CloudCommon.Tests;

public class ThemeTests
{
    [Theory]
    [InlineData(ThemeModes.Light)]
    [InlineData(ThemeModes.Dark)]
    public void UnconfiguredThemeHasCompleteGrayscalePalettes(ThemeModes mode)
    {
        ResolvedTheme result = ThemeResolver.Resolve(mode: mode);
        foreach (string palette in new[] { "primary", "secondary", "neutral", "success", "warning", "danger" })
            foreach (int shade in ThemeResolver.Shades)
            {
                ThemeColor color = ThemeColor.Parse(result.Tokens[$"{palette}-{shade}"]);
                Assert.Equal(color.Red, color.Green);
                Assert.Equal(color.Green, color.Blue);
            }
        foreach (ThemeElements element in Enum.GetValues<ThemeElements>())
            foreach (ThemeStates state in Enum.GetValues<ThemeStates>())
                foreach (string property in VisualCss.Properties)
                    Assert.True(result.Tokens.ContainsKey($"{ThemeResolver.Name(element)}-{state.ToString().ToLowerInvariant()}-{property}"));
    }

    [Fact]
    public void PalettePreservesSeedAndExplicitShades()
    {
        ThemeDefinition theme = CloudThemes.Color("#ff6600");
        theme.Colors.PrimaryShades[700] = "#993300";
        ResolvedTheme result = ThemeResolver.Resolve(theme);
        Assert.Equal("#ff6600", result.Tokens["primary-500"]);
        Assert.Equal("#993300", result.Tokens["primary-700"]);
        Assert.True(ThemeColor.Parse(result.Tokens["primary-50"]).Luminance > ThemeColor.Parse(result.Tokens["primary-500"]).Luminance);
    }

    [Theory]
    [InlineData("#ffffff")]
    [InlineData("#000000")]
    [InlineData("#ffff00")]
    [InlineData("#ff6600")]
    [InlineData("#6750a4")]
    public void GeneratedForegroundsAndLinksHaveReadableContrast(string seed)
    {
        foreach (ThemeModes mode in Enum.GetValues<ThemeModes>())
        {
            ResolvedTheme result = ThemeResolver.Resolve(CloudThemes.Color(seed), mode);
            foreach (string suffix in new[] { "", "-hover", "-active" })
                Assert.True(ThemeColor.Parse(result.Tokens[$"color-primary{suffix}"]).Contrast(ThemeColor.Parse(result.Tokens[$"color-on-primary{suffix}"])) >= 4.5);
            Assert.True(ThemeColor.Parse(result.Tokens["color-link"]).Contrast(ThemeColor.Parse(result.Tokens["color-surface"])) >= 4.5);
        }
    }

    [Fact]
    public void CssOverridesOnePropertyWithoutReplacingOtherStates()
    {
        ThemeDefinition theme = CloudThemes.Color("#6750a4");
        theme.Element(ThemeElements.Button).Normal.BorderRadius = "12px";
        theme.Element(ThemeElements.Button).Normal.Background = "#123456";
        theme.Element(ThemeElements.Button).Normal.Css = "background: #112233";
        theme.Element(ThemeElements.Button).Hover.Css = "box-shadow: 0 0 8px #333333";
        ResolvedTheme result = ThemeResolver.Resolve(theme);
        Assert.Equal("#112233", result.Tokens["button-normal-background"]);
        Assert.Equal("12px", result.Tokens["button-hover-border-radius"]);
        Assert.NotEqual("#112233", result.Tokens["button-hover-background"]);
        Assert.Equal("0 0 8px #333333", result.Tokens["button-hover-box-shadow"]);
    }

    [Theory]
    [InlineData("position: fixed")]
    [InlineData("width: 100vw")]
    [InlineData("background: url(https://example.com/pixel)")]
    [InlineData("color: red !important")]
    [InlineData("color: red;} body {display:none")]
    [InlineData("color: </style><script>alert(1)</script>")]
    [InlineData("color: var(--broken")]
    [InlineData("@import 'bad'")]
    public void RejectsStructuralAndEscapingCss(string css) => Assert.Throws<ArgumentException>(() => VisualCss.Parse(css));

    [Fact]
    public void CssExportIncludesPortableRulesAndScopedAnimations()
    {
        ThemeDefinition theme = CloudThemes.Color("#007e87");
        theme.Animations["pulse"] = new();
        theme.Element(ThemeElements.Button).Normal.Animation = "pulse";
        string css = ThemeCss.Export(theme, scope: "client-preview");
        Assert.Contains(".client-preview{", css);
        Assert.Contains("@keyframes client-preview-animation-pulse", css);
        Assert.Contains("[data-cloud-element=", css);
        Assert.DoesNotContain("--cloudcommerce-", css);
        Assert.Contains("prefers-reduced-motion", css);
        Assert.DoesNotContain(":root", css);
    }

    [Fact]
    public void InvalidScopeAndAnimationCannotEscapeGeneratedStyle()
    {
        Assert.Throws<ArgumentException>(() => ThemeCss.Export(scope: "a}body{"));
        ThemeDefinition theme = CloudThemes.Grayscale();
        theme.Element(ThemeElements.Button).Normal.Animation = "missing";
        Assert.Throws<ArgumentException>(() => ThemeResolver.Resolve(theme));
    }

    [Fact]
    public void ModesResolveIndependentlyAndDoNotMutateDefinition()
    {
        ThemeDefinition theme = CloudThemes.Color("#6750a4");
        theme.ModeTokens[ThemeModes.Dark] = new() { ["color-surface"] = "#101010" };
        Assert.Equal("#101010", ThemeResolver.Resolve(theme, ThemeModes.Dark).Tokens["color-surface"]);
        Assert.Equal("#ffffff", ThemeResolver.Resolve(theme).Tokens["color-surface"]);
        Assert.Empty(theme.Elements);
    }
}


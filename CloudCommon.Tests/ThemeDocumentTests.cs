using AngryMonkey.CloudCommon.Theming;
using Xunit;

namespace AngryMonkey.CloudCommon.Tests;

public class ThemeDocumentTests
{
    [Fact]
    public void DocumentExportContainsIndependentModesAndOnlySharedTokenNamespace()
    {
        string css = ThemeCss.ExportDocument(CloudThemes.Color("#6750a4"), false);
        Assert.Contains(":root{color-scheme:light;", css);
        Assert.Contains(":root[data-amc-theme=dark]{color-scheme:dark;", css);
        Assert.Contains("@media(prefers-color-scheme:dark){:root[data-amc-theme=system]", css);
        Assert.DoesNotContain("--cloud-", css);
        Assert.DoesNotContain("--cloudcomponents-", css);
        Assert.DoesNotContain("--cloudlogin-", css);
        Assert.DoesNotContain("--cloudcommerce-", css);
        Assert.DoesNotContain("--cdm-", css);
    }

    [Theory]
    [InlineData("#06C", "#0066cc")]
    [InlineData("#ABC", "#aabbcc")]
    public void ExistingShortHexAccentSettingsRemainSupported(string input, string expected)
    {
        Assert.Equal(expected, ThemeResolver.Resolve(CloudThemes.Color(input)).Tokens["primary-500"]);
    }

    [Fact]
    public void DocumentAnimationReferencesResolveToDeclaredModeKeyframes()
    {
        ThemeDefinition theme = CloudThemes.Color("#6750a4");
        theme.Animations["pulse"] = new();
        theme.Element(ThemeElements.Button).Normal.Animation = "pulse";
        string css = ThemeCss.ExportDocument(theme, false);
        foreach (string scope in new[] { "amc-document", "amc-document-dark", "amc-document-system" })
        {
            Assert.Contains($"@keyframes {scope}-animation-pulse", css);
            Assert.Contains($"--amc-button-normal-animation:{scope}-animation-pulse ", css);
        }
    }
}

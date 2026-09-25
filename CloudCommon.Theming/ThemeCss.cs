using System.Reflection;
using System.Text;
namespace AngryMonkey.CloudCommon.Theming;

public static class ThemeCss
{
    private static readonly Lazy<string> Styles = new(() =>
    {
        using Stream stream = typeof(ThemeCss).Assembly.GetManifestResourceStream("CloudCommon.Elements.css") ?? throw new InvalidOperationException("Build theme assets before compiling.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    });
    public static string ElementStyles => Styles.Value;
    public static string Export(ThemeDefinition? definition = null, ThemeModes mode = ThemeModes.Light, string scope = "cloud-theme", bool includeElementStyles = true)
    {
        VisualCss.Identifier(scope);
        ThemeDefinition theme = definition ?? CloudThemes.Grayscale();
        ResolvedTheme resolved = ThemeResolver.Resolve(theme, mode);
        StringBuilder css = new();
        css.Append('.').Append(scope).Append("{color-scheme:").Append(mode == ThemeModes.Dark ? "dark" : "light").Append(';');
        foreach ((string key, string value) in resolved.Tokens)
            css.Append("--amc-").Append(key).Append(':').Append(value.Replace("cloud-animation-", $"{scope}-animation-", StringComparison.Ordinal)).Append(';');
        css.AppendLine("}");
        foreach ((string name, ThemeAnimation animation) in theme.Animations)
        {
            css.Append("@keyframes ").Append(scope).Append("-animation-").Append(name).Append("{from{");
            foreach ((string property, string value) in VisualCss.Parse(animation.From))
                css.Append(property).Append(':').Append(value).Append(';');
            css.Append("}to{");
            foreach ((string property, string value) in VisualCss.Parse(animation.To))
                css.Append(property).Append(':').Append(value).Append(';');
            css.AppendLine("}}");
        }
        if (includeElementStyles) css.Append(ElementStyles);
        return css.ToString();
    }

    public static string ExportDocument(ThemeDefinition? definition = null, bool includeElementStyles = true)
    {
        string light = Export(definition, ThemeModes.Light, "amc-document", false).Replace(".amc-document{", ":root{", StringComparison.Ordinal);
        string dark = Export(definition, ThemeModes.Dark, "amc-document-dark", false).Replace(".amc-document-dark{", ":root[data-amc-theme=dark]{", StringComparison.Ordinal);
        string system = Export(definition, ThemeModes.Dark, "amc-document-system", false).Replace(".amc-document-system{", ":root[data-amc-theme=system]{", StringComparison.Ordinal);
        return light + dark + "@media(prefers-color-scheme:dark){" + system + "}" + (includeElementStyles ? ElementStyles : "");
    }
}

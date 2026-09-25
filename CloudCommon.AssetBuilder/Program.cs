using System.Text.Json;
using AngryMonkey.CloudCommon.Theming;

string output = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
Directory.CreateDirectory(output);
File.WriteAllText(Path.Combine(output, "theme-defaults.json"), JsonSerializer.Serialize(ThemeResolver.Resolve().Tokens, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(output, "theme.css"), ThemeCss.ExportDocument());

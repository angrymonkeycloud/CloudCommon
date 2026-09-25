using AngryMonkey.CloudCommon.Demo.Components;
using AngryMonkey.CloudCommon.Theming;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCloudLogin("http://127.0.0.1:5188");
WebApplication app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/themes/grayscale.css", () => Results.Text(ThemeCss.Export(), "text/css"));
app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();


using ScrumPoker.Components;
using ScrumPoker.Services;

var builder = WebApplication.CreateBuilder(args);

// Listen on all interfaces so teammates on the same network can connect via your IP
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(5111));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<RoomService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

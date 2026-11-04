using Bewirtung.Webserver.Components;
using Bewirtung.Webserver.Data;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Deutsche Kultur erzwingen, damit Währungs-/Zahlenformatierung (z. B. "€")
// unabhängig von der Server-/Container-Kultur korrekt ist.
var deCulture = new CultureInfo("de-DE");
CultureInfo.DefaultThreadCurrentCulture = deCulture;
CultureInfo.DefaultThreadCurrentUICulture = deCulture;

// Blazor Server (interaktiv)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Datenzugriff
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<BenutzerRepository>();
builder.Services.AddScoped<VereinRepository>();
builder.Services.AddScoped<TischRepository>();
builder.Services.AddScoped<BereichTischeRepository>();
builder.Services.AddScoped<TerminRepository>();
builder.Services.AddScoped<SpeisekarteRepository>();
builder.Services.AddScoped<TischTerminRepository>();
builder.Services.AddScoped<GlobalePreislisteRepository>();
builder.Services.AddScoped<MwstRepository>();
builder.Services.AddScoped<AbrechnungRepository>();
builder.Services.AddScoped<WordVorlageRepository>();
builder.Services.AddScoped<PreislisteDokumentService>();
builder.Services.AddScoped<AbrechnungPdfService>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<SessionState>();

// Stirling-PDF: Umwandlung von .docx in .pdf über den Docker-Service.
var stirlingBaseUrl = builder.Configuration["StirlingPdf:BaseUrl"] ?? "http://host.docker.internal:8080";
builder.Services.AddHttpClient<StirlingPdfService>(client =>
{
    client.BaseAddress = new Uri(stirlingBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();

// Deutsche Kultur für alle Requests setzen (Währung "€", Dezimalkomma usw.).
var deCultures = new[] { deCulture };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(deCulture),
    SupportedCultures = deCultures,
    SupportedUICultures = deCultures
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using Bewirtung.Webserver.Components;
using Bewirtung.Webserver.Data;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

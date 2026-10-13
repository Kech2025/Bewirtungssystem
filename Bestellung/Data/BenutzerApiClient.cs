using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>Ein Benutzer zur Anmeldung (UserId + Name).</summary>
public class BenutzerDto
{
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}

/// <summary>
/// Ruft die Benutzerliste zur Anmeldung über die Web-API ab.
/// </summary>
public class BenutzerApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>Liefert die Benutzer (GET /api/benutzer).</summary>
    public async Task<List<BenutzerDto>> GetBenutzerAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/benutzer";
        var result = await _httpClient.GetFromJsonAsync<List<BenutzerDto>>(url, cancellationToken);
        return result ?? new List<BenutzerDto>();
    }

    /// <summary>
    /// Liefert die einem Termin zugeordneten Helfer/Benutzer
    /// (GET /api/benutzer/termin?datum=...&vereinId=...), gefiltert über die Tabelle HelferTermin.
    /// </summary>
    public async Task<List<BenutzerDto>> GetBenutzerFuerTerminAsync(int bewirtungsDatum, long vereinId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/benutzer/termin?datum={bewirtungsDatum}&vereinId={vereinId}";
        var result = await _httpClient.GetFromJsonAsync<List<BenutzerDto>>(url, cancellationToken);
        return result ?? new List<BenutzerDto>();
    }
}

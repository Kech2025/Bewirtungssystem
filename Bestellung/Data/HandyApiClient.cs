using System.Net;
using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>Ergebnis der Seriennummer-Validierung (Seriennummer + zugehöriger Name).</summary>
public class HandyDto
{
    public string Seriennummer { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>True, wenn für dieses Gerät der Bestellmodus erlaubt ist.</summary>
    public bool Bestellung { get; set; }
}

/// <summary>
/// Validiert eine Handy-Seriennummer gegen die Tabelle handy über die Web-API.
/// </summary>
public class HandyApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Prüft die Seriennummer (GET /api/handy/validate). Liefert bei Treffer das Handy
    /// (inkl. Name), sonst null (nicht gefunden).
    /// </summary>
    public async Task<HandyDto?> ValidiereSeriennummerAsync(
        string seriennummer, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/handy/validate?seriennummer={Uri.EscapeDataString(seriennummer)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HandyDto>(cancellationToken);
    }
}

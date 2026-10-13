using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Ruft die zum Termin ausgewählte Preisliste (Speisekarte) über die Web-API ab.
/// </summary>
public class SpeisekarteApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Liefert die Positionen der Preisliste für einen Termin
    /// (GET /api/speisekarte?datum=...&amp;vereinId=...).
    /// </summary>
    public async Task<List<SpeisekartePositionDto>> GetSpeisekarteAsync(
        int bewirtungsDatum, long vereinId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/speisekarte?datum={bewirtungsDatum}&vereinId={vereinId}";
        var result = await _httpClient.GetFromJsonAsync<List<SpeisekartePositionDto>>(url, cancellationToken);
        return result ?? new List<SpeisekartePositionDto>();
    }
}

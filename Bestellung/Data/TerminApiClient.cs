using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Ruft Termine (Datum + Verein) über die Web-API des lokalen Servers ab.
/// </summary>
public class TerminApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Liefert alle Termine vom Server (GET /api/termine), neueste zuerst.
    /// </summary>
    public async Task<List<TerminDto>> GetTermineAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/termine";
        var result = await _httpClient.GetFromJsonAsync<List<TerminDto>>(url, cancellationToken);
        return result ?? new List<TerminDto>();
    }
}

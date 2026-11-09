using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>Eine Küchenstation (Id + Name) zur Modus-Auswahl.</summary>
public class KuechenstationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}

/// <summary>
/// Lädt die verfügbaren Küchenstationen über die Web-API (GET /api/kuechenstationen).
/// </summary>
public class KuechenstationApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<List<KuechenstationDto>> GetKuechenstationenAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/kuechenstationen";
        var result = await _httpClient.GetFromJsonAsync<List<KuechenstationDto>>(url, cancellationToken);
        return result ?? new List<KuechenstationDto>();
    }

    /// <summary>
    /// Lädt die einem Termin (Datum + Verein) zugeordneten Küchenstationen.
    /// </summary>
    public async Task<List<KuechenstationDto>> GetKuechenstationenFuerTerminAsync(int bewirtungsDatum, long vereinId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/kuechenstationen?datum={bewirtungsDatum}&vereinId={vereinId}";
        var result = await _httpClient.GetFromJsonAsync<List<KuechenstationDto>>(url, cancellationToken);
        return result ?? new List<KuechenstationDto>();
    }
}

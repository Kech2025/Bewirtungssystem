using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Ein Tisch (Nummer + Bereich aus BereichTische) für die Auswahl.
/// </summary>
public class TischDto
{
    public int Tischnummer { get; set; }
    public string Bereich { get; set; } = string.Empty;

    /// <summary>Sofortlieferung: 0 = Nein, 1 = Ja.</summary>
    public int SofortLieferung { get; set; }

    /// <summary>Anzeigetext für die Auswahl, z. B. "Tisch 5 – Terrasse".</summary>
    public string Anzeige =>
        string.IsNullOrWhiteSpace(Bereich) ? $"Tisch {Tischnummer}" : $"Tisch {Tischnummer} – {Bereich}";

    public override string ToString() => Anzeige;
}

/// <summary>
/// Ruft Tisch-Daten über die Web-API des lokalen Servers ab.
/// </summary>
public class TischApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Liefert alle vorhandenen Tische (Nummer + Bereich) vom Server (GET /api/tische).
    /// </summary>
    public async Task<List<TischDto>> GetTischeAsync(int bewirtungsDatum, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/tische?datum={bewirtungsDatum}";
        var result = await _httpClient.GetFromJsonAsync<List<TischDto>>(url, cancellationToken);
        return result ?? new List<TischDto>();
    }
}

using System.ComponentModel;
using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Ruft offene Getränke-Auslieferungen je Tisch ab und meldet die Auslieferung zurück.
/// </summary>
public class AuslieferungApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Lädt offene Getränke-Positionen (Preiskategorie=1, MengeGeliefert &lt; Menge)
    /// gruppiert je Tisch für den angegebenen Termin.
    /// </summary>
    public async Task<List<AuslieferungTischDto>> GetGetraenkeAsync(int termin, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/auslieferung/getraenke?termin={termin}";
        var result = await _httpClient.GetFromJsonAsync<List<AuslieferungTischDto>>(url, cancellationToken);
        return result ?? new List<AuslieferungTischDto>();
    }

    /// <summary>
    /// Lädt offene Speisen-Positionen (Preiskategorie=0, MengeGeliefert &lt; Menge)
    /// gruppiert je Tisch für den angegebenen Termin.
    /// </summary>
    public async Task<List<AuslieferungTischDto>> GetSpeisenAsync(int termin, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/auslieferung/speisen?termin={termin}";
        var result = await _httpClient.GetFromJsonAsync<List<AuslieferungTischDto>>(url, cancellationToken);
        return result ?? new List<AuslieferungTischDto>();
    }

    /// <summary>
    /// Meldet die Auslieferung für einen Tisch: setzt je Position MengeGeliefert (und ggf. Menge).
    /// </summary>
    public async Task FertigAsync(IEnumerable<AuslieferungFertigPosition> positionen, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/auslieferung/fertig";
        var body = new { Positionen = positionen.ToList() };
        var response = await _httpClient.PostAsJsonAsync(url, body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

/// <summary>Ein Tisch mit seinen offenen Getränke-Positionen.</summary>
public class AuslieferungTischDto
{
    public int TischNummer { get; set; }
    public List<AuslieferungPositionDto> Positionen { get; set; } = new();

    public string TischTitel => $"Tisch {TischNummer}";
}

/// <summary>Eine offene Getränke-Position mit editierbarer Seitenmenge (Delta).</summary>
public class AuslieferungPositionDto : INotifyPropertyChanged
{
    public long AuftragId { get; set; }
    public long PosNr { get; set; }
    public string Produkt { get; set; } = string.Empty;
    public decimal Menge { get; set; }
    public decimal MengeGeliefert { get; set; }

    private int _seitenMenge;

    /// <summary>Auf der Seite erfasste zusätzliche Menge (Delta, Startwert 0).</summary>
    public int SeitenMenge
    {
        get => _seitenMenge;
        set
        {
            var begrenzt = value < 0 ? 0 : value;
            if (_seitenMenge != begrenzt)
            {
                _seitenMenge = begrenzt;
                OnPropertyChanged(nameof(SeitenMenge));
                OnPropertyChanged(nameof(SeitenMengeText));
            }
        }
    }

    public string SeitenMengeText => _seitenMenge.ToString();

    /// <summary>Anzeigetext: bestellte Menge und Produktname.</summary>
    public string AnzeigeText => $"{Menge:0.##} × {Produkt}";

    /// <summary>Anzeigetext der bereits gelieferten Menge, z. B. "3 von 5 geliefert".</summary>
    public string GeliefertText => $"{MengeGeliefert:0.##} von {Menge:0.##} geliefert";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Request-Position zum Melden der Auslieferung.</summary>
public class AuslieferungFertigPosition
{
    public long AuftragId { get; set; }
    public long PosNr { get; set; }
    public decimal SeitenMenge { get; set; }
}

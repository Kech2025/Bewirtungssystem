using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Sendet eine Bestellung (Bewirtungsauftrag + Positionen) an die Web-API.
/// </summary>
public class BestellungApiClient
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Schreibt den Auftrag samt Positionen (POST /api/bestellung) und liefert die neue AuftragId.
    /// </summary>
    public async Task<long> BestellungSendenAsync(
        BestellungRequest bestellung, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/bestellung";
        var response = await _httpClient.PostAsJsonAsync(url, bestellung, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BestellungResponse>(cancellationToken);
        return result?.AuftragId ?? 0;
    }

    /// <summary>
    /// Lädt den offenen Auftrag (Status = 0) für Termin/Tisch samt Positionen.
    /// Liefert null, wenn kein offener Auftrag existiert.
    /// </summary>
    public async Task<OffeneBestellungDto?> GetOffeneBestellungAsync(
        int termin, int tischNummer, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/bestellung/offen?termin={termin}&tisch={tischNummer}";
        return await _httpClient.GetFromJsonAsync<OffeneBestellungDto?>(url, cancellationToken);
    }

    /// <summary>
    /// Schließt einen Auftrag ab (Status = 1) und speichert den bezahlten Betrag.
    /// </summary>
    public async Task BezahlenAsync(long auftragId, decimal betrag, CancellationToken cancellationToken = default)
    {
        await BezahlenAsync(new List<long> { auftragId }, betrag, cancellationToken);
    }

    /// <summary>
    /// Schließt mehrere Aufträge ab (Status = 1) und speichert den bezahlten Betrag.
    /// </summary>
    public async Task BezahlenAsync(List<long> auftragIds, decimal betrag, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/bestellung/bezahlen";
        var auftragId = auftragIds is { Count: > 0 } ? auftragIds[0] : 0L;
        var response = await _httpClient.PostAsJsonAsync(
            url, new { AuftragId = auftragId, Betrag = betrag, AuftragIds = auftragIds }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Bezahlt eine Teilmenge je Position (Teilrechnung). Vollständig bezahlte Aufträge
    /// werden serverseitig auf Status = 1 gesetzt.
    /// </summary>
    public async Task TeilzahlenAsync(
        decimal betrag,
        IEnumerable<OffeneBestellungPosition> positionen,
        CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/bestellung/teilzahlen";
        var body = new
        {
            Betrag = betrag,
            Positionen = positionen
                .Where(p => p.TeilMenge > 0m)
                .Select(p => new { p.AuftragId, p.PosNr, MengeZahlen = p.TeilMenge })
                .ToList()
        };
        var response = await _httpClient.PostAsJsonAsync(url, body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

/// <summary>Offener Auftrag samt flacher Positionsliste.</summary>
public class OffeneBestellungDto
{
    public long AuftragId { get; set; }
    public List<long> AuftragIds { get; set; } = new();
    public decimal Gesamtpreis { get; set; }
    public List<OffeneBestellungPosition> Positionen { get; set; } = new();

    /// <summary>Alle abzuschließenden Auftrags-IDs (Fallback auf AuftragId).</summary>
    public List<long> AlleAuftragIds =>
        AuftragIds is { Count: > 0 } ? AuftragIds : new List<long> { AuftragId };

    /// <summary>Summe der Positionen (Menge × Preis); Fallback auf gespeicherten Gesamtpreis.</summary>
    public decimal Summe
    {
        get
        {
            var ausPositionen = Positionen.Sum(p => p.Menge * p.Preis);
            return ausPositionen > 0m ? ausPositionen : Gesamtpreis;
        }
    }
}

/// <summary>Eine Position eines offenen Auftrags (flach, mit Produktname).</summary>
public class OffeneBestellungPosition : System.ComponentModel.INotifyPropertyChanged
{
    public long PreislisteId { get; set; }
    public long ZusatzId { get; set; }
    public string Produkt { get; set; } = string.Empty;
    public decimal Preis { get; set; }
    public decimal Menge { get; set; }

    /// <summary>Bereits bezahlte Menge (aus BewirtungsPosition.MengeBezahlt).</summary>
    public decimal MengeBezahlt { get; set; }

    /// <summary>Auftrag und Positionsnummer zur eindeutigen Zuordnung (für Teilrechnung).</summary>
    public long AuftragId { get; set; }
    public long PosNr { get; set; }

    private decimal _teilMenge;

    /// <summary>Für die Teilrechnung aktuell gewählte zu zahlende Menge (0..offene Menge).</summary>
    public decimal TeilMenge
    {
        get => _teilMenge;
        set
        {
            var begrenzt = value < 0m ? 0m : (value > OffeneMenge ? OffeneMenge : value);
            if (_teilMenge != begrenzt)
            {
                _teilMenge = begrenzt;
                OnPropertyChanged(nameof(TeilMenge));
                OnPropertyChanged(nameof(TeilMengeText));
            }
        }
    }

    /// <summary>Noch offene (unbezahlte) Menge.</summary>
    public decimal OffeneMenge => Menge - MengeBezahlt;

    private static readonly System.Globalization.CultureInfo EuroKultur =
        System.Globalization.CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Anzeigetext der Zeile, z. B. "2 × Bier – 3,00 €".</summary>
    public string AnzeigeText =>
        $"{Menge:0.##} × {Produkt}";

    /// <summary>Anzeigetext für die Teilrechnung: "Produkt" + darunter "Bestellt / Bezahlt".</summary>
    public string MengenText =>
        $"{Menge:0.##} bestellt / {MengeBezahlt:0.##} bezahlt";

    /// <summary>Aktuell gewählte Teilmenge als Text.</summary>
    public string TeilMengeText => _teilMenge.ToString("0.##", EuroKultur);

    /// <summary>Zeilensumme als Euro-Text.</summary>
    public string SummeText => (Menge * Preis).ToString("C", EuroKultur);

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}

/// <summary>Anforderung zum Speichern einer Bestellung.</summary>
public class BestellungRequest
{
    public int Bewirtungstermin { get; set; }
    public long UserId { get; set; }
    public decimal Gesamtpreis { get; set; }
    public int TischNummer { get; set; }
    public List<BestellungPosition> Positionen { get; set; } = new();
}

/// <summary>Eine einzelne Bestellposition.</summary>
public class BestellungPosition
{
    public long PreislisteId { get; set; }
    public long ZusatzId { get; set; }
    public decimal Preis { get; set; }
    public decimal Menge { get; set; }
    public string Besonderheit { get; set; } = string.Empty;
}

/// <summary>Antwort der API nach dem Speichern.</summary>
public class BestellungResponse
{
    public long AuftragId { get; set; }
}

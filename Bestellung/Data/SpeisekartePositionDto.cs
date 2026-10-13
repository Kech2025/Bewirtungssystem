using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Bestellung.Data;

/// <summary>
/// Eine Position der zum Termin ausgewählten Preisliste (Produkt + Preis)
/// mit auswählbarer Menge.
/// </summary>
public class SpeisekartePositionDto : INotifyPropertyChanged
{
    private int _menge;

    public long PreislisteId { get; set; }
    public long ZusatzId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ZusatzText { get; set; } = string.Empty;
    public string Produkt { get; set; } = string.Empty;
    public string KategorieText { get; set; } = string.Empty;
    public long Preiskategorie { get; set; }
    public decimal Preis { get; set; }

    /// <summary>Freitext für Besonderheiten (Tabelle BewirtungsPosition, Feld Besonderheit).</summary>
    public string Besonderheit { get; set; } = string.Empty;

    /// <summary>Textfarbe des Produkts: Blau für Preiskategorie 0, sonst Standard.</summary>
    public Color ProduktFarbe => Preiskategorie == 0 ? Colors.Blue : Colors.Black;

    /// <summary>Kultur für die Euro-Formatierung (z. B. "1,50 €").</summary>
    private static readonly CultureInfo EuroKultur = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Preis formatiert als Euro-Währung.</summary>
    public string PreisText => Preis.ToString("C", EuroKultur);

    /// <summary>Vom Benutzer gewählte Menge.</summary>
    public int Menge
    {
        get => _menge;
        set
        {
            if (_menge != value)
            {
                _menge = value;
                OnPropertyChanged(nameof(Menge));
                OnPropertyChanged(nameof(SummeText));
            }
        }
    }

    /// <summary>Zwischensumme (Menge × Preis) als Text.</summary>
    public string SummeText => (Menge * Preis).ToString("C", EuroKultur);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

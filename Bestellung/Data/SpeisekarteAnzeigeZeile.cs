using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Bestellung.Data;

/// <summary>Art einer Anzeigezeile in der Preisliste.</summary>
public enum ZeilenArt
{
    /// <summary>Einzelnes Produkt ohne Untervarianten – mit +/- Buttons.</summary>
    Einzel,
    /// <summary>Gruppenkopf für ein Produkt mit mehreren Untervarianten – mit Aufklapp-Zeichen.</summary>
    Kopf,
    /// <summary>Untervariante innerhalb einer Gruppe – mit +/- Buttons, eingerückt.</summary>
    Variante
}

/// <summary>
/// Eine Zeile zur Anzeige in der Preisliste. Kann eine Einzelposition,
/// ein Gruppenkopf (mehrere Untervarianten) oder eine Untervariante sein.
/// </summary>
public class SpeisekarteAnzeigeZeile : INotifyPropertyChanged
{
    private bool _istErweitert;
    private bool _zeigeBesonderheit;

    public ZeilenArt Art { get; init; }

    /// <summary>Zugrundeliegende Position (bei Einzel/Variante); null beim Gruppenkopf.</summary>
    public SpeisekartePositionDto? Position { get; init; }

    /// <summary>Untervarianten (nur beim Gruppenkopf gesetzt).</summary>
    public List<SpeisekarteAnzeigeZeile> Varianten { get; init; } = new();

    /// <summary>Verweis auf den Gruppenkopf (nur bei Untervarianten gesetzt).</summary>
    public SpeisekarteAnzeigeZeile? Kopf { get; set; }

    /// <summary>Anzeigetext des Produkts bzw. der Variante.</summary>
    public string Produkt { get; init; } = string.Empty;

    public string PreisText => Position?.PreisText ?? string.Empty;

    public Color ProduktFarbe { get; init; } = Colors.Black;

    /// <summary>Einrückung: Untervarianten etwas eingerückt.</summary>
    public Thickness Einzug => Art == ZeilenArt.Variante ? new Thickness(24, 6, 0, 6) : new Thickness(0, 6);

    /// <summary>+/- Buttons nur bei Einzelposition oder Untervariante.</summary>
    public bool ZeigeMengenButtons => Art != ZeilenArt.Kopf;

    /// <summary>Aufklapp-Zeichen nur beim Gruppenkopf.</summary>
    public bool ZeigeExpander => Art == ZeilenArt.Kopf;

    /// <summary>
    /// Aktionsbutton (Spalte 0) für alle Zeilen anzeigen: beim Gruppenkopf zum
    /// Auf-/Zuklappen der Untervarianten, sonst zum Ein-/Ausblenden der Besonderheit.
    /// </summary>
    public bool ZeigeAktionsButton => true;

    /// <summary>
    /// Zeichen des Aktionsbuttons: beim Gruppenkopf das Aufklapp-Zeichen,
    /// sonst + (Besonderheit einblenden) bzw. − (ausblenden).
    /// </summary>
    public string AktionsZeichen =>
        Art == ZeilenArt.Kopf ? ExpanderZeichen : (_zeigeBesonderheit ? "−" : "+");

    /// <summary>
    /// Hintergrundfarbe des Aktionsbuttons: Grün beim Gruppenkopf (Unterpositionen öffnen),
    /// Gelb bei Einzel/Variante (Besonderheit öffnen).
    /// </summary>
    public Color AktionsButtonFarbe =>
        Art == ZeilenArt.Kopf ? Colors.Green : Colors.Gold;

    /// <summary>Besonderheit-Eingabe nur bei Einzel/Variante und wenn aufgeklappt.</summary>
    public bool ZeigeBesonderheit
    {
        get => _zeigeBesonderheit && Art != ZeilenArt.Kopf;
        set
        {
            if (_zeigeBesonderheit != value)
            {
                _zeigeBesonderheit = value;
                OnPropertyChanged(nameof(ZeigeBesonderheit));
                OnPropertyChanged(nameof(AktionsZeichen));
            }
        }
    }

    /// <summary>Freitext für Besonderheiten der zugrundeliegenden Position.</summary>
    public string Besonderheit
    {
        get => Position?.Besonderheit ?? string.Empty;
        set
        {
            if (Position is not null && Position.Besonderheit != value)
            {
                Position.Besonderheit = value ?? string.Empty;
                OnPropertyChanged(nameof(Besonderheit));
            }
        }
    }

    /// <summary>Beim Gruppenkopf anzuzeigendes Zeichen (+ zusammengeklappt, − aufgeklappt).</summary>
    public string ExpanderZeichen => _istErweitert ? "−" : "+";

    /// <summary>Gibt an, ob die Gruppe aufgeklappt ist.</summary>
    public bool IstErweitert
    {
        get => _istErweitert;
        set
        {
            if (_istErweitert != value)
            {
                _istErweitert = value;
                OnPropertyChanged(nameof(IstErweitert));
                OnPropertyChanged(nameof(ExpanderZeichen));
            }
        }
    }

    /// <summary>Menge der zugrundeliegenden Position (0 ohne Position).</summary>
    public int Menge
    {
        get => Position?.Menge ?? 0;
        set
        {
            if (Position is not null && Position.Menge != value)
            {
                Position.Menge = value;
                OnPropertyChanged(nameof(Menge));
                OnPropertyChanged(nameof(AnzeigeMenge));
                // Aggregierte Menge im Gruppenkopf aktualisieren.
                Kopf?.AktualisiereMengeAusVarianten();
            }
        }
    }

    /// <summary>
    /// Beim Gruppenkopf: Summe der Mengen aller Untervarianten.
    /// Bei Einzel/Variante: eigene Menge.
    /// </summary>
    public int AnzeigeMenge =>
        Art == ZeilenArt.Kopf ? Varianten.Sum(v => v.Menge) : Menge;

    /// <summary>Lässt den Gruppenkopf seine aggregierte Menge neu berechnen.</summary>
    public void AktualisiereMengeAusVarianten() =>
        OnPropertyChanged(nameof(AnzeigeMenge));

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

namespace Bestellung.Data;

/// <summary>
/// Ein Termin (Datum + Verein), wie er von der Server-API geliefert wird.
/// Bestimmt die aktuell gültige Preisliste/Speisekarte.
/// </summary>
public class TerminDto
{
    public int BewirtungsDatum { get; set; }
    public int VereinId { get; set; }
    public string VereinName { get; set; } = string.Empty;

    /// <summary>Datum als lesbarer Text (TT.MM.JJJJ).</summary>
    public string DatumText
    {
        get
        {
            var jahr = BewirtungsDatum / 10000;
            var monat = (BewirtungsDatum / 100) % 100;
            var tag = BewirtungsDatum % 100;

            return new DateTime(jahr, monat, tag).ToString("dd.MM.yyyy");

        }
    }

    /// <summary>Anzeigetext für die Auswahl.</summary>
    public string Anzeige => $"{DatumText} – {VereinName}";

    public override string ToString() => Anzeige;
}

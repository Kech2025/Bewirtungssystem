namespace Bestellung.Data;

/// <summary>
/// Ein Termin (Datum + Verein), wie er von der Server-API geliefert wird.
/// Bestimmt die aktuell gültige Preisliste/Speisekarte.
/// </summary>
public class TerminDto
{
    public int BewirtungsDatum { get; set; }
    public long VereinId { get; set; }
    public string VereinName { get; set; } = string.Empty;

    /// <summary>Datum als lesbarer Text (TT.MM.JJJJ).</summary>
    public string DatumText
    {
        get
        {
            var jahr = BewirtungsDatum / 10000;
            var monat = BewirtungsDatum / 100 % 100;
            var tag = BewirtungsDatum % 100;
            if (jahr < 1 || monat < 1 || monat > 12 || tag < 1 || tag > 31)
            {
                return BewirtungsDatum.ToString();
            }
            try
            {
                return new DateTime(jahr, monat, tag).ToString("dd.MM.yyyy");
            }
            catch
            {
                return BewirtungsDatum.ToString();
            }
        }
    }

    /// <summary>Anzeigetext für die Auswahl.</summary>
    public string Anzeige => $"{DatumText} – {VereinName}";

    public override string ToString() => Anzeige;
}

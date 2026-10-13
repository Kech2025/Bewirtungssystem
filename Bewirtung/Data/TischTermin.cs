namespace Bewirtung.Data
{
    /// <summary>
    /// Datenmodell für die Zuordnung eines Tisches zu einem Termin (Tabelle TischTermin).
    /// Zusammengesetzter Schlüssel aus Bewirtungsdatum (yyyyMMdd), VereinId und Tischnummer.
    /// </summary>
    public class TischTermin
    {
        public int Bewirtungsdatum { get; set; }
        public long VereinId { get; set; }
        public int Tischnummer { get; set; }

        public string VereinName { get; set; } = string.Empty;

        public string BereichName { get; set; } = string.Empty;

        public string DatumText => Termin.FromYyyyMmDd(Bewirtungsdatum).ToString("dd.MM.yyyy");

        public string TerminAnzeige => $"{DatumText} – {VereinName}";
    }
}

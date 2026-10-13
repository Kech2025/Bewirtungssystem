namespace Bewirtung.Data
{
    /// <summary>
    /// Eine Zeile der aktuellen Speisekarte: eine Position der globalen Preisliste
    /// bzw. eine ihrer Zusatz-Varianten, kombiniert mit dem Preis aus GlobalerPreis
    /// und (falls gespeichert) dem überschreibbaren Preis aus PreislisteTermin.
    /// </summary>
    public class SpeisekartenZeile
    {
        public long PreislisteId { get; set; }
        public long ZusatzId { get; set; }
        public string KategorieText { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ZusatzText { get; set; } = string.Empty;

        /// <summary>Preis aus GlobalerPreis der gültigen Zeitperiode (null wenn keiner hinterlegt).</summary>
        public decimal? GlobalPreis { get; set; }

        /// <summary>Kennzeichnet, ob die Position für den Termin ausgewählt ist.</summary>
        public bool Ausgewaehlt { get; set; }

        /// <summary>Überschreibbarer Preis für den Termin (Vorbelegung = GlobalPreis).</summary>
        public decimal TerminPreis { get; set; }

        public string Produkt =>
            string.IsNullOrEmpty(ZusatzText) ? Name : $"{Name} – {ZusatzText}";
    }

    /// <summary>
    /// Datenmodell für die Tabelle PreislisteTermin
    /// (Schlüssel: PreislisteId, PreislisteZusatzId, VereinId, Bewirtungsdatum).
    /// </summary>
    public class PreislisteTermin
    {
        public long PreislisteId { get; set; }
        public long PreislisteZusatzId { get; set; }
        public long VereinId { get; set; }
        public int Bewirtungsdatum { get; set; }
        public decimal Preis { get; set; }
    }
}

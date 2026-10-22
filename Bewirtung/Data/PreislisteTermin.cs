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
        public int DruckParent { get; set; }
        public int DruckClient { get; set; }
        public string Produkt =>
            string.IsNullOrEmpty(ZusatzText) ? Name : $"{Name} – {ZusatzText}";
    }

    public class FehlerAbrechnungZeile : SpeisekartenZeile
    {
        public string Tisch { get; set; }
        public string Bereich { get; set; }
        public DateTime Bewirtungszeitpunkt { get; set; }
        public string UserName { get; set; }
        public string ArtikelName { get; set; }
        public string Besonderheit { get; set; }
        public decimal Menge { get; set; }
        public decimal Gesamtpreis { get; set; }
        public decimal MengeGeliefert { get; set; }
        public decimal MengeBezahlt { get; set; }
        public bool chkLöschen { get; set; }
        public bool chkFehlerKorrigieren { get; set; }
        public long BewirtungsauftragId { get; set; }
        public long Bewirtungsposition { get; set; }
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

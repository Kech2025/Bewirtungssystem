namespace Bewirtung.Data
{
    /// <summary>
    /// Preiskategorie einer Position der globalen Preisliste.
    /// </summary>
    public enum Preiskategorie
    {
        Speisen = 0,
        Getraenke = 1
    }

    /// <summary>
    /// Datenmodell für eine Position der globalen Preisliste (Tabelle GlobalePreisliste).
    /// </summary>
    public class GlobalePreisliste
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Preiskategorie Preiskategorie { get; set; }
        public string BildPfad { get; set; } = string.Empty;
        public bool Druck { get; set; }

        /// <summary>
        /// Zusätzlicher beschreibender Text zur Position.
        /// </summary>
        public string Zusatztext { get; set; } = string.Empty;

        /// <summary>
        /// Aktuell gültiger Preis der Position (aus GlobalerPreis) zum gewählten Datum.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public decimal? Preis { get; set; }

        /// <summary>
        /// Startdatum (GueltigAb, yyyyMMdd) des gültigen Preises zum gewählten Datum.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public int StartDatum { get; set; }

        public DateTime? StartDatumWert =>
            StartDatum > 0 ? Termin.FromYyyyMmDd(StartDatum).Date : null;

        /// <summary>
        /// Enddatum (GueltigBis, yyyyMMdd) des gültigen Preises zum gewählten Datum.
        /// 0 kennzeichnet einen offenen (unbegrenzt gültigen) Preis.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public int GueltigBis { get; set; }

        public DateTime? GueltigBisWert =>
            GueltigBis > 0 ? Termin.FromYyyyMmDd(GueltigBis).Date : null;

        public string KategorieText =>
            Preiskategorie == Preiskategorie.Getraenke ? "Getränke" : "Speisen";
    }

    /// <summary>
    /// Zusatz-Position zu einer Position der globalen Preisliste
    /// (Tabelle GlobalePreislisteZusatz, SpeisekarteId = FK auf GlobalePreisliste.Id).
    /// </summary>
    public class GlobalePreislisteZusatz
    {
        public long Id { get; set; }
        public long SpeisekarteId { get; set; }
        public string Bezeichnung { get; set; } = string.Empty;
        public string BildPfad { get; set; } = string.Empty;
        public bool Druck { get; set; }

        /// <summary>
        /// Aktuell gültiger Preis der Zusatz-Position (aus GlobalerPreis) zum gewählten Datum.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public decimal? Preis { get; set; }

        /// <summary>
        /// Startdatum (GueltigAb, yyyyMMdd) des gültigen Preises zum gewählten Datum.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public int StartDatum { get; set; }

        public DateTime? StartDatumWert =>
            StartDatum > 0 ? Termin.FromYyyyMmDd(StartDatum).Date : null;

        /// <summary>
        /// Enddatum (GueltigBis, yyyyMMdd) des gültigen Preises zum gewählten Datum.
        /// 0 kennzeichnet einen offenen (unbegrenzt gültigen) Preis.
        /// Nur für die Anzeige im Grid, wird nicht persistiert.
        /// </summary>
        public int GueltigBis { get; set; }

        public DateTime? GueltigBisWert =>
            GueltigBis > 0 ? Termin.FromYyyyMmDd(GueltigBis).Date : null;
    }
}

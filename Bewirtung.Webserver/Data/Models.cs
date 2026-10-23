namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenmodell für einen Benutzer (Tabelle Benutzer).
    /// </summary>
    public class Benutzer
    {
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Administrator { get; set; }
        public string Telefon { get; set; } = string.Empty;
        public long? VereinId { get; set; }
        public string VereinName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vereinfachtes Vereins-Modell für die Auswahl.
    /// </summary>
    public class VereinOption
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vereinfachtes Benutzer-Modell für die Auswahl (z. B. Ansprechpartner).
    /// </summary>
    public class BenutzerOption
    {
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datenmodell für einen Verein (Tabelle Verein).
    /// </summary>
    public class Verein
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? AnsprechpartnerId { get; set; }
        public string AnsprechpartnerName { get; set; } = string.Empty;
        public string Strasse { get; set; } = string.Empty;
        public string Plz { get; set; } = string.Empty;
        public string Ort { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datenmodell für einen Tisch (Tabelle Tisch).
    /// </summary>
    public class Tisch
    {
        public int Tischnummer { get; set; }
        public long? BereichId { get; set; }
        public string BereichName { get; set; } = string.Empty;

        /// <summary>Sofortlieferung: 0 = Nein, 1 = Ja.</summary>
        public int SofortLieferung { get; set; }
    }

    /// <summary>
    /// Vereinfachtes Bereich-Modell für die Auswahl (Tabelle BereichTische).
    /// </summary>
    public class BereichOption
    {
        public long BereichId { get; set; }
        public string Bezeichnung { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datenmodell für einen Termin (Tabelle Termin).
    /// Zusammengesetzter Schlüssel aus BewirtungsDatum (yyyyMMdd) und VereinId.
    /// </summary>
    public class Termin
    {
        public int BewirtungsDatum { get; set; }
        public long VereinId { get; set; }
        public string VereinName { get; set; } = string.Empty;

        public DateTime Datum
        {
            get => FromYyyyMmDd(BewirtungsDatum);
            set => BewirtungsDatum = ToYyyyMmDd(value);
        }

        public string DatumText => Datum.ToString("dd.MM.yyyy");
        public string Anzeige => $"{DatumText} – {VereinName}";

        public static int ToYyyyMmDd(DateTime date) =>
            date.Year * 10000 + date.Month * 100 + date.Day;

        public static DateTime FromYyyyMmDd(int value)
        {
            var year = value / 10000;
            var month = value / 100 % 100;
            var day = value % 100;
            if (year < 1 || month < 1 || month > 12 || day < 1 || day > 31)
            {
                return DateTime.Today;
            }
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return DateTime.Today;
            }
        }
    }

    /// <summary>
    /// Eine Zeile der Speisekarte eines Termins (globale Position bzw. Zusatz-Variante
    /// kombiniert mit globalem Preis und überschreibbarem Terminpreis).
    /// </summary>
    public class SpeisekartenZeile
    {
        public long PreislisteId { get; set; }
        public long ZusatzId { get; set; }
        public string KategorieText { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ZusatzText { get; set; } = string.Empty;
        public decimal? GlobalPreis { get; set; }
        public bool Ausgewaehlt { get; set; }
        public decimal TerminPreis { get; set; }
        public int DruckParent { get; set; }
        public int DruckClient { get; set; }
        public string Produkt =>
            string.IsNullOrEmpty(ZusatzText) ? Name : $"{Name} – {ZusatzText}";
    }

    /// <summary>
    /// Datenmodell für eine Tisch-Termin-Zuordnung (Tabelle TischTermin).
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

    /// <summary>
    /// Preiskategorie einer Position der globalen Preisliste.
    /// </summary>
    public enum Preiskategorie
    {
        Speisen = 0,
        Getraenke = 1
    }

    /// <summary>
    /// Datenmodell für einen Mehrwertsteuersatz (Tabelle Mwst).
    /// Zuordnung über Preiskategorie (0 = Speisen, 1 = Getränke).
    /// </summary>
    public class MwstSatz
    {
        public int Preiskategorie { get; set; }

        /// <summary>Mehrwertsteuersatz in Prozent.</summary>
        public decimal Mwst { get; set; }

        public string KategorieText =>
            Preiskategorie == (int)Bewirtung.Webserver.Data.Preiskategorie.Getraenke ? "Getränke" : "Speisen";
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
        public string Zusatztext { get; set; } = string.Empty;

        public decimal? Preis { get; set; }
        public int StartDatum { get; set; }
        public int GueltigBis { get; set; }

        public DateTime? StartDatumWert =>
            StartDatum > 0 ? Termin.FromYyyyMmDd(StartDatum).Date : null;

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

        public decimal? Preis { get; set; }
        public int StartDatum { get; set; }
        public int GueltigBis { get; set; }

        public DateTime? StartDatumWert =>
            StartDatum > 0 ? Termin.FromYyyyMmDd(StartDatum).Date : null;
    }

    /// <summary>
    /// Datenmodell für einen Datensatz der Tabelle GlobalerPreis.
    /// Schlüssel: GlobalePreislisteId, ZusatzId, GueltigAb.
    /// GueltigBis = 0 kennzeichnet einen offenen (unbegrenzt gültigen) Preis.
    /// </summary>
    public class GlobalerPreis
    {
        public long GlobalePreislisteId { get; set; }
        public long ZusatzId { get; set; }
        public int GueltigAb { get; set; }
        public int GueltigBis { get; set; }
        public decimal Preis { get; set; }
    }

    /// <summary>
    /// Aggregierte Abrechnungszeile für die Tagesabrechnung (AbrechnungTagForm).
    /// </summary>
    public class AbrechnungZeile
    {
        public long UserId { get; set; }
        public string Benutzer { get; set; } = string.Empty;

        public long PreislisteId { get; set; }
        public long PreislisteZusatzId { get; set; }
        public string Artikel { get; set; } = string.Empty;

        public decimal Menge { get; set; }
        public string Besonderheit { get; set; } = string.Empty;
        public decimal Gesamtpreis { get; set; }
        public int Tischnummer { get; set; }

        public long BereichId { get; set; }
        public string BereichName { get; set; } = string.Empty;

        /// <summary>Mehrwertsteuerbetrag der Zeile.</summary>
        public decimal Mwst { get; set; }

        /// <summary>Preiskategorie der Position: 0 = Speisen, 1 = Getränke.</summary>
        public int Preiskategorie { get; set; }
    }

    /// <summary>
    /// Fehlerhafte Abrechnungszeile (fehlende/abweichende Liefer- bzw. Bezahlmengen)
    /// für die Fehlerkorrektur zu einem Termin (FehlerForm).
    /// </summary>
    public class FehlerAbrechnungZeile
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime Bewirtungszeitpunkt { get; set; }
        public string Tisch { get; set; } = string.Empty;
        public string Bereich { get; set; } = string.Empty;
        public string ArtikelName { get; set; } = string.Empty;
        public string Besonderheit { get; set; } = string.Empty;
        public decimal Menge { get; set; }
        public decimal Gesamtpreis { get; set; }
        public decimal MengeGeliefert { get; set; }
        public decimal MengeBezahlt { get; set; }
        public bool ChkLoeschen { get; set; }
        public bool ChkFehlerKorrigieren { get; set; }
        public long BewirtungsauftragId { get; set; }
        public long Bewirtungsposition { get; set; }
        public int DruckParent { get; set; }
        public int DruckClient { get; set; }
        public string ZeitpunktText =>
            Bewirtungszeitpunkt == DateTime.MinValue ? string.Empty : Bewirtungszeitpunkt.ToString("dd.MM.yyyy HH:mm");
    }

    /// <summary>
    /// Datenmodell für eine Word-Vorlage (Tabelle wordvorlage).
    /// </summary>
    public class WordVorlage
    {
        public long Id { get; set; }
        public string Dateiname { get; set; } = string.Empty;
        public byte[] Inhalt { get; set; } = Array.Empty<byte>();
    }
}

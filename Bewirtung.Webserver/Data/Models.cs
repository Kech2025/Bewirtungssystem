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
}

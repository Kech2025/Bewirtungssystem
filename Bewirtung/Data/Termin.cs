namespace Bewirtung.Data
{
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
}

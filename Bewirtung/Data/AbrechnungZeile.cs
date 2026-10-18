namespace Bewirtung.Data
{
    /// <summary>
    /// Aggregierte Abrechnungszeile für die Tagesabrechnung (AbrechnungTagForm).
    /// Fasst gleichartige Bewirtungspositionen (PreislisteId, PreislisteZusatzId,
    /// Besonderheit) je Benutzer und Tisch zusammen.
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

        /// <summary>Nettobetrag = Gesamtpreis * 100 / (100 + Mwst-Prozentsatz).</summary>
        public decimal Mwst { get; set; }

        /// <summary>Preiskategorie der Position: 0 = Speisen, 1 = Getränke.</summary>
        public int Preiskategorie { get; set; }
    }
}

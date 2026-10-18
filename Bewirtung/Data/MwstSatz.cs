namespace Bewirtung.Data
{
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
            Preiskategorie == (int)Bewirtung.Data.Preiskategorie.Getraenke ? "Getränke" : "Speisen";
    }
}

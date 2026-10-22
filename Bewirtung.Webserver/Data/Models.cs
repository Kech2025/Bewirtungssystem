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
}

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
}

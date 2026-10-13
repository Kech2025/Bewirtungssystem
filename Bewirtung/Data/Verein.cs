namespace Bewirtung.Data
{
    /// <summary>
    /// Datenmodell für einen Verein.
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

        public override string ToString() => Name;
    }
}

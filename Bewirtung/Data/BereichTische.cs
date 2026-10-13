namespace Bewirtung.Data
{
    /// <summary>
    /// Datenmodell für einen Tisch-Bereich (Tabelle BereichTische).
    /// </summary>
    public class BereichTische
    {
        public long BereichId { get; set; }
        public string Bezeichnung { get; set; } = string.Empty;

        public override string ToString() => Bezeichnung;
    }
}

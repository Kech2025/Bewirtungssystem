namespace Bewirtung.Data
{
    /// <summary>
    /// Datenmodell für einen Tisch (Tabelle Tisch).
    /// </summary>
    public class Tisch
    {
        public int Tischnummer { get; set; }
        public long? BereichId { get; set; }
        public string BereichName { get; set; } = string.Empty;

        public override string ToString() => Tischnummer.ToString();
    }
}

namespace Bewirtung.Fiskal.Tse
{
    /// <summary>
    /// Konfiguration für die Cloud-TSE (z. B. fiskaly / Deutsche Fiskal).
    /// Wird aus dem Konfigurationsabschnitt "CloudTse" gebunden.
    /// </summary>
    public class CloudTseOptions
    {
        public const string SectionName = "CloudTse";

        /// <summary>Basis-URL der Cloud-TSE-API.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>API-Key / Client-Id des Anbieters.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>API-Secret des Anbieters.</summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>Kennung der TSE-Instanz beim Anbieter.</summary>
        public string TseId { get; set; } = string.Empty;

        /// <summary>
        /// Wenn true, wird nicht der echte Dienst aufgerufen, sondern
        /// eine simulierte Signatur erzeugt (Entwicklung/Test).
        /// </summary>
        public bool DummyModus { get; set; } = true;
    }
}

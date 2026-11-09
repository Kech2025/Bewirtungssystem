namespace Bewirtung.Fiskal.Tse
{
    /// <summary>
    /// Anforderung zum Signieren eines Vorgangs durch die TSE.
    /// Enthält die fachlichen Daten, aus denen der TSE-Prozessdatenstring gebildet wird.
    /// </summary>
    public class TseSignaturAnfrage
    {
        /// <summary>Kennung der Kasse/des Clients (z. B. Handy-Seriennummer).</summary>
        public string KasseId { get; set; } = string.Empty;

        /// <summary>Prozesstyp laut TR-03153 (z. B. "Kassenbeleg-V1").</summary>
        public string ProzessTyp { get; set; } = "Kassenbeleg-V1";

        /// <summary>
        /// Die zu signierenden Prozessdaten (bei "Kassenbeleg-V1" z. B.
        /// "Beleg^BRUTTO_STEUER_19^...^Zahlart").
        /// </summary>
        public string ProzessDaten { get; set; } = string.Empty;

        /// <summary>Startzeitpunkt des Vorgangs.</summary>
        public DateTime StartZeitpunkt { get; set; }
    }

    /// <summary>Ergebnis einer TSE-Signatur. Wird 1:1 in <see cref="FiskalSignatur"/> übernommen.</summary>
    public class TseSignaturErgebnis
    {
        public bool Erfolgreich { get; set; }
        public string? Fehler { get; set; }

        public string TseSerienNummer { get; set; } = string.Empty;
        public long TransaktionsNummer { get; set; }
        public long SignaturZaehler { get; set; }
        public string Signatur { get; set; } = string.Empty;
        public string SignaturAlgorithmus { get; set; } = string.Empty;
        public string ZertifikatReferenz { get; set; } = string.Empty;
        public DateTime TseStartZeit { get; set; }
        public DateTime TseEndZeit { get; set; }
        public string ProzessTyp { get; set; } = string.Empty;
        public string QrCodeDaten { get; set; } = string.Empty;
    }

    /// <summary>
    /// Abstraktion über die Technische Sicherheitseinrichtung (TSE) gemäß KassenSichV.
    /// Erlaubt austauschbare Implementierungen (Cloud-TSE wie fiskaly / Deutsche Fiskal
    /// oder Hardware-TSE), ohne die Fiskalisierungslogik zu ändern.
    /// </summary>
    public interface ITseProvider
    {
        /// <summary>Technischer Name des Providers (für Logging/DSFinV-K).</summary>
        string ProviderName { get; }

        /// <summary>Signiert einen Geschäftsvorfall und liefert die TSE-Signaturdaten.</summary>
        Task<TseSignaturErgebnis> SigniereVorgangAsync(
            TseSignaturAnfrage anfrage,
            CancellationToken abbruch = default);
    }
}

namespace Bewirtung.Fiskal.Tse
{
    /// <summary>
    /// Platzhalter-Implementierung von <see cref="ITseProvider"/> OHNE echte Signatur.
    /// Dient nur als Gerüst, bis die Cloud-TSE (z. B. fiskaly / Deutsche Fiskal) angebunden ist.
    /// ACHTUNG: Diese Implementierung ist NICHT KassenSichV-konform und darf nicht
    /// im Echtbetrieb verwendet werden.
    /// </summary>
    public class NichtKonformerTseProvider : ITseProvider
    {
        private long _transaktionsNummer;
        private long _signaturZaehler;

        public string ProviderName => "Platzhalter (keine echte TSE)";

        public Task<TseSignaturErgebnis> SigniereVorgangAsync(
            TseSignaturAnfrage anfrage,
            CancellationToken abbruch = default)
        {
            var transaktion = Interlocked.Increment(ref _transaktionsNummer);
            var zaehler = Interlocked.Increment(ref _signaturZaehler);
            var start = anfrage.StartZeitpunkt == default ? DateTime.Now : anfrage.StartZeitpunkt;
            var ende = DateTime.Now;

            var ergebnis = new TseSignaturErgebnis
            {
                Erfolgreich = true,
                TseSerienNummer = "PLATZHALTER-TSE",
                TransaktionsNummer = transaktion,
                SignaturZaehler = zaehler,
                Signatur = string.Empty,
                SignaturAlgorithmus = "ecdsa-plain-SHA256",
                ZertifikatReferenz = string.Empty,
                TseStartZeit = start,
                TseEndZeit = ende,
                ProzessTyp = anfrage.ProzessTyp,
                QrCodeDaten = string.Empty
            };

            return Task.FromResult(ergebnis);
        }
    }
}

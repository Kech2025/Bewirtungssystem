using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bewirtung.Fiskal.Tse
{
    /// <summary>
    /// Cloud-TSE-Provider (z. B. fiskaly / Deutsche Fiskal).
    ///
    /// Aktuell arbeitet die Implementierung im DUMMY-Modus: sie erzeugt lokal
    /// realistisch aufgebaute Signaturdaten (Transaktions-/Signaturzähler,
    /// simulierte ECDSA-Signatur, QR-Code-Daten gemäß DSFinV-K-Struktur),
    /// OHNE einen echten TSE-Dienst aufzurufen.
    ///
    /// ACHTUNG: Im Dummy-Modus NICHT KassenSichV-konform. Für den Echtbetrieb
    /// ist <see cref="RufeCloudTseAufAsync"/> mit den HTTP-Aufrufen des Anbieters
    /// zu implementieren und <see cref="CloudTseOptions.DummyModus"/> auf false zu setzen.
    /// </summary>
    public class CloudTseProvider : ITseProvider
    {
        private readonly CloudTseOptions _options;
        private readonly ILogger<CloudTseProvider> _logger;

        // Simulierte, pro Prozess fortlaufende Zähler (echte TSE verwaltet diese intern).
        private static long _transaktionsNummer;
        private static long _signaturZaehler;

        public CloudTseProvider(IOptions<CloudTseOptions> options, ILogger<CloudTseProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string ProviderName =>
            _options.DummyModus ? "Cloud-TSE (DUMMY)" : "Cloud-TSE";

        public async Task<TseSignaturErgebnis> SigniereVorgangAsync(
            TseSignaturAnfrage anfrage,
            CancellationToken abbruch = default)
        {
            if (!_options.DummyModus)
            {
                return await RufeCloudTseAufAsync(anfrage, abbruch).ConfigureAwait(false);
            }

            return ErzeugeDummySignatur(anfrage);
        }

        /// <summary>
        /// Erzeugt eine lokal simulierte Signatur mit realistischem Aufbau.
        /// Die Signatur ist ein HMAC über die Prozessdaten (nur zu Testzwecken).
        /// </summary>
        private TseSignaturErgebnis ErzeugeDummySignatur(TseSignaturAnfrage anfrage)
        {
            var transaktion = Interlocked.Increment(ref _transaktionsNummer);
            var zaehler = Interlocked.Increment(ref _signaturZaehler);
            var start = anfrage.StartZeitpunkt == default ? DateTime.Now : anfrage.StartZeitpunkt;
            var ende = DateTime.Now;

            var tseSerie = string.IsNullOrWhiteSpace(_options.TseId) ? "DUMMY-TSE-0001" : _options.TseId;

            var signaturBasis =
                $"{tseSerie}|{transaktion}|{zaehler}|{anfrage.ProzessTyp}|{anfrage.ProzessDaten}|{start:O}|{ende:O}";
            var signatur = BerechneDummySignatur(signaturBasis);

            // QR-Code-Daten gemäß DSFinV-K-Struktur (vereinfacht, kompatibler Aufbau).
            var qr = string.Join(";",
                "V0",
                anfrage.ProzessTyp,
                anfrage.ProzessDaten,
                transaktion.ToString(),
                zaehler.ToString(),
                start.ToString("O"),
                ende.ToString("O"),
                "ecdsa-plain-SHA256",
                tseSerie,
                signatur);

            _logger.LogInformation(
                "Cloud-TSE DUMMY-Signatur erzeugt: Transaktion {Transaktion}, Zähler {Zaehler}, TSE {Tse}",
                transaktion, zaehler, tseSerie);

            return new TseSignaturErgebnis
            {
                Erfolgreich = true,
                TseSerienNummer = tseSerie,
                TransaktionsNummer = transaktion,
                SignaturZaehler = zaehler,
                Signatur = signatur,
                SignaturAlgorithmus = "ecdsa-plain-SHA256",
                ZertifikatReferenz = "DUMMY-CERT",
                TseStartZeit = start,
                TseEndZeit = ende,
                ProzessTyp = anfrage.ProzessTyp,
                QrCodeDaten = qr
            };
        }

        private static string BerechneDummySignatur(string basis)
        {
            // Fester Dummy-Schlüssel – ausschließlich für simulierte Signaturen.
            var schluessel = Encoding.UTF8.GetBytes("BEWIRTUNG-DUMMY-TSE-KEY");
            using var hmac = new HMACSHA256(schluessel);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(basis));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Platzhalter für die echte Cloud-TSE-Anbindung (fiskaly / Deutsche Fiskal).
        /// Hier werden später die HTTP-Aufrufe (Transaktion starten/finalisieren)
        /// implementiert. Bis dahin aktiv nur im Nicht-Dummy-Modus.
        /// </summary>
        private Task<TseSignaturErgebnis> RufeCloudTseAufAsync(
            TseSignaturAnfrage anfrage,
            CancellationToken abbruch)
        {
            throw new NotImplementedException(
                "Die echte Cloud-TSE-Anbindung ist noch nicht implementiert. " +
                "Setze CloudTse:DummyModus=true oder implementiere die Anbieter-HTTP-Aufrufe.");
        }
    }
}

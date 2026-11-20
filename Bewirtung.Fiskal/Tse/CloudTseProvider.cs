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

        private readonly TseKonfigurationRepository? _konfigRepo;
        private TseKonfiguration? _dbKonfig;
        private DateTime _dbKonfigGeladen;

        public CloudTseProvider(
            IOptions<CloudTseOptions> options,
            ILogger<CloudTseProvider> logger,
            TseKonfigurationRepository? konfigRepo = null)
        {
            _options = options.Value;
            _logger = logger;
            _konfigRepo = konfigRepo;
        }

        public string ProviderName =>
            _options.DummyModus && _dbKonfig == null ? "Cloud-TSE (DUMMY)" : "Cloud-TSE";

        public async Task<TseSignaturErgebnis> SigniereVorgangAsync(
            TseSignaturAnfrage anfrage,
            CancellationToken abbruch = default)
        {
            var zugang = await ErmittleZugangAsync(abbruch).ConfigureAwait(false);
            if (zugang != null)
            {
                return await RufeCloudTseAufAsync(anfrage, zugang, abbruch).ConfigureAwait(false);
            }

            return ErzeugeDummySignatur(anfrage);
        }

        /// <summary>
        /// Liefert die wirksamen Zugangsdaten: aktive Konfiguration aus der Datenbank
        /// (vorrangig, 30 s gecacht), sonst CloudTseOptions. null = Dummy-Modus.
        /// </summary>
        private async Task<TseKonfiguration?> ErmittleZugangAsync(CancellationToken abbruch)
        {
            if (_konfigRepo != null)
            {
                if (DateTime.UtcNow - _dbKonfigGeladen > TimeSpan.FromSeconds(30))
                {
                    try
                    {
                        var k = await _konfigRepo.LadeAktiveAsync(abbruch).ConfigureAwait(false);
                        _dbKonfig = k is { IstVollstaendig: true } ? k : null;
                        _dbKonfigGeladen = DateTime.UtcNow;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning(ex, "TSE-Konfiguration konnte nicht aus der Datenbank gelesen werden");
                    }
                }
                if (_dbKonfig != null)
                    return _dbKonfig;
            }

            if (_options.DummyModus)
                return null;

            return new TseKonfiguration
            {
                ApiKey = _options.ApiKey,
                ApiSecret = _options.ApiSecret,
                TssId = _options.TseId,
                ClientId = _options.ClientId
            };
        }

        private string BasisUrl => (string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "https://kassensichv-middleware.fiskaly.com/api/v2"
            : _options.BaseUrl.Trim()).TrimEnd('/');

        /// <summary>
        /// Prüft Zugangsdaten bei fiskaly (Anmeldung + Abruf der TSS). Liefert null bei Erfolg, sonst Fehlertext.
        /// </summary>
        public static async Task<string?> TesteVerbindungAsync(
            string apiKey, string apiSecret, string tssId, string? baseUrl = null, CancellationToken abbruch = default)
        {
            var basis = (string.IsNullOrWhiteSpace(baseUrl)
                ? "https://kassensichv-middleware.fiskaly.com/api/v2" : baseUrl.Trim()).TrimEnd('/');
            try
            {
                using var auth = await SendeAsync(HttpMethod.Post, $"{basis}/auth", null,
                    new { api_key = apiKey.Trim(), api_secret = apiSecret.Trim() }, abbruch).ConfigureAwait(false);
                var token = auth.RootElement.GetProperty("access_token").GetString();
                if (string.IsNullOrWhiteSpace(tssId))
                    return null;

                using var anfrage = new HttpRequestMessage(HttpMethod.Get, $"{basis}/tss/{tssId.Trim()}");
                anfrage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using var antwort = await Http.SendAsync(anfrage, abbruch).ConfigureAwait(false);
                var text = await antwort.Content.ReadAsStringAsync(abbruch).ConfigureAwait(false);
                if (!antwort.IsSuccessStatusCode)
                    return $"TSS nicht gefunden ({(int)antwort.StatusCode}).";
                using var doc = System.Text.Json.JsonDocument.Parse(text);
                var state = doc.RootElement.TryGetProperty("state", out var st) ? st.GetString() : null;
                return state == "INITIALIZED" ? null : $"TSS-Zustand ist {state}, erwartet INITIALIZED.";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return ex.Message.Contains("401") ? "Anmeldung fehlgeschlagen (API-Key/Secret falsch)." : ex.Message;
            }
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
        private async Task<TseSignaturErgebnis> RufeCloudTseAufAsync(
            TseSignaturAnfrage anfrage,
            TseKonfiguration zugang,
            CancellationToken abbruch)
        {
            try
            {
                var basis = BasisUrl;
                var token = await HoleTokenAsync(basis, zugang, abbruch).ConfigureAwait(false);
                var txId = Guid.NewGuid();
                var txUrl = $"{basis}/tss/{zugang.TssId.Trim()}/tx/{txId}";
                var clientId = zugang.ClientId.Trim();

                await SendeAsync(HttpMethod.Put, $"{txUrl}?tx_revision=1", token,
                    new { state = "ACTIVE", client_id = clientId }, abbruch).ConfigureAwait(false);

                var antwort = await SendeAsync(HttpMethod.Put, $"{txUrl}?tx_revision=2", token,
                    new
                    {
                        state = "FINISHED",
                        client_id = clientId,
                        schema = new
                        {
                            raw = new
                            {
                                process_type = anfrage.ProzessTyp,
                                process_data = Convert.ToBase64String(Encoding.UTF8.GetBytes(anfrage.ProzessDaten))
                            }
                        }
                    }, abbruch).ConfigureAwait(false);

                var r = antwort.RootElement;
                var sig = r.GetProperty("signature");
                return new TseSignaturErgebnis
                {
                    Erfolgreich = true,
                    TseSerienNummer = r.TryGetProperty("tss_serial_number", out var s) ? s.GetString() ?? string.Empty : zugang.TssId,
                    TransaktionsNummer = r.GetProperty("number").GetInt64(),
                    SignaturZaehler = sig.GetProperty("counter").GetInt64(),
                    Signatur = sig.GetProperty("value").GetString() ?? string.Empty,
                    SignaturAlgorithmus = sig.GetProperty("algorithm").GetString() ?? string.Empty,
                    ZertifikatReferenz = sig.TryGetProperty("public_key", out var pk) ? pk.GetString() ?? string.Empty : string.Empty,
                    TseStartZeit = DateTimeOffset.FromUnixTimeSeconds(r.GetProperty("time_start").GetInt64()).LocalDateTime,
                    TseEndZeit = DateTimeOffset.FromUnixTimeSeconds(r.GetProperty("time_end").GetInt64()).LocalDateTime,
                    ProzessTyp = anfrage.ProzessTyp,
                    QrCodeDaten = r.TryGetProperty("qr_code_data", out var qr) ? qr.GetString() ?? string.Empty : string.Empty
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "fiskaly-Signatur fehlgeschlagen");
                return new TseSignaturErgebnis { Erfolgreich = false, Fehler = ex.Message, ProzessTyp = anfrage.ProzessTyp };
            }
        }

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };
        private static readonly SemaphoreSlim TokenSperre = new(1, 1);
        private static string? _token;
        private static string? _tokenFuer;
        private static DateTime _tokenAblauf;

        private static async Task<string> HoleTokenAsync(string basis, TseKonfiguration zugang, CancellationToken abbruch)
        {
            await TokenSperre.WaitAsync(abbruch).ConfigureAwait(false);
            try
            {
                var schluessel = basis + "|" + zugang.ApiKey.Trim() + "|" + zugang.ApiSecret.Trim();
                if (_token != null && _tokenFuer == schluessel && DateTime.UtcNow < _tokenAblauf)
                    return _token;

                using var antwort = await SendeAsync(HttpMethod.Post, $"{basis}/auth", null,
                    new { api_key = zugang.ApiKey.Trim(), api_secret = zugang.ApiSecret.Trim() }, abbruch).ConfigureAwait(false);
                _tokenFuer = schluessel;
                _token = antwort.RootElement.GetProperty("access_token").GetString();
                var gueltig = antwort.RootElement.TryGetProperty("access_token_expires_in", out var e) ? e.GetInt64() : 3600;
                _tokenAblauf = DateTime.UtcNow.AddSeconds(Math.Max(60, gueltig - 300));
                return _token ?? throw new InvalidOperationException("fiskaly lieferte kein Token.");
            }
            finally
            {
                TokenSperre.Release();
            }
        }

        private static async Task<System.Text.Json.JsonDocument> SendeAsync(
            HttpMethod methode, string url, string? token, object inhalt, CancellationToken abbruch)
        {
            using var anfrage = new HttpRequestMessage(methode, url)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(inhalt), Encoding.UTF8, "application/json")
            };
            if (token != null)
                anfrage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var antwort = await Http.SendAsync(anfrage, abbruch).ConfigureAwait(false);
            var text = await antwort.Content.ReadAsStringAsync(abbruch).ConfigureAwait(false);
            if (!antwort.IsSuccessStatusCode)
                throw new HttpRequestException($"fiskaly {(int)antwort.StatusCode}: {text}");
            return System.Text.Json.JsonDocument.Parse(text);
        }
    }
}

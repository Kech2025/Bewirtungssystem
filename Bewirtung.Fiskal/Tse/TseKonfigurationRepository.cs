using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace Bewirtung.Fiskal.Tse
{
    /// <summary>Gespeicherte Cloud-TSE-Konfiguration einer Umgebung (test/live).</summary>
    public class TseKonfiguration
    {
        public string Umgebung { get; set; } = "test";
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string TssId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public bool Aktiv { get; set; }
        public DateTime GeaendertAm { get; set; }
        public string GeaendertVon { get; set; } = string.Empty;

        public bool IstVollstaendig =>
            !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ApiSecret) &&
            !string.IsNullOrWhiteSpace(TssId) && !string.IsNullOrWhiteSpace(ClientId);
    }

    /// <summary>
    /// Liest/schreibt die fiskaly-Konfiguration in der Tabelle fiskal_tse_konfiguration.
    /// API-Key und API-Secret werden mit AES-GCM verschlüsselt; der Master-Key
    /// (Base64, 32 Byte) kommt aus CloudTse:MasterKey (Kubernetes-Secret).
    /// </summary>
    public class TseKonfigurationRepository
    {
        public const string UmgebungTest = "test";
        public const string UmgebungLive = "live";

        private readonly string _connectionString;
        private readonly byte[]? _masterKey;
        private bool _tabelleGeprueft;

        public TseKonfigurationRepository(string connectionString, string? masterKeyBase64)
        {
            _connectionString = connectionString;
            if (!string.IsNullOrWhiteSpace(masterKeyBase64))
            {
                var key = Convert.FromBase64String(masterKeyBase64.Trim());
                if (key.Length != 32)
                    throw new InvalidOperationException("CloudTse:MasterKey muss 32 Byte (Base64) lang sein.");
                _masterKey = key;
            }
        }

        public bool VerschluesselungVerfuegbar => _masterKey != null;

        public async Task<List<TseKonfiguration>> LadeAlleAsync(CancellationToken abbruch = default)
        {
            await using var con = await OeffneAsync(abbruch).ConfigureAwait(false);
            await using var cmd = new NpgsqlCommand(
                "SELECT umgebung, api_key_enc, api_secret_enc, tss_id, client_id, aktiv, geaendert_am, geaendert_von " +
                "FROM public.fiskal_tse_konfiguration ORDER BY umgebung DESC", con);
            await using var r = await cmd.ExecuteReaderAsync(abbruch).ConfigureAwait(false);
            var liste = new List<TseKonfiguration>();
            while (await r.ReadAsync(abbruch).ConfigureAwait(false))
            {
                liste.Add(new TseKonfiguration
                {
                    Umgebung = r.GetString(0),
                    ApiKey = Entschluesseln(r.GetString(1)),
                    ApiSecret = Entschluesseln(r.GetString(2)),
                    TssId = r.GetString(3),
                    ClientId = r.GetString(4),
                    Aktiv = r.GetBoolean(5),
                    GeaendertAm = r.GetDateTime(6),
                    GeaendertVon = r.GetString(7)
                });
            }
            return liste;
        }

        public async Task<TseKonfiguration?> LadeAktiveAsync(CancellationToken abbruch = default)
        {
            var alle = await LadeAlleAsync(abbruch).ConfigureAwait(false);
            return alle.FirstOrDefault(k => k.Aktiv);
        }

        /// <summary>
        /// Speichert eine Umgebung. Leere ApiKey/ApiSecret-Werte lassen den gespeicherten Wert unverändert.
        /// </summary>
        public async Task SpeichernAsync(TseKonfiguration k, string geaendertVon, CancellationToken abbruch = default)
        {
            if (_masterKey == null)
                throw new InvalidOperationException("CloudTse:MasterKey ist nicht gesetzt – Speichern nicht möglich.");

            await using var con = await OeffneAsync(abbruch).ConfigureAwait(false);
            await using var tx = await con.BeginTransactionAsync(abbruch).ConfigureAwait(false);

            if (k.Aktiv)
            {
                await using var aus = new NpgsqlCommand(
                    "UPDATE public.fiskal_tse_konfiguration SET aktiv = false WHERE umgebung <> @u", con, tx);
                aus.Parameters.AddWithValue("u", k.Umgebung);
                await aus.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);
            }

            await using var cmd = new NpgsqlCommand(
                "UPDATE public.fiskal_tse_konfiguration SET " +
                "api_key_enc = CASE WHEN @key = '' THEN api_key_enc ELSE @key END, " +
                "api_secret_enc = CASE WHEN @sec = '' THEN api_secret_enc ELSE @sec END, " +
                "tss_id = @tss, client_id = @client, aktiv = @aktiv, geaendert_am = now(), geaendert_von = @von " +
                "WHERE umgebung = @u", con, tx);
            cmd.Parameters.AddWithValue("key", string.IsNullOrWhiteSpace(k.ApiKey) ? "" : Verschluesseln(k.ApiKey.Trim()));
            cmd.Parameters.AddWithValue("sec", string.IsNullOrWhiteSpace(k.ApiSecret) ? "" : Verschluesseln(k.ApiSecret.Trim()));
            cmd.Parameters.AddWithValue("tss", k.TssId.Trim());
            cmd.Parameters.AddWithValue("client", k.ClientId.Trim());
            cmd.Parameters.AddWithValue("aktiv", k.Aktiv);
            cmd.Parameters.AddWithValue("von", geaendertVon);
            cmd.Parameters.AddWithValue("u", k.Umgebung);
            await cmd.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);

            await tx.CommitAsync(abbruch).ConfigureAwait(false);
        }

        private async Task<NpgsqlConnection> OeffneAsync(CancellationToken abbruch)
        {
            var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync(abbruch).ConfigureAwait(false);
            if (!_tabelleGeprueft)
            {
                await using var cmd = new NpgsqlCommand(Schema, con);
                await cmd.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);
                _tabelleGeprueft = true;
            }
            return con;
        }

        private string Verschluesseln(string klartext)
        {
            var nonce = RandomNumberGenerator.GetBytes(12);
            var daten = Encoding.UTF8.GetBytes(klartext);
            var chiffre = new byte[daten.Length];
            var tag = new byte[16];
            using var aes = new AesGcm(_masterKey!, 16);
            aes.Encrypt(nonce, daten, chiffre, tag);
            return "v1:" + Convert.ToBase64String(nonce.Concat(tag).Concat(chiffre).ToArray());
        }

        private string Entschluesseln(string wert)
        {
            if (string.IsNullOrEmpty(wert) || _masterKey == null || !wert.StartsWith("v1:"))
                return string.Empty;
            try
            {
                var bytes = Convert.FromBase64String(wert[3..]);
                var nonce = bytes.AsSpan(0, 12);
                var tag = bytes.AsSpan(12, 16);
                var chiffre = bytes.AsSpan(28);
                var klar = new byte[chiffre.Length];
                using var aes = new AesGcm(_masterKey, 16);
                aes.Decrypt(nonce, chiffre, tag, klar);
                return Encoding.UTF8.GetString(klar);
            }
            catch (CryptographicException)
            {
                return string.Empty;
            }
        }

        private const string Schema = """
            CREATE TABLE IF NOT EXISTS public.fiskal_tse_konfiguration (
                umgebung        varchar(10)  PRIMARY KEY CHECK (umgebung IN ('test', 'live')),
                api_key_enc     text         NOT NULL DEFAULT '',
                api_secret_enc  text         NOT NULL DEFAULT '',
                tss_id          varchar(64)  NOT NULL DEFAULT '',
                client_id       varchar(64)  NOT NULL DEFAULT '',
                aktiv           boolean      NOT NULL DEFAULT false,
                geaendert_am    timestamptz  NOT NULL DEFAULT now(),
                geaendert_von   varchar(200) NOT NULL DEFAULT ''
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ux_fiskal_tse_konfiguration_aktiv
                ON public.fiskal_tse_konfiguration (aktiv) WHERE aktiv;
            INSERT INTO public.fiskal_tse_konfiguration (umgebung) VALUES ('test'), ('live')
            ON CONFLICT (umgebung) DO NOTHING;
            """;
    }
}

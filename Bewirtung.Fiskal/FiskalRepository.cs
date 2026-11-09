using Bewirtung.Fiskal.Tse;
using Npgsql;

namespace Bewirtung.Fiskal
{
    /// <summary>
    /// Datenzugriff für die Fiskalisierung gemäß KassenSichV / DSFinV-K
    /// (Tabellen fiskal_beleg, fiskal_position, fiskal_steuer, fiskal_signatur,
    /// fiskal_abschluss).
    ///
    /// Grundsatz: Belege sind unveränderbar. Ein Beleg wird in einer einzigen
    /// Datenbank-Transaktion zusammen mit seinen Positionen, der Steueraufteilung
    /// und der TSE-Signatur gespeichert. Stornos/Korrekturen werden als eigene
    /// Belege mit Verweis auf den Ursprungsbeleg angelegt.
    /// </summary>
    public class FiskalRepository
    {
        private readonly FiskalDatabase _database;
        private readonly ITseProvider _tse;

        public FiskalRepository(FiskalDatabase database, ITseProvider tse)
        {
            _database = database;
            _tse = tse;
        }

        /// <summary>
        /// Signiert einen Beleg über die TSE und speichert ihn vollständig
        /// (Kopf, Positionen, Steueraufteilung, Signatur) in einer Transaktion.
        /// Die fortlaufende Belegnummer je Kasse wird dabei lückenlos vergeben.
        /// </summary>
        public async Task<FiskalBeleg> SpeichereBelegMitSignaturAsync(
            FiskalBeleg beleg,
            CancellationToken abbruch = default)
        {
            if (beleg.StartZeitpunkt == default)
            {
                beleg.StartZeitpunkt = DateTime.Now;
            }

            // 1. TSE-Signatur anfordern (vor dem Schreiben, damit bei Fehler nichts persistiert wird).
            var anfrage = new TseSignaturAnfrage
            {
                KasseId = beleg.KasseId,
                ProzessTyp = "Kassenbeleg-V1",
                ProzessDaten = BaueProzessDaten(beleg),
                StartZeitpunkt = beleg.StartZeitpunkt
            };
            var tseErgebnis = await _tse.SigniereVorgangAsync(anfrage, abbruch).ConfigureAwait(false);
            if (!tseErgebnis.Erfolgreich)
            {
                throw new InvalidOperationException(
                    $"TSE-Signatur fehlgeschlagen ({_tse.ProviderName}): {tseErgebnis.Fehler}");
            }

            beleg.EndZeitpunkt = tseErgebnis.TseEndZeit == default ? DateTime.Now : tseErgebnis.TseEndZeit;
            beleg.Status = FiskalBelegStatus.Abgeschlossen;

            await using var connection = await _database.CreateConnectionAsync(abbruch).ConfigureAwait(false);
            await using var transaktion = await connection.BeginTransactionAsync(abbruch).ConfigureAwait(false);
            try
            {
                beleg.BelegNummer = await NaechsteBelegNummerAsync(connection, transaktion, beleg.KasseId, abbruch)
                    .ConfigureAwait(false);

                beleg.Id = await InsertBelegAsync(connection, transaktion, beleg, abbruch).ConfigureAwait(false);

                foreach (var position in beleg.Positionen)
                {
                    position.BelegId = beleg.Id;
                    await InsertPositionAsync(connection, transaktion, position, abbruch).ConfigureAwait(false);
                }

                foreach (var steuer in beleg.SteuerPositionen)
                {
                    steuer.BelegId = beleg.Id;
                    await InsertSteuerAsync(connection, transaktion, steuer, abbruch).ConfigureAwait(false);
                }

                var signatur = ErstelleSignatur(beleg.Id, tseErgebnis);
                beleg.Signatur = signatur;
                await InsertSignaturAsync(connection, transaktion, signatur, abbruch).ConfigureAwait(false);

                await transaktion.CommitAsync(abbruch).ConfigureAwait(false);
                return beleg;
            }
            catch
            {
                await transaktion.RollbackAsync(abbruch).ConfigureAwait(false);
                throw;
            }
        }

        private static string BaueProzessDaten(FiskalBeleg beleg)
        {
            // Vereinfachte Prozessdaten-Darstellung für "Kassenbeleg-V1".
            // Die echte Formatierung übernimmt später der konkrete TSE-Provider
            // bzw. die Cloud-TSE anhand der DSFinV-K-Vorgaben.
            var brutto = beleg.GesamtBrutto.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var zahlart = beleg.Zahlart == FiskalZahlart.Bar ? "Bar" : "Unbar";
            return $"Beleg^{brutto}^{zahlart}";
        }

        private static FiskalSignatur ErstelleSignatur(long belegId, TseSignaturErgebnis e) => new()
        {
            BelegId = belegId,
            TseSerienNummer = e.TseSerienNummer,
            TransaktionsNummer = e.TransaktionsNummer,
            SignaturZaehler = e.SignaturZaehler,
            Signatur = e.Signatur,
            SignaturAlgorithmus = e.SignaturAlgorithmus,
            ZertifikatReferenz = e.ZertifikatReferenz,
            TseStartZeit = e.TseStartZeit,
            TseEndZeit = e.TseEndZeit,
            ProzessTyp = e.ProzessTyp,
            QrCodeDaten = e.QrCodeDaten
        };

        private static async Task<long> NaechsteBelegNummerAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaktion,
            string kasseId,
            CancellationToken abbruch)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaktion;
            command.CommandText =
                "SELECT COALESCE(MAX(belegnummer), 0) + 1 FROM fiskal_beleg WHERE kasseid = @kasseid;";
            command.Parameters.AddWithValue("kasseid", kasseId);
            var result = await command.ExecuteScalarAsync(abbruch).ConfigureAwait(false);
            return Convert.ToInt64(result!);
        }

        private static async Task<long> InsertBelegAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaktion,
            FiskalBeleg beleg,
            CancellationToken abbruch)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaktion;
            command.CommandText = @"
                INSERT INTO fiskal_beleg
                    (belegnummer, kasseid, bediener, vorgangstyp, status, zahlart,
                     startzeitpunkt, endzeitpunkt, gesamtbrutto, ursprungsbelegid)
                VALUES
                    (@belegnummer, @kasseid, @bediener, @vorgangstyp, @status, @zahlart,
                     @startzeitpunkt, @endzeitpunkt, @gesamtbrutto, @ursprungsbelegid)
                RETURNING id;";
            command.Parameters.AddWithValue("belegnummer", beleg.BelegNummer);
            command.Parameters.AddWithValue("kasseid", beleg.KasseId);
            command.Parameters.AddWithValue("bediener", (object?)beleg.Bediener ?? DBNull.Value);
            command.Parameters.AddWithValue("vorgangstyp", (int)beleg.VorgangsTyp);
            command.Parameters.AddWithValue("status", (int)beleg.Status);
            command.Parameters.AddWithValue("zahlart", (int)beleg.Zahlart);
            command.Parameters.AddWithValue("startzeitpunkt", beleg.StartZeitpunkt);
            command.Parameters.AddWithValue("endzeitpunkt", (object?)beleg.EndZeitpunkt ?? DBNull.Value);
            command.Parameters.AddWithValue("gesamtbrutto", beleg.GesamtBrutto);
            command.Parameters.AddWithValue("ursprungsbelegid", (object?)beleg.UrsprungsBelegId ?? DBNull.Value);
            var id = await command.ExecuteScalarAsync(abbruch).ConfigureAwait(false);
            return Convert.ToInt64(id!);
        }

        private static async Task InsertPositionAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaktion,
            FiskalBelegPosition position,
            CancellationToken abbruch)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaktion;
            command.CommandText = @"
                INSERT INTO fiskal_position
                    (belegid, positionsnummer, artikel, menge, einzelpreisbrutto, gesamtbrutto, mwstsatz)
                VALUES
                    (@belegid, @positionsnummer, @artikel, @menge, @einzelpreisbrutto, @gesamtbrutto, @mwstsatz);";
            command.Parameters.AddWithValue("belegid", position.BelegId);
            command.Parameters.AddWithValue("positionsnummer", position.PositionsNummer);
            command.Parameters.AddWithValue("artikel", position.Artikel);
            command.Parameters.AddWithValue("menge", position.Menge);
            command.Parameters.AddWithValue("einzelpreisbrutto", position.EinzelpreisBrutto);
            command.Parameters.AddWithValue("gesamtbrutto", position.GesamtBrutto);
            command.Parameters.AddWithValue("mwstsatz", position.MwstSatz);
            await command.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);
        }

        private static async Task InsertSteuerAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaktion,
            FiskalSteuerPosition steuer,
            CancellationToken abbruch)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaktion;
            command.CommandText = @"
                INSERT INTO fiskal_steuer
                    (belegid, steuerschluessel, mwstsatz, brutto, netto, steuer)
                VALUES
                    (@belegid, @steuerschluessel, @mwstsatz, @brutto, @netto, @steuer);";
            command.Parameters.AddWithValue("belegid", steuer.BelegId);
            command.Parameters.AddWithValue("steuerschluessel", steuer.SteuerSchluessel);
            command.Parameters.AddWithValue("mwstsatz", steuer.MwstSatz);
            command.Parameters.AddWithValue("brutto", steuer.Brutto);
            command.Parameters.AddWithValue("netto", steuer.Netto);
            command.Parameters.AddWithValue("steuer", steuer.Steuer);
            await command.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);
        }

        private static async Task InsertSignaturAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaktion,
            FiskalSignatur signatur,
            CancellationToken abbruch)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaktion;
            command.CommandText = @"
                INSERT INTO fiskal_signatur
                    (belegid, tseseriennummer, transaktionsnummer, signaturzaehler, signatur,
                     signaturalgorithmus, zertifikatreferenz, tsestartzeit, tseendzeit, prozesstyp, qrcodedaten)
                VALUES
                    (@belegid, @tseseriennummer, @transaktionsnummer, @signaturzaehler, @signatur,
                     @signaturalgorithmus, @zertifikatreferenz, @tsestartzeit, @tseendzeit, @prozesstyp, @qrcodedaten);";
            command.Parameters.AddWithValue("belegid", signatur.BelegId);
            command.Parameters.AddWithValue("tseseriennummer", signatur.TseSerienNummer);
            command.Parameters.AddWithValue("transaktionsnummer", signatur.TransaktionsNummer);
            command.Parameters.AddWithValue("signaturzaehler", signatur.SignaturZaehler);
            command.Parameters.AddWithValue("signatur", (object?)signatur.Signatur ?? DBNull.Value);
            command.Parameters.AddWithValue("signaturalgorithmus", (object?)signatur.SignaturAlgorithmus ?? DBNull.Value);
            command.Parameters.AddWithValue("zertifikatreferenz", (object?)signatur.ZertifikatReferenz ?? DBNull.Value);
            command.Parameters.AddWithValue("tsestartzeit", (object?)signatur.TseStartZeit ?? DBNull.Value);
            command.Parameters.AddWithValue("tseendzeit", (object?)signatur.TseEndZeit ?? DBNull.Value);
            command.Parameters.AddWithValue("prozesstyp", (object?)signatur.ProzessTyp ?? DBNull.Value);
            command.Parameters.AddWithValue("qrcodedaten", (object?)signatur.QrCodeDaten ?? DBNull.Value);
            await command.ExecuteNonQueryAsync(abbruch).ConfigureAwait(false);
        }

        // ----- Lesen -----

        /// <summary>Lädt alle Belege eines Zeitraums (ohne Positionen) für Übersicht/Export.</summary>
        public List<FiskalBeleg> GetBelege(DateTime vonInklusive, DateTime bisExklusive, string? kasseId = null)
        {
            var result = new List<FiskalBeleg>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, belegnummer, kasseid, bediener, vorgangstyp, status, zahlart,
                       startzeitpunkt, endzeitpunkt, gesamtbrutto, ursprungsbelegid
                FROM fiskal_beleg
                WHERE startzeitpunkt >= @von AND startzeitpunkt < @bis
                  AND (@kasseid IS NULL OR kasseid = @kasseid)
                ORDER BY kasseid, belegnummer;";
            command.Parameters.AddWithValue("von", vonInklusive);
            command.Parameters.AddWithValue("bis", bisExklusive);
            command.Parameters.AddWithValue("kasseid", (object?)kasseId ?? DBNull.Value);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapBeleg(reader));
            }
            return result;
        }

        private static FiskalBeleg MapBeleg(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt64(0),
            BelegNummer = reader.GetInt64(1),
            KasseId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Bediener = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            VorgangsTyp = (FiskalVorgangsTyp)reader.GetInt32(4),
            Status = (FiskalBelegStatus)reader.GetInt32(5),
            Zahlart = (FiskalZahlart)reader.GetInt32(6),
            StartZeitpunkt = reader.GetDateTime(7),
            EndZeitpunkt = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
            GesamtBrutto = reader.GetDecimal(9),
            UrsprungsBelegId = reader.IsDBNull(10) ? null : reader.GetInt64(10)
        };

        /// <summary>
        /// Lädt alle Belege eines Zeitraums vollständig (inkl. Positionen, Steueraufteilung
        /// und TSE-Signatur) – Grundlage für den DSFinV-K-Export.
        /// </summary>
        public List<FiskalBeleg> GetBelegeMitDetails(DateTime vonInklusive, DateTime bisExklusive, string? kasseId = null)
        {
            var belege = GetBelege(vonInklusive, bisExklusive, kasseId);
            if (belege.Count == 0)
            {
                return belege;
            }

            var nachId = belege.ToDictionary(b => b.Id);
            using var connection = _database.CreateConnection();

            // Positionen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT p.id, p.belegid, p.positionsnummer, p.artikel, p.menge,
                           p.einzelpreisbrutto, p.gesamtbrutto, p.mwstsatz
                    FROM fiskal_position p
                    JOIN fiskal_beleg b ON b.id = p.belegid
                    WHERE b.startzeitpunkt >= @von AND b.startzeitpunkt < @bis
                      AND (@kasseid IS NULL OR b.kasseid = @kasseid)
                    ORDER BY p.belegid, p.positionsnummer;";
                command.Parameters.AddWithValue("von", vonInklusive);
                command.Parameters.AddWithValue("bis", bisExklusive);
                command.Parameters.AddWithValue("kasseid", (object?)kasseId ?? DBNull.Value);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var belegId = reader.GetInt64(1);
                    if (nachId.TryGetValue(belegId, out var beleg))
                    {
                        beleg.Positionen.Add(new FiskalBelegPosition
                        {
                            Id = reader.GetInt64(0),
                            BelegId = belegId,
                            PositionsNummer = reader.GetInt32(2),
                            Artikel = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Menge = reader.GetDecimal(4),
                            EinzelpreisBrutto = reader.GetDecimal(5),
                            GesamtBrutto = reader.GetDecimal(6),
                            MwstSatz = reader.GetDecimal(7)
                        });
                    }
                }
            }

            // Steueraufteilung
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT s.id, s.belegid, s.steuerschluessel, s.mwstsatz, s.brutto, s.netto, s.steuer
                    FROM fiskal_steuer s
                    JOIN fiskal_beleg b ON b.id = s.belegid
                    WHERE b.startzeitpunkt >= @von AND b.startzeitpunkt < @bis
                      AND (@kasseid IS NULL OR b.kasseid = @kasseid)
                    ORDER BY s.belegid, s.steuerschluessel;";
                command.Parameters.AddWithValue("von", vonInklusive);
                command.Parameters.AddWithValue("bis", bisExklusive);
                command.Parameters.AddWithValue("kasseid", (object?)kasseId ?? DBNull.Value);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var belegId = reader.GetInt64(1);
                    if (nachId.TryGetValue(belegId, out var beleg))
                    {
                        beleg.SteuerPositionen.Add(new FiskalSteuerPosition
                        {
                            Id = reader.GetInt64(0),
                            BelegId = belegId,
                            SteuerSchluessel = reader.GetInt32(2),
                            MwstSatz = reader.GetDecimal(3),
                            Brutto = reader.GetDecimal(4),
                            Netto = reader.GetDecimal(5),
                            Steuer = reader.GetDecimal(6)
                        });
                    }
                }
            }

            // Signaturen
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT g.id, g.belegid, g.tseseriennummer, g.transaktionsnummer, g.signaturzaehler,
                           g.signatur, g.signaturalgorithmus, g.zertifikatreferenz,
                           g.tsestartzeit, g.tseendzeit, g.prozesstyp, g.qrcodedaten
                    FROM fiskal_signatur g
                    JOIN fiskal_beleg b ON b.id = g.belegid
                    WHERE b.startzeitpunkt >= @von AND b.startzeitpunkt < @bis
                      AND (@kasseid IS NULL OR b.kasseid = @kasseid);";
                command.Parameters.AddWithValue("von", vonInklusive);
                command.Parameters.AddWithValue("bis", bisExklusive);
                command.Parameters.AddWithValue("kasseid", (object?)kasseId ?? DBNull.Value);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var belegId = reader.GetInt64(1);
                    if (nachId.TryGetValue(belegId, out var beleg))
                    {
                        beleg.Signatur = new FiskalSignatur
                        {
                            Id = reader.GetInt64(0),
                            BelegId = belegId,
                            TseSerienNummer = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            TransaktionsNummer = reader.GetInt64(3),
                            SignaturZaehler = reader.GetInt64(4),
                            Signatur = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            SignaturAlgorithmus = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            ZertifikatReferenz = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                            TseStartZeit = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                            TseEndZeit = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                            ProzessTyp = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                            QrCodeDaten = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                        };
                    }
                }
            }

            return belege;
        }
    }
}

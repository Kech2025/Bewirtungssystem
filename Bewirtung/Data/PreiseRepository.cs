using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Eine Zeile zur Preisverwaltung: eine Position der globalen Preisliste bzw.
    /// eine ihrer Zusatz-Varianten mit dem aktuell gültigen Preis aus GlobalerPreis.
    /// </summary>
    public class PreisZeile
    {
        public long PreislisteId { get; set; }
        public long ZusatzId { get; set; }
        public string KategorieText { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ZusatzText { get; set; } = string.Empty;
        public decimal Preis { get; set; }
    }

    /// <summary>
    /// Datenmodell für einen Datensatz der Tabelle GlobalerPreis.
    /// Schlüssel: GlobalePreislisteId, ZusatzId, GueltigAb.
    /// GueltigBis = 0 kennzeichnet einen offenen (unbegrenzt gültigen) Preis.
    /// </summary>
    public class GlobalerPreis
    {
        public long GlobalePreislisteId { get; set; }
        public long ZusatzId { get; set; }
        public int GueltigAb { get; set; }
        public int GueltigBis { get; set; }
        public decimal Preis { get; set; }
    }

    /// <summary>
    /// Datenzugriff zur Verwaltung der Preise in der Tabelle GlobalerPreis
    /// (Schlüssel GlobalePreislisteId + ZusatzId, Gültigkeit über GueltigAb/GueltigBis,
    /// wobei GueltigBis = 0 einen offenen (aktuell gültigen) Preis kennzeichnet).
    /// </summary>
    public class PreiseRepository
    {
        /// <summary>
        /// Liefert alle Positionen (inkl. Zusatz-Varianten) mit dem am DatumAb
        /// gültigen Preis aus GlobalerPreis.
        /// </summary>
        public List<PreisZeile> GetPreise(int datumAb)
        {
            var zeilen = new List<PreisZeile>();
            using var connection = Database.CreateConnection();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT p.Id, p.Name, p.Preiskategorie,
                           z.Id AS ZusatzId, z.Bezeichnung
                    FROM GlobalePreisliste p
                    LEFT JOIN GlobalePreislisteZusatz z ON z.SpeisekarteId = p.Id
                    ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var kategorie = reader.IsDBNull(2)
                        ? Preiskategorie.Speisen
                        : (Preiskategorie)reader.GetInt64(2);
                    zeilen.Add(new PreisZeile
                    {
                        PreislisteId = reader.GetInt64(0),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        KategorieText = kategorie == Preiskategorie.Getraenke ? "Getränke" : "Speisen",
                        ZusatzId = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                        ZusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                    });
                }
            }

            var preise = GetGueltigePreise(connection, datumAb);
            foreach (var zeile in zeilen)
            {
                if (preise.TryGetValue((zeile.PreislisteId, zeile.ZusatzId), out var preis))
                {
                    zeile.Preis = preis;
                }
            }

            return zeilen;
        }

        /// <summary>
        /// Liefert das End-Datum (GueltigBis, yyyyMMdd) der am <paramref name="datumAb"/> gültigen Preise.
        /// Gibt null zurück, wenn mindestens ein gültiger Preis kein End-Datum hat
        /// (GueltigBis = 0, also unbegrenzt gültig) oder keine Preise vorhanden sind.
        /// </summary>
        public int? GetEndDatum(int datumAb)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GueltigBis
                FROM GlobalerPreis
                WHERE GueltigAb <= $datumAb AND (GueltigBis = 0 OR GueltigBis >= $datumAb);";
            command.Parameters.AddWithValue("$datumAb", datumAb);

            int? maxEnde = null;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var ende = reader.IsDBNull(0) ? 0 : (int)reader.GetInt64(0);
                if (ende == 0)
                {
                    return null;
                }
                if (maxEnde == null || ende > maxEnde)
                {
                    maxEnde = ende;
                }
            }
            return maxEnde;
        }

        public static Dictionary<(long, long), decimal> GetGueltigePreise(SqliteConnection connection, int datumAb)
        {
            var result = new Dictionary<(long, long), decimal>();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, ZusatzId, Preis
                FROM GlobalerPreis
                WHERE GueltigAb <= $datumAb AND (GueltigBis = 0 OR GueltigBis >= $datumAb);";
            command.Parameters.AddWithValue("$datumAb", datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = (reader.GetInt64(0), reader.GetInt64(1));
                result[key] = reader.IsDBNull(2) ? 0m : (decimal)reader.GetDouble(2);
            }
            return result;
        }

        /// <summary>
        /// Speichert die Preise.
        /// Ist <paramref name="abDatum"/> gleich dem angezeigten Tag
        /// (GueltigBis = 0) einfach aktualisiert.
        /// Ist <paramref name="abDatum"/> größer, werden die bisherigen offenen Preise
        /// mit GueltigBis = abDatum - 1 Tag abgeschlossen und neue offene Preise
        /// (GueltigAb = abDatum, GueltigBis = 0) angelegt.
        /// </summary>
        public void Speichern(int abDatum, IEnumerable<PreisZeile> zeilen)
        {
            using var connection = Database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            
            int vortag = Termin.ToYyyyMmDd(Termin.FromYyyyMmDd(abDatum).AddDays(-1));

            foreach (var zeile in zeilen)
            {
                var aenderung = ExistiertPreisAmStichtag(connection, zeile, abDatum);
                if (aenderung)
                {
                    // Existiert zum DatumAb kein (Start-)Preis, wird ein neuer angelegt;
                    // andernfalls der aktuell offene Preis aktualisiert.
                    UpdateOffenerPreis(connection, zeile, abDatum);
                }
                else
                {
                    SchliesseOffenenPreis(connection, zeile, vortag);
                    InsertNeuerPreis(connection, zeile, abDatum);
                }
            }

            transaction.Commit();
        }

        private static bool ExistiertPreisAmStichtag(SqliteConnection connection, PreisZeile zeile, int datumAb)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 1
                FROM GlobalerPreis
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId = $zusatzId
                  AND GueltigAb <= $datumAb
                  AND (GueltigBis = 0 OR GueltigBis >= $datumAb)
                LIMIT 1;";
            command.Parameters.AddWithValue("$preislisteId", zeile.PreislisteId);
            command.Parameters.AddWithValue("$zusatzId", zeile.ZusatzId);
            command.Parameters.AddWithValue("$datumAb", datumAb);
            return command.ExecuteScalar() != null;
        }

        private static bool UpdateOffenerPreis(SqliteConnection connection, PreisZeile zeile, int gueltigAb)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalerPreis
                SET Preis = $preis
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId = $zusatzId
                  AND GueltigAb = $gueltigAb
                  AND GueltigBis = 0;";
            command.Parameters.AddWithValue("$preis", (double)zeile.Preis);
            command.Parameters.AddWithValue("$preislisteId", zeile.PreislisteId);
            command.Parameters.AddWithValue("$zusatzId", zeile.ZusatzId);
            command.Parameters.AddWithValue("$gueltigAb", gueltigAb);
            return command.ExecuteNonQuery() > 0;
        }

        private static void SchliesseOffenenPreis(SqliteConnection connection, PreisZeile zeile, int vortag)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalerPreis
                SET GueltigBis = $vortag
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId = $zusatzId
                  AND GueltigBis = 0;";
            command.Parameters.AddWithValue("$vortag", vortag);
            command.Parameters.AddWithValue("$preislisteId", zeile.PreislisteId);
            command.Parameters.AddWithValue("$zusatzId", zeile.ZusatzId);
            command.ExecuteNonQuery();
        }

        public static void InsertNeuerPreis(SqliteConnection connection, PreisZeile zeile, int abDatum)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalerPreis
                    (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                VALUES ($preislisteId, $zusatzId, $abDatum, 0, $preis);";
            command.Parameters.AddWithValue("$preislisteId", zeile.PreislisteId);
            command.Parameters.AddWithValue("$zusatzId", zeile.ZusatzId);
            command.Parameters.AddWithValue("$abDatum", abDatum);
            command.Parameters.AddWithValue("$preis", (double)zeile.Preis);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Speichert einen GlobalerPreis-Datensatz mit explizitem Gültigkeitszeitraum
        /// (GueltigAb/GueltigBis, GueltigBis = 0 kennzeichnet einen offenen Preis).
        /// Existiert bereits ein Datensatz mit dem bisherigen Schlüssel
        /// (GlobalePreislisteId, ZusatzId, <paramref name="altGueltigAb"/>), wird dieser
        /// aktualisiert – auch der Schlüsselteil GueltigAb. Andernfalls (z. B. bei einem
        /// neuen Datensatz der GlobalePreisliste) wird ein neuer Datensatz angelegt, damit
        /// in GlobalerPreis immer ein Preis angezeigt werden kann.
        /// </summary>
        public void SpeichereGueltigkeit(long preislisteId, long zusatzId,
            int altGueltigAb, int neuGueltigAb, int gueltigBis, decimal preis)
        {
            using var connection = Database.CreateConnection();

            bool vorhanden = false;
            if (altGueltigAb > 0)
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT 1
                    FROM GlobalerPreis
                    WHERE GlobalePreislisteId = $preislisteId
                      AND ZusatzId = $zusatzId
                      AND GueltigAb = $altGueltigAb
                    LIMIT 1;";
                checkCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                checkCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                checkCmd.Parameters.AddWithValue("$altGueltigAb", altGueltigAb);
                vorhanden = checkCmd.ExecuteScalar() != null;
            }

            if (vorhanden)
            {
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE GlobalerPreis
                    SET GueltigAb = $neuGueltigAb,
                        GueltigBis = $gueltigBis,
                        Preis = $preis
                    WHERE GlobalePreislisteId = $preislisteId
                      AND ZusatzId = $zusatzId
                      AND GueltigAb = $altGueltigAb;";
                updateCmd.Parameters.AddWithValue("$neuGueltigAb", neuGueltigAb);
                updateCmd.Parameters.AddWithValue("$gueltigBis", gueltigBis);
                updateCmd.Parameters.AddWithValue("$preis", (double)preis);
                updateCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                updateCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                updateCmd.Parameters.AddWithValue("$altGueltigAb", altGueltigAb);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO GlobalerPreis
                        (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                    VALUES ($preislisteId, $zusatzId, $neuGueltigAb, $gueltigBis, $preis);";
                insertCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                insertCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                insertCmd.Parameters.AddWithValue("$neuGueltigAb", neuGueltigAb);
                insertCmd.Parameters.AddWithValue("$gueltigBis", gueltigBis);
                insertCmd.Parameters.AddWithValue("$preis", (double)preis);
                insertCmd.ExecuteNonQuery();
            }
        }

        // --- CRUD-Zugriff auf die Tabelle GlobalerPreis ---

        /// <summary>Liefert alle Datensätze der Tabelle GlobalerPreis.</summary>
        public static List<GlobalerPreis> GetAllAtDate(int startDate)
        {
            var result = new List<GlobalerPreis>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis
                FROM GlobalerPreis
                WHERE GueltigAb == $startDate
                ORDER BY GlobalePreislisteId, ZusatzId, GueltigAb;";
            command.Parameters.AddWithValue("$startDate", startDate);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapGlobalerPreis(reader));
            }
            return result;
        }

        /// <summary>
        /// Liefert je Hauptposition (ohne Zusatz, ZusatzId = 0) den am <paramref name="datumAb"/>
        /// gültigen Preis inklusive dessen Startdatum (GueltigAb).
        /// Schlüssel des Ergebnisses ist GlobalePreislisteId.
        /// </summary>
        public static Dictionary<long, (decimal Preis, int GueltigAb, int GueltigBis)> GetHauptPreiseMitStart(int datumAb)
        {
            var result = new Dictionary<long, (decimal, int, int)>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, GueltigAb, GueltigBis, Preis
                FROM GlobalerPreis
                WHERE ZusatzId = 0
                  AND GueltigAb <= $datumAb
                  AND (GueltigBis = 0 OR GueltigBis >= $datumAb);";
            command.Parameters.AddWithValue("$datumAb", datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var gueltigAb = (int)reader.GetInt64(1);
                var gueltigBis = (int)reader.GetInt64(2);
                var preis = reader.IsDBNull(3) ? 0m : (decimal)reader.GetDouble(3);
                result[id] = (preis, gueltigAb, gueltigBis);
            }
            return result;
        }

        /// <summary>
        /// Liefert je Zusatz-Position (ZusatzId) einer Hauptposition den am
        /// <paramref name="datumAb"/> gültigen Preis inklusive dessen Startdatum (GueltigAb).
        /// Schlüssel des Ergebnisses ist ZusatzId.
        /// </summary>
        public static Dictionary<long, (decimal Preis, int GueltigAb)> GetZusatzPreiseMitStart(long preislisteId, int datumAb)
        {
            var result = new Dictionary<long, (decimal, int)>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ZusatzId, GueltigAb, Preis
                FROM GlobalerPreis
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId <> 0
                  AND GueltigAb <= $datumAb
                  AND (GueltigBis = 0 OR GueltigBis >= $datumAb);";
            command.Parameters.AddWithValue("$preislisteId", preislisteId);
            command.Parameters.AddWithValue("$datumAb", datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var zusatzId = reader.GetInt64(0);
                var gueltigAb = (int)reader.GetInt64(1);
                var preis = reader.IsDBNull(2) ? 0m : (decimal)reader.GetDouble(2);
                result[zusatzId] = (preis, gueltigAb);
            }
            return result;
        }

        /// <summary>
        /// Stellt sicher, dass für jede Preis-Position (GlobalePreislisteId, ZusatzId),
        /// die vor bzw. zum <paramref name="datumAb"/> jemals einen Preis hatte, auch am
        /// <paramref name="datumAb"/> ein gültiger Preis existiert. Fehlt ein gültiger
        /// Preis (weil die letzte Periode bereits abgeschlossen ist), wird der zuletzt
        /// bekannte Preis der Vorperiode als neuer offener Preis
        /// (GueltigAb = datumAb, GueltigBis = 0) übernommen. Der bisher offene Preis
        /// der Vorperiode (GueltigBis = 0) wird dabei mit GueltigBis = datumAb - 1 Tag
        /// abgeschlossen.
        /// Liefert die Anzahl der neu angelegten Preis-Einträge.
        /// </summary>
        public static int UebernehmeFehlendeVorperiodenPreise(int datumAb)
        {
            using var connection = Database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            int vortag = Termin.ToYyyyMmDd(Termin.FromYyyyMmDd(datumAb).AddDays(-1));

            // Alle Positionen, die zum/vor dem Stichtag jemals einen Preis besitzen.
            var positionen = new List<(long PreislisteId, long ZusatzId)>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT DISTINCT GlobalePreislisteId, ZusatzId
                    FROM GlobalerPreis
                    WHERE GueltigAb <= $datumAb;";
                command.Parameters.AddWithValue("$datumAb", datumAb);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    positionen.Add((reader.GetInt64(0), reader.GetInt64(1)));
                }
            }

            int angelegt = 0;
            foreach (var (preislisteId, zusatzId) in positionen)
            {
                // Ist bereits ein gültiger Preis am Stichtag vorhanden?
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = @"
                        SELECT 1
                        FROM GlobalerPreis
                        WHERE GlobalePreislisteId = $preislisteId
                          AND ZusatzId = $zusatzId
                          AND GueltigAb <= $datumAb
                          AND (GueltigBis = 0 OR GueltigBis >= $datumAb)
                        LIMIT 1;";
                    checkCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                    checkCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                    checkCmd.Parameters.AddWithValue("$datumAb", datumAb);
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        // Offene Preise der Vorperiode zum Vortag abschließen (GueltigBis = 0 -> Vortag).
                        using (var closeCmd = connection.CreateCommand())
                        {
                            closeCmd.CommandText = @"UPDATE GlobalerPreis
                                                      SET GueltigBis = $vortag
                                                      WHERE GlobalePreislisteId = $preislisteId
                                                      AND ZusatzId = $zusatzId
                                                      AND GueltigAb < $datumAb
                                                      AND GueltigBis = 0";
                            closeCmd.Parameters.AddWithValue("$vortag", vortag);
                            closeCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                            closeCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                            closeCmd.Parameters.AddWithValue("$datumAb", datumAb);
                            closeCmd.ExecuteNonQuery();
                        }
                        continue;
                    }
                }

                // Preis der jüngsten Vorperiode ermitteln.
                decimal? vorperiodenPreis = null;
                using (var prevCmd = connection.CreateCommand())
                {
                    prevCmd.CommandText = @"
                        SELECT Preis
                        FROM GlobalerPreis
                        WHERE GlobalePreislisteId = $preislisteId
                          AND ZusatzId = $zusatzId
                          AND GueltigAb <= $datumAb
                        ORDER BY GueltigAb DESC
                        LIMIT 1;";
                    prevCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                    prevCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                    prevCmd.Parameters.AddWithValue("$datumAb", datumAb);
                    var result = prevCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        vorperiodenPreis = (decimal)(double)result;
                    }
                }

                if (vorperiodenPreis == null)
                {
                    continue;
                }

                // Neuen offenen Preis mit dem Preis der Vorperiode anlegen.
                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO GlobalerPreis
                            (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                        VALUES ($preislisteId, $zusatzId, $datumAb, 0, $preis);";
                    insertCmd.Parameters.AddWithValue("$preislisteId", preislisteId);
                    insertCmd.Parameters.AddWithValue("$zusatzId", zusatzId);
                    insertCmd.Parameters.AddWithValue("$datumAb", datumAb);
                    insertCmd.Parameters.AddWithValue("$preis", (double)vorperiodenPreis.Value);
                    insertCmd.ExecuteNonQuery();
                    angelegt++;
                }
            }

            transaction.Commit();
            return angelegt;
        }

        public static List<int> GetAllDates()
        {
            var result = new List<int>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GueltigAb
                FROM GlobalerPreis
                GROUP BY GueltigAb
                ORDER BY GueltigAb;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetInt32(0));
            }
            return result;
        }
        /// <summary>Fügt einen neuen Datensatz in die Tabelle GlobalerPreis ein.</summary>
        public static void Insert(GlobalerPreis preis)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalerPreis
                    (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                VALUES ($preislisteId, $zusatzId, $gueltigAb, $gueltigBis, $preis);";
            AddParameters(command, preis);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Aktualisiert einen bestehenden Datensatz der Tabelle GlobalerPreis.
        /// Der Datensatz wird über den Schlüssel (GlobalePreislisteId, ZusatzId, GueltigAb) identifiziert.
        /// </summary>
        public void Update(GlobalerPreis preis)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalerPreis
                SET GueltigBis = $gueltigBis,
                    Preis = $preis
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId = $zusatzId
                  AND GueltigAb = $gueltigAb;";
            AddParameters(command, preis);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Löscht einen Datensatz der Tabelle GlobalerPreis anhand des Schlüssels
        /// (GlobalePreislisteId, ZusatzId, GueltigAb).
        /// </summary>
        public void Delete(long globalePreislisteId, long zusatzId, int gueltigAb)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM GlobalerPreis
                WHERE GlobalePreislisteId = $preislisteId
                  AND ZusatzId = $zusatzId
                  AND GueltigAb = $gueltigAb;";
            command.Parameters.AddWithValue("$preislisteId", globalePreislisteId);
            command.Parameters.AddWithValue("$zusatzId", zusatzId);
            command.Parameters.AddWithValue("$gueltigAb", gueltigAb);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(SqliteCommand command, GlobalerPreis preis)
        {
            command.Parameters.AddWithValue("$preislisteId", preis.GlobalePreislisteId);
            command.Parameters.AddWithValue("$zusatzId", preis.ZusatzId);
            command.Parameters.AddWithValue("$gueltigAb", preis.GueltigAb);
            command.Parameters.AddWithValue("$gueltigBis", preis.GueltigBis);
            command.Parameters.AddWithValue("$preis", (double)preis.Preis);
        }

        private static GlobalerPreis MapGlobalerPreis(SqliteDataReader reader) => new()
        {
            GlobalePreislisteId = reader.GetInt64(0),
            ZusatzId = reader.GetInt64(1),
            GueltigAb = (int)reader.GetInt64(2),
            GueltigBis = (int)reader.GetInt64(3),
            Preis = reader.IsDBNull(4) ? 0m : (decimal)reader.GetDouble(4)
        };
    }
}

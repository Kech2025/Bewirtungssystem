using Npgsql;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für die globale Preisliste (Tabelle GlobalePreisliste),
    /// deren Zusatz-Positionen (Tabelle GlobalePreislisteZusatz) sowie die
    /// zeitraumbezogenen Preise (Tabelle GlobalerPreis).
    /// </summary>
    public class GlobalePreislisteRepository
    {
        private readonly Database _database;

        public GlobalePreislisteRepository(Database database)
        {
            _database = database;
        }

        // Datums-Grenze: int yyyyMMdd (0 = offen/kein Enddatum) <-> PostgreSQL date (NULL = offen).
        private static object GueltigBisParam(int yyyyMmDd) =>
            yyyyMmDd == 0 ? DBNull.Value : Termin.FromYyyyMmDd(yyyyMmDd);

        private static int ReadGueltigBis(NpgsqlDataReader reader, int ordinal) =>
            reader.IsDBNull(ordinal) ? 0 : Termin.ToYyyyMmDd(reader.GetDateTime(ordinal));

        // ----- Positionen -----

        public List<GlobalePreisliste> GetAll(int datumAb)
        {
            var result = new List<GlobalePreisliste>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Preiskategorie, BildPfad, Druck, MAX(GueltigAb) AS GueltigAb, Zusatztext
                FROM GlobalePreisliste
                LEFT JOIN GlobalerPreis ON GlobalePreislisteId = Id
                AND GueltigAb <= @DatumAb
                GROUP BY Id, Name, Preiskategorie, BildPfad, Druck, Zusatztext
                ORDER BY Preiskategorie, Name;";
            command.Parameters.AddWithValue("DatumAb", Termin.FromYyyyMmDd(datumAb));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Map(reader));
            }

            // Aktuell gültige Preise inkl. Start-/Enddatum ergänzen.
            var preise = GetHauptPreiseMitStart(datumAb);
            foreach (var eintrag in result)
            {
                if (preise.TryGetValue(eintrag.Id, out var info))
                {
                    eintrag.Preis = info.Preis;
                    eintrag.StartDatum = info.GueltigAb;
                    eintrag.GueltigBis = info.GueltigBis;
                }
                else
                {
                    eintrag.Preis = null;
                    eintrag.StartDatum = 0;
                    eintrag.GueltigBis = 0;
                }
            }
            return result;
        }

        public long Insert(GlobalePreisliste eintrag)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalePreisliste (Name, Preiskategorie, BildPfad, Druck, Zusatztext)
                VALUES (@name, @preiskategorie, @bildPfad, @druck, @zusatztext)
                RETURNING Id;";
            AddParameters(command, eintrag);
            var id = Convert.ToInt64(command.ExecuteScalar()!);
            eintrag.Id = id;
            return id;
        }

        public void Update(GlobalePreisliste eintrag)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalePreisliste SET
                    Name = @name,
                    Preiskategorie = @preiskategorie,
                    BildPfad = @bildPfad,
                    Druck = @druck,
                    Zusatztext = @zusatztext
                WHERE Id = @id;";
            AddParameters(command, eintrag);
            command.Parameters.AddWithValue("id", eintrag.Id);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var deleteZusatz = connection.CreateCommand())
            {
                deleteZusatz.CommandText = "DELETE FROM GlobalePreislisteZusatz WHERE SpeisekarteId = @id;";
                deleteZusatz.Parameters.AddWithValue("id", id);
                deleteZusatz.ExecuteNonQuery();
            }

            using (var deleteEintrag = connection.CreateCommand())
            {
                deleteEintrag.CommandText = "DELETE FROM GlobalePreisliste WHERE Id = @id;";
                deleteEintrag.Parameters.AddWithValue("id", id);
                deleteEintrag.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        // ----- Zusatzpositionen -----

        public List<GlobalePreislisteZusatz> GetZusatz(long speisekarteId, int datumAb)
        {
            var result = new List<GlobalePreislisteZusatz>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, SpeisekarteId, Bezeichnung, BildPfad, Druck
                FROM GlobalePreislisteZusatz
                WHERE SpeisekarteId = @speisekarteId
                ORDER BY Bezeichnung;";
            command.Parameters.AddWithValue("speisekarteId", speisekarteId);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(MapZusatz(reader));
                }
            }

            var preise = GetZusatzPreiseMitStart(speisekarteId, datumAb);
            foreach (var zusatz in result)
            {
                if (preise.TryGetValue(zusatz.Id, out var info))
                {
                    zusatz.Preis = info.Preis;
                    zusatz.StartDatum = info.GueltigAb;
                }
                else
                {
                    zusatz.Preis = null;
                    zusatz.StartDatum = 0;
                }
            }
            return result;
        }

        public long InsertZusatz(GlobalePreislisteZusatz zusatz)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalePreislisteZusatz (SpeisekarteId, Bezeichnung, BildPfad, Druck)
                VALUES (@speisekarteId, @bezeichnung, @bildPfad, @druck)
                RETURNING Id;";
            command.Parameters.AddWithValue("speisekarteId", zusatz.SpeisekarteId);
            command.Parameters.AddWithValue("bezeichnung", (object?)zusatz.Bezeichnung ?? DBNull.Value);
            command.Parameters.AddWithValue("bildPfad", (object?)zusatz.BildPfad ?? DBNull.Value);
            command.Parameters.AddWithValue("druck", zusatz.Druck);
            var id = Convert.ToInt64(command.ExecuteScalar()!);
            zusatz.Id = id;
            return id;
        }

        public void UpdateZusatz(GlobalePreislisteZusatz zusatz)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalePreislisteZusatz SET
                    Bezeichnung = $bezeichnung,
                    BildPfad = @bildPfad,
                    Druck = @druck
                WHERE Id = @id;";
            command.Parameters.AddWithValue("bezeichnung", (object?)zusatz.Bezeichnung ?? DBNull.Value);
            command.Parameters.AddWithValue("bildPfad", (object?)zusatz.BildPfad ?? DBNull.Value);
            command.Parameters.AddWithValue("druck", zusatz.Druck);
            command.Parameters.AddWithValue("id", zusatz.Id);
            command.ExecuteNonQuery();
        }

        public void DeleteZusatz(long id)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM GlobalePreislisteZusatz WHERE Id = @id;";
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        // ----- Preise / Datumsverwaltung -----

        /// <summary>Alle Datumswerte (yyyyMMdd), zu denen Preise hinterlegt sind.</summary>
        public List<int> GetAllDates()
        {
            var result = new List<int>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GueltigAb
                FROM GlobalerPreis
                GROUP BY GueltigAb
                ORDER BY GueltigAb;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Termin.ToYyyyMmDd(reader.GetDateTime(0)));
            }
            return result;
        }

        public Dictionary<long, (decimal Preis, int GueltigAb, int GueltigBis)> GetHauptPreiseMitStart(int datumAb)
        {
            var result = new Dictionary<long, (decimal, int, int)>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, GueltigAb, GueltigBis, Preis
                FROM GlobalerPreis
                WHERE ZusatzId = 0
                  AND GueltigAb <= @datumAb
                  AND (GueltigBis IS NULL OR GueltigBis >= @datumAb);";
            command.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = (long)reader.GetInt32(0);
                var gueltigAb = Termin.ToYyyyMmDd(reader.GetDateTime(1));
                var gueltigBis = ReadGueltigBis(reader, 2);
                var preis = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                result[id] = (preis, gueltigAb, gueltigBis);
            }
            return result;
        }

        public Dictionary<long, (decimal Preis, int GueltigAb)> GetZusatzPreiseMitStart(long preislisteId, int datumAb)
        {
            var result = new Dictionary<long, (decimal, int)>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ZusatzId, GueltigAb, Preis
                FROM GlobalerPreis
                WHERE GlobalePreislisteId = @preislisteId
                  AND ZusatzId <> 0
                  AND GueltigAb <= @datumAb
                  AND (GueltigBis IS NULL OR GueltigBis >= @datumAb);";
            command.Parameters.AddWithValue("preislisteId", preislisteId);
            command.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var zusatzId = (long)reader.GetInt32(0);
                var gueltigAb = Termin.ToYyyyMmDd(reader.GetDateTime(1));
                var preis = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                result[zusatzId] = (preis, gueltigAb);
            }
            return result;
        }

        /// <summary>
        /// Speichert den Preis mit explizitem Gültigkeitszeitraum (GueltigAb/GueltigBis).
        /// Bei einem neuen Datensatz (altGueltigAb = 0) wird in GlobalerPreis angelegt.
        /// </summary>
        public void SpeichereGueltigkeit(long preislisteId, long zusatzId,
            int altGueltigAb, int neuGueltigAb, int gueltigBis, decimal preis)
        {
            using var connection = _database.CreateConnection();

            bool vorhanden = false;
            if (altGueltigAb > 0)
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT 1
                    FROM GlobalerPreis
                    WHERE GlobalePreislisteId = @preislisteId
                      AND ZusatzId = @zusatzId
                      AND GueltigAb = @altGueltigAb
                    LIMIT 1;";
                checkCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                checkCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                checkCmd.Parameters.AddWithValue("altGueltigAb", Termin.FromYyyyMmDd(altGueltigAb));
                vorhanden = checkCmd.ExecuteScalar() != null;
            }

            if (vorhanden)
            {
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE GlobalerPreis
                    SET GueltigAb = @neuGueltigAb,
                        GueltigBis = @gueltigBis,
                        Preis = @preis
                    WHERE GlobalePreislisteId = @preislisteId
                      AND ZusatzId = @zusatzId
                      AND GueltigAb = @altGueltigAb;";
                updateCmd.Parameters.AddWithValue("neuGueltigAb", Termin.FromYyyyMmDd(neuGueltigAb));
                updateCmd.Parameters.AddWithValue("gueltigBis", GueltigBisParam(gueltigBis));
                updateCmd.Parameters.AddWithValue("preis", preis);
                updateCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                updateCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                updateCmd.Parameters.AddWithValue("altGueltigAb", Termin.FromYyyyMmDd(altGueltigAb));
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO GlobalerPreis
                        (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                    VALUES (@preislisteId, @zusatzId, @neuGueltigAb, @gueltigBis, @preis);";
                insertCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                insertCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                insertCmd.Parameters.AddWithValue("neuGueltigAb", Termin.FromYyyyMmDd(neuGueltigAb));
                insertCmd.Parameters.AddWithValue("gueltigBis", GueltigBisParam(gueltigBis));
                insertCmd.Parameters.AddWithValue("preis", preis);
                insertCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Stellt sicher, dass für jede Position, die vor/zum Stichtag jemals einen
        /// Preis hatte, auch am Stichtag ein gültiger Preis existiert (aus Vorperiode).
        /// </summary>
        public int UebernehmeFehlendeVorperiodenPreise(int datumAb)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            int vortag = Termin.ToYyyyMmDd(Termin.FromYyyyMmDd(datumAb).AddDays(-1));

            var positionen = new List<(long PreislisteId, long ZusatzId)>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT DISTINCT GlobalePreislisteId, ZusatzId
                    FROM GlobalerPreis
                    WHERE GueltigAb <= @datumAb;";
                command.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    positionen.Add(((long)reader.GetInt32(0), (long)reader.GetInt32(1)));
                }
            }

            int angelegt = 0;
            foreach (var (preislisteId, zusatzId) in positionen)
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = @"
                        SELECT 1
                        FROM GlobalerPreis
                        WHERE GlobalePreislisteId = @preislisteId
                          AND ZusatzId = @zusatzId
                          AND GueltigAb <= @datumAb
                          AND (GueltigBis IS NULL OR GueltigBis >= @datumAb)
                        LIMIT 1;";
                    checkCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    checkCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    checkCmd.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        using var closeCmd = connection.CreateCommand();
                        closeCmd.CommandText = @"UPDATE GlobalerPreis
                                                  SET GueltigBis = @vortag
                                                  WHERE GlobalePreislisteId = @preislisteId
                                                  AND ZusatzId = @zusatzId
                                                  AND GueltigAb < @datumAb
                                                  AND GueltigBis IS NULL";
                        closeCmd.Parameters.AddWithValue("vortag", Termin.FromYyyyMmDd(vortag));
                        closeCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                        closeCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                        closeCmd.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
                        closeCmd.ExecuteNonQuery();
                        continue;
                    }
                }

                decimal? vorperiodenPreis = null;
                using (var prevCmd = connection.CreateCommand())
                {
                    prevCmd.CommandText = @"
                        SELECT Preis
                        FROM GlobalerPreis
                        WHERE GlobalePreislisteId = @preislisteId
                          AND ZusatzId = @zusatzId
                          AND GueltigAb <= @datumAb
                        ORDER BY GueltigAb DESC
                        LIMIT 1;";
                    prevCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    prevCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    prevCmd.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
                    var result = prevCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        vorperiodenPreis = Convert.ToDecimal(result);
                    }
                }

                if (vorperiodenPreis == null)
                {
                    continue;
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO GlobalerPreis
                            (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                        VALUES (@preislisteId, @zusatzId, @datumAb, NULL, @preis);";
                    insertCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    insertCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    insertCmd.Parameters.AddWithValue("datumAb", Termin.FromYyyyMmDd(datumAb));
                    insertCmd.Parameters.AddWithValue("preis", vorperiodenPreis.Value);
                    insertCmd.ExecuteNonQuery();
                    angelegt++;
                }
            }

            transaction.Commit();
            return angelegt;
        }

        /// <summary>
        /// Übernimmt alle am Quelldatum gültigen Preise als neue offene Preise
        /// für ein neues Datum (sofern dort noch nicht vorhanden).
        /// </summary>
        public void AddDatum(int quelleDatum, int neuDatum)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var quellPreise = new List<GlobalerPreis>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis
                    FROM GlobalerPreis
                    WHERE GueltigAb <= @datum AND (GueltigBis IS NULL OR GueltigBis >= @datum);";
                command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(quelleDatum));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    quellPreise.Add(new GlobalerPreis
                    {
                        GlobalePreislisteId = reader.GetInt32(0),
                        ZusatzId = reader.GetInt32(1),
                        GueltigAb = Termin.ToYyyyMmDd(reader.GetDateTime(2)),
                        GueltigBis = ReadGueltigBis(reader, 3),
                        Preis = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4)
                    });
                }
            }

            foreach (var zeile in quellPreise)
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = @"
                        SELECT 1 FROM GlobalerPreis
                        WHERE GlobalePreislisteId = @preislisteId
                          AND ZusatzId = @zusatzId
                          AND GueltigAb = @neuDatum
                        LIMIT 1;";
                    checkCmd.Parameters.AddWithValue("preislisteId", zeile.GlobalePreislisteId);
                    checkCmd.Parameters.AddWithValue("zusatzId", zeile.ZusatzId);
                    checkCmd.Parameters.AddWithValue("neuDatum", Termin.FromYyyyMmDd(neuDatum));
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        continue;
                    }
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO GlobalerPreis
                            (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis)
                        VALUES (@preislisteId, @zusatzId, @neuDatum, NULL, @preis);";
                    insertCmd.Parameters.AddWithValue("preislisteId", zeile.GlobalePreislisteId);
                    insertCmd.Parameters.AddWithValue("zusatzId", zeile.ZusatzId);
                    insertCmd.Parameters.AddWithValue("neuDatum", Termin.FromYyyyMmDd(neuDatum));
                    insertCmd.Parameters.AddWithValue("preis", zeile.Preis);
                    insertCmd.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        private static void AddParameters(NpgsqlCommand command, GlobalePreisliste eintrag)
        {
            command.Parameters.AddWithValue("name", (object?)eintrag.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("preiskategorie", (int)eintrag.Preiskategorie);
            command.Parameters.AddWithValue("bildPfad", (object?)eintrag.BildPfad ?? DBNull.Value);
            command.Parameters.AddWithValue("druck", eintrag.Druck);
            command.Parameters.AddWithValue("zusatztext", (object?)eintrag.Zusatztext ?? DBNull.Value);
        }

        private static GlobalePreisliste Map(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Preiskategorie = reader.IsDBNull(2) ? Preiskategorie.Speisen : (Preiskategorie)reader.GetInt32(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetBoolean(4),
            StartDatum = reader.IsDBNull(5) ? 0 : Termin.ToYyyyMmDd(reader.GetDateTime(5)),
            Zusatztext = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
        };

        private static GlobalePreislisteZusatz MapZusatz(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            SpeisekarteId = reader.GetInt32(1),
            Bezeichnung = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetBoolean(4)
        };
    }
}

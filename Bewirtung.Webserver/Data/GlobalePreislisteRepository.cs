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

        public List<GlobalePreisliste> GetAll(int datumAbInt)
        {
            DateTime datumAb = Termin.FromYyyyMmDd(datumAbInt);
            var result = new List<GlobalePreisliste>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Preiskategorie, BildPfad, Druck, MAX(GueltigAb) AS GueltigAb, Zusatztext, Status
                FROM GlobalePreisliste
                LEFT JOIN GlobalerPreis ON GlobalePreislisteId = Id
                AND GueltigAb <= @DatumAb
                GROUP BY Id, Name, Preiskategorie, BildPfad, Druck, Zusatztext, Status
                ORDER BY Preiskategorie, Name;";
            command.Parameters.AddWithValue("DatumAb", datumAb);
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
                    eintrag.Verrechnungspreis = info.Verrechnungspreis;
                    eintrag.Einkaufspreis = info.Einkaufspreis;
                    eintrag.StartDatum = info.GueltigAb;
                    eintrag.GueltigBis = info.GueltigBis;
                }
                else
                {
                    eintrag.Preis = null;
                    eintrag.Verrechnungspreis = null;
                    eintrag.Einkaufspreis = null;
                    eintrag.StartDatum = DateTime.MinValue;
                    eintrag.GueltigBis = DateTime.MinValue;
                }
            }
            return result;
        }

        public long Insert(GlobalePreisliste eintrag)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalePreisliste (Name, Preiskategorie, BildPfad, Druck, Zusatztext, Status)
                VALUES (@name, @preiskategorie, @bildPfad, @druck, @zusatztext, @status)
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
                    Zusatztext = @zusatztext,
                    Status = @status
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

        public List<GlobalePreislisteZusatz> GetZusatz(long speisekarteId, int datumAbInt)
        {
            DateTime datumAb = Termin.FromYyyyMmDd(datumAbInt);
            var result = new List<GlobalePreislisteZusatz>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, SpeisekarteId, Bezeichnung, BildPfad, Druck, Status
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
                    zusatz.Verrechnungspreis = info.Verrechnungspreis;
                    zusatz.Einkaufspreis = info.Einkaufspreis;
                    zusatz.StartDatum = Termin.ToYyyyMmDd(info.GueltigAb);
                }
                else
                {
                    zusatz.Preis = null;
                    zusatz.Verrechnungspreis = null;
                    zusatz.Einkaufspreis = null;
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
                INSERT INTO GlobalePreislisteZusatz (SpeisekarteId, Bezeichnung, BildPfad, Druck, Status)
                VALUES (@speisekarteId, @bezeichnung, @bildPfad, @druck, @status)
                RETURNING Id;";
            command.Parameters.AddWithValue("speisekarteId", zusatz.SpeisekarteId);
            command.Parameters.AddWithValue("bezeichnung", (object?)zusatz.Bezeichnung ?? DBNull.Value);
            command.Parameters.AddWithValue("bildPfad", (object?)zusatz.BildPfad ?? DBNull.Value);
            command.Parameters.AddWithValue("druck", zusatz.Druck);
            command.Parameters.AddWithValue("status", zusatz.Status);
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
                    Bezeichnung = @bezeichnung,
                    BildPfad = @bildPfad,
                    Druck = @druck,
                    Status = @status
                WHERE Id = @id;";
            command.Parameters.AddWithValue("bezeichnung", (object?)zusatz.Bezeichnung ?? DBNull.Value);
            command.Parameters.AddWithValue("bildPfad", (object?)zusatz.BildPfad ?? DBNull.Value);
            command.Parameters.AddWithValue("druck", zusatz.Druck);
            command.Parameters.AddWithValue("status", zusatz.Status);
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

        public Dictionary<long, (decimal Preis, decimal Verrechnungspreis, decimal Einkaufspreis, DateTime GueltigAb, DateTime GueltigBis)> GetHauptPreiseMitStart(DateTime datumAb)
        {
            var result = new Dictionary<long, (decimal, decimal, decimal, DateTime, DateTime)>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, GueltigAb, GueltigBis, Preis, Verrechnungspreis, Einkaufspreis
                FROM GlobalerPreis
                WHERE ZusatzId = 0
                  AND GueltigAb <= @datumAb
                  AND (GueltigBis IS NULL OR NOT isfinite(GueltigBis) OR GueltigBis >= @datumAb);";
            command.Parameters.AddWithValue("datumAb",datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = (long)reader.GetInt32(0);
                DateTime gueltigAb = reader.GetDateTime(1);
                DateTime gueltigBis = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);
                var preis = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                var verrechnungspreis = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                var einkaufspreis = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5);
                result[id] = (preis, verrechnungspreis, einkaufspreis, gueltigAb, gueltigBis);
            }
            return result;
        }

        public Dictionary<long, (decimal Preis, decimal Verrechnungspreis, decimal Einkaufspreis, DateTime GueltigAb)> GetZusatzPreiseMitStart(long preislisteId, DateTime datumAb)
        {
            var result = new Dictionary<long, (decimal, decimal, decimal, DateTime)>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ZusatzId, GueltigAb, Preis, Verrechnungspreis, Einkaufspreis
                FROM GlobalerPreis
                WHERE GlobalePreislisteId = @preislisteId
                  AND ZusatzId <> 0
                  AND GueltigAb <= @datumAb
                  AND (GueltigBis IS NULL OR NOT isfinite(GueltigBis) OR GueltigBis >= @datumAb);";
            command.Parameters.AddWithValue("preislisteId", preislisteId);
            command.Parameters.AddWithValue("datumAb", datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var zusatzId = (long)reader.GetInt32(0);
                DateTime gueltigAb = reader.GetDateTime(1);
                var preis = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                var verrechnungspreis = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                var einkaufspreis = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4);
                result[zusatzId] = (preis, verrechnungspreis, einkaufspreis, gueltigAb);
            }
            return result;
        }

        /// <summary>
        /// Speichert den Preis mit explizitem Gültigkeitszeitraum (GueltigAb/GueltigBis).
        /// Bei einem neuen Datensatz (altGueltigAb = 0) wird in GlobalerPreis angelegt.
        /// </summary>
        public void SpeichereGueltigkeit(long preislisteId, long zusatzId,
            DateTime altGueltigAb, DateTime neuGueltigAb, DateTime gueltigBis, decimal preis,
            decimal verrechnungspreis, decimal einkaufspreis)
        {
            using var connection = _database.CreateConnection();

            bool vorhanden = false;
            
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
            checkCmd.Parameters.AddWithValue("altGueltigAb", altGueltigAb);
            vorhanden = checkCmd.ExecuteScalar() != null;

            if (neuGueltigAb != altGueltigAb)
            {
                // Primarykey Änderung, Datensatz löschen und neu erstellen
                checkCmd.CommandText = @"
                DELETE
                FROM GlobalerPreis
                WHERE GlobalePreislisteId = @preislisteId
                    AND ZusatzId = @zusatzId
                    AND GueltigAb = @altGueltigAb;";
                checkCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                checkCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                checkCmd.Parameters.AddWithValue("altGueltigAb", altGueltigAb);
                checkCmd.ExecuteNonQuery(); 
                vorhanden = false;
            }

            if (vorhanden)
            {
                using var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE GlobalerPreis
                    SET GueltigBis = @gueltigBis,
                        Preis = @preis,
                        Verrechnungspreis = @verrechnungspreis,
                        Einkaufspreis = @einkaufspreis
                    WHERE GlobalePreislisteId = @preislisteId
                      AND ZusatzId = @zusatzId
                      AND GueltigAb = @altGueltigAb;";
                updateCmd.Parameters.AddWithValue("gueltigBis", gueltigBis);
                updateCmd.Parameters.AddWithValue("preis", preis);
                updateCmd.Parameters.AddWithValue("verrechnungspreis", verrechnungspreis);
                updateCmd.Parameters.AddWithValue("einkaufspreis", einkaufspreis);
                updateCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                updateCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                updateCmd.Parameters.AddWithValue("altGueltigAb", altGueltigAb);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO GlobalerPreis
                        (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis, Verrechnungspreis, Einkaufspreis)
                    VALUES (@preislisteId, @zusatzId, @neuGueltigAb, @gueltigBis, @preis, @verrechnungspreis, @einkaufspreis);";
                insertCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                insertCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                insertCmd.Parameters.AddWithValue("neuGueltigAb",neuGueltigAb);
                insertCmd.Parameters.AddWithValue("gueltigBis", gueltigBis);
                insertCmd.Parameters.AddWithValue("preis", preis);
                insertCmd.Parameters.AddWithValue("verrechnungspreis", verrechnungspreis);
                insertCmd.Parameters.AddWithValue("einkaufspreis", einkaufspreis);
                insertCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Stellt sicher, dass für jede Position, die vor/zum Stichtag jemals einen
        /// Preis hatte, auch am Stichtag ein gültiger Preis existiert (aus Vorperiode).
        /// </summary>
        public int UebernehmeFehlendeVorperiodenPreise(int datumAbInt)
        {
            DateTime datumAb = Termin.FromYyyyMmDd(datumAbInt);
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            DateTime vortag = datumAb.AddDays(-1d);

            var positionen = new List<(long PreislisteId, long ZusatzId)>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT DISTINCT GlobalePreislisteId, ZusatzId
                    FROM GlobalerPreis
                    WHERE GueltigAb <= @datumAb;";
                command.Parameters.AddWithValue("datumAb", datumAb);
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
                          AND (GueltigBis IS NULL OR NOT isfinite(GueltigBis) OR GueltigBis >= @datumAb)
                        LIMIT 1;";
                    checkCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    checkCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    checkCmd.Parameters.AddWithValue("datumAb", datumAb);
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        using var closeCmd = connection.CreateCommand();
                        closeCmd.CommandText = @"UPDATE GlobalerPreis
                                                  SET GueltigBis = @vortag
                                                  WHERE GlobalePreislisteId = @preislisteId
                                                  AND ZusatzId = @zusatzId
                                                  AND GueltigAb < @datumAb
                                                  AND (GueltigBis IS NULL OR NOT isfinite(GueltigBis))";
                        closeCmd.Parameters.AddWithValue("vortag", vortag);
                        closeCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                        closeCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                        closeCmd.Parameters.AddWithValue("datumAb", datumAb);
                        closeCmd.ExecuteNonQuery();
                        continue;
                    }
                }

                decimal? vorperiodenPreis = null;
                decimal vorperiodenVerrechnungspreis = 0m;
                decimal vorperiodenEinkaufspreis = 0m;
                using (var prevCmd = connection.CreateCommand())
                {
                    prevCmd.CommandText = @"
                        SELECT Preis, Verrechnungspreis, Einkaufspreis
                        FROM GlobalerPreis
                        WHERE GlobalePreislisteId = @preislisteId
                          AND ZusatzId = @zusatzId
                          AND GueltigAb <= @datumAb
                        ORDER BY GueltigAb DESC
                        LIMIT 1;";
                    prevCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    prevCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    prevCmd.Parameters.AddWithValue("datumAb", datumAb);
                    using var prevReader = prevCmd.ExecuteReader();
                    if (prevReader.Read())
                    {
                        if (!prevReader.IsDBNull(0))
                        {
                            vorperiodenPreis = prevReader.GetDecimal(0);
                        }
                        vorperiodenVerrechnungspreis = prevReader.IsDBNull(1) ? 0m : prevReader.GetDecimal(1);
                        vorperiodenEinkaufspreis = prevReader.IsDBNull(2) ? 0m : prevReader.GetDecimal(2);
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
                            (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis, Verrechnungspreis, Einkaufspreis)
                        VALUES (@preislisteId, @zusatzId, @datumAb, NULL, @preis, @verrechnungspreis, @einkaufspreis)
                        ON CONFLICT (GlobalePreislisteId, ZusatzId, GueltigAb)
                        DO UPDATE SET GueltigBis = NULL, Preis = EXCLUDED.Preis, Verrechnungspreis = EXCLUDED.Verrechnungspreis, Einkaufspreis = EXCLUDED.Einkaufspreis;";
                    insertCmd.Parameters.AddWithValue("preislisteId", preislisteId);
                    insertCmd.Parameters.AddWithValue("zusatzId", zusatzId);
                    insertCmd.Parameters.AddWithValue("datumAb", datumAb);
                    insertCmd.Parameters.AddWithValue("preis", vorperiodenPreis.Value);
                    insertCmd.Parameters.AddWithValue("verrechnungspreis", vorperiodenVerrechnungspreis);
                    insertCmd.Parameters.AddWithValue("einkaufspreis", vorperiodenEinkaufspreis);
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
        public void AddDatum(int quelleDatumInt, int neuDatumInt)
        {
            DateTime quelleDatum = Termin.FromYyyyMmDd(quelleDatumInt);
            DateTime neuDatum = Termin.FromYyyyMmDd(neuDatumInt);
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var quellPreise = new List<GlobalerPreis>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis, Verrechnungspreis, Einkaufspreis
                    FROM GlobalerPreis
                    WHERE GueltigAb <= @datum AND (GueltigBis IS NULL OR NOT isfinite(GueltigBis) OR GueltigBis >= @datum);";
                command.Parameters.AddWithValue("datum", quelleDatum);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    quellPreise.Add(new GlobalerPreis
                    {
                        GlobalePreislisteId = reader.GetInt32(0),
                        ZusatzId = reader.GetInt32(1),
                        GueltigAb = reader.GetDateTime(2),
                        GueltigBis = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                        Preis = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                        Verrechnungspreis = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                        Einkaufspreis = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6)
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
                    checkCmd.Parameters.AddWithValue("neuDatum", neuDatum);
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        continue;
                    }
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.CommandText = @"
                        INSERT INTO GlobalerPreis
                            (GlobalePreislisteId, ZusatzId, GueltigAb, GueltigBis, Preis, Verrechnungspreis, Einkaufspreis)
                        VALUES (@preislisteId, @zusatzId, @neuDatum, NULL, @preis, @verrechnungspreis, @einkaufspreis)
                        ON CONFLICT (GlobalePreislisteId, ZusatzId, GueltigAb)
                        DO UPDATE SET GueltigBis = NULL, Preis = EXCLUDED.Preis, Verrechnungspreis = EXCLUDED.Verrechnungspreis, Einkaufspreis = EXCLUDED.Einkaufspreis;";
                    insertCmd.Parameters.AddWithValue("preislisteId", zeile.GlobalePreislisteId);
                    insertCmd.Parameters.AddWithValue("zusatzId", zeile.ZusatzId);
                    insertCmd.Parameters.AddWithValue("neuDatum", neuDatum);
                    insertCmd.Parameters.AddWithValue("preis", zeile.Preis);
                    insertCmd.Parameters.AddWithValue("verrechnungspreis", zeile.Verrechnungspreis);
                    insertCmd.Parameters.AddWithValue("einkaufspreis", zeile.Einkaufspreis);
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
            command.Parameters.AddWithValue("status", eintrag.Status);
        }

        private static GlobalePreisliste Map(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Preiskategorie = reader.IsDBNull(2) ? Preiskategorie.Speisen : (Preiskategorie)reader.GetInt32(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetBoolean(4),
            StartDatum = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
            Zusatztext = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            Status = !reader.IsDBNull(7) && reader.GetBoolean(7),
        };

        private static GlobalePreislisteZusatz MapZusatz(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            SpeisekarteId = reader.GetInt32(1),
            Bezeichnung = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetBoolean(4),
            Status = !reader.IsDBNull(5) && reader.GetBoolean(5)
        };
    }
}

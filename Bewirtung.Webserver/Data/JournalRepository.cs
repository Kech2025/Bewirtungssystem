namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Ein Journal-Eintrag (Tabelle journal): protokolliert An-/Abmeldungen der Bestellung-App.
    /// </summary>
    public class JournalEintrag
    {
        public int UserId { get; set; }
        public string BenutzerName { get; set; } = string.Empty;
        public DateTime Eingeloggt { get; set; }
        public DateTime? Ausgeloggt { get; set; }
        public int? Modus { get; set; }
        public string ModusText { get; set; } = string.Empty;
        public DateTime? Termin { get; set; }
    }

    /// <summary>
    /// Nur-Lese-Datenzugriff für das Journal (Tabelle journal). Liefert die neuesten
    /// Anmeldungen zuerst und reichert den Benutzernamen aus der Tabelle Benutzer an.
    /// </summary>
    public class JournalRepository
    {
        /// <summary>Modus-Wert für Anmeldungen am Webserver (wird als "Webseite" angezeigt).</summary>
        public const int ModusWebseite = -1;

        private readonly Database _database;

        public JournalRepository(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Protokolliert eine Anmeldung (Login) in der Tabelle journal.
        /// </summary>
        public void Login(long userId, int modus)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO journal (userid, eingeloggt, modus, termin)
                VALUES (@userid, @eingeloggt, @modus, NULL);";
            command.Parameters.AddWithValue("@userid", (int)userId);
            command.Parameters.AddWithValue("@eingeloggt", DateTime.Now);
            command.Parameters.AddWithValue("@modus", modus);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Schließt den letzten noch offenen Journal-Eintrag des Benutzers ab (Logout).
        /// </summary>
        public void Logout(long userId, int modus)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE journal
                SET ausgeloggt = @ausgeloggt
                WHERE userid = @userid
                  AND eingeloggt = (
                      SELECT MAX(eingeloggt) FROM journal
                      WHERE userid = @userid AND modus = @modus AND ausgeloggt IS NULL
                  );";
            command.Parameters.AddWithValue("@ausgeloggt", DateTime.Now);
            command.Parameters.AddWithValue("@userid", (int)userId);
            command.Parameters.AddWithValue("@modus", modus);
            command.ExecuteNonQuery();
        }

        public List<JournalEintrag> GetAll(int limit = 500)
        {
            var result = new List<JournalEintrag>();
            using var connection = _database.CreateConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT j.userid, COALESCE(b.Name, '') AS name,
                           j.eingeloggt, j.ausgeloggt, j.modus, j.termin
                    FROM journal j
                    LEFT JOIN Benutzer b ON b.UserId = j.userid
                    ORDER BY j.eingeloggt DESC
                    LIMIT @limit;";
                command.Parameters.AddWithValue("@limit", limit);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new JournalEintrag
                    {
                        UserId = reader.GetInt32(0),
                        BenutzerName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Eingeloggt = reader.GetDateTime(2),
                        Ausgeloggt = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        Modus = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        Termin = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                    });
                }
            }

            // Modus-Text nachschlagen: Picker-Index 0 = "Bestellung", danach die dem
            // Termin (Datum) zugeordneten Küchenstationen – nach Name sortiert, genau wie
            // auf der Anmeldeseite der Bestellung-App.
            var stationenJeDatum = new Dictionary<DateTime, List<string>>();
            foreach (var eintrag in result)
            {
                if (eintrag.Modus is not int modus)
                {
                    continue;
                }

                if (modus == ModusWebseite)
                {
                    eintrag.ModusText = "Webseite";
                    continue;
                }

                if (modus == 0)
                {
                    eintrag.ModusText = "Bestellung";
                    continue;
                }

                if (eintrag.Termin is not DateTime termin)
                {
                    eintrag.ModusText = modus.ToString();
                    continue;
                }

                if (!stationenJeDatum.TryGetValue(termin.Date, out var stationen))
                {
                    stationen = GetStationNamenFuerDatum(connection, termin.Date);
                    stationenJeDatum[termin.Date] = stationen;
                }

                var stationIndex = modus - 1;
                eintrag.ModusText = stationIndex >= 0 && stationIndex < stationen.Count
                    ? stationen[stationIndex]
                    : modus.ToString();
            }

            return result;
        }

        private static List<string> GetStationNamenFuerDatum(
            Npgsql.NpgsqlConnection connection, DateTime datum)
        {
            var namen = new List<string>();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT k.name
                FROM kuechenstationtermin kst
                JOIN kuechenstation k ON k.id = kst.kuechenstationid
                WHERE kst.bewirtungsdatum = @datum
                ORDER BY k.name;";
            command.Parameters.AddWithValue("@datum", datum);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                namen.Add(reader.IsDBNull(0) ? string.Empty : reader.GetString(0));
            }
            return namen;
        }
    }
}

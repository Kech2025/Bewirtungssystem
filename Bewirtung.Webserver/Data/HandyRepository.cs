namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Handys (Tabelle handy). Löschen wird nicht unterstützt.
    /// </summary>
    public class HandyRepository
    {
        private readonly Database _database;

        public HandyRepository(Database database)
        {
            _database = database;
        }

        public List<Handy> GetAll()
        {
            var result = new List<Handy>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT seriennummer, frei, userid, activated, name, terminated, bestellung
                FROM handy
                ORDER BY name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Handy
                {
                    Seriennummer = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Frei = !reader.IsDBNull(1) && reader.GetBoolean(1),
                    UserId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    Activated = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    Name = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Terminated = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    Bestellung = !reader.IsDBNull(6) && reader.GetBoolean(6)
                });
            }
            return result;
        }

        public void Insert(Handy handy)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO handy (seriennummer, frei, userid, activated, name, terminated, bestellung)
                VALUES (@seriennummer, @frei, @userid, @activated, @name, @terminated, @bestellung);";
            AddParameters(command, handy);
            command.ExecuteNonQuery();
        }

        public void Update(Handy handy)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE handy SET
                    frei = @frei,
                    userid = @userid,
                    activated = @activated,
                    name = @name,
                    terminated = @terminated,
                    bestellung = @bestellung
                WHERE seriennummer = @seriennummer;";
            AddParameters(command, handy);
            command.ExecuteNonQuery();
        }

        public List<BenutzerOption> GetBenutzer()
        {
            var result = new List<BenutzerOption>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Name FROM Benutzer ORDER BY Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new BenutzerOption
                {
                    UserId = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        private static void AddParameters(Npgsql.NpgsqlCommand command, Handy handy)
        {
            command.Parameters.AddWithValue("seriennummer", (object?)handy.Seriennummer ?? DBNull.Value);
            command.Parameters.AddWithValue("frei", handy.Frei);
            command.Parameters.AddWithValue("userid", (object?)handy.UserId ?? DBNull.Value);
            command.Parameters.AddWithValue("activated", (object?)handy.Activated ?? DBNull.Value);
            command.Parameters.AddWithValue("name", (object?)handy.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("terminated", (object?)handy.Terminated ?? DBNull.Value);
            command.Parameters.AddWithValue("bestellung", handy.Bestellung);
        }
    }
}

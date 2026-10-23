namespace Bewirtung.Webserver.Data
{
    using Npgsql;

    /// <summary>
    /// Datenzugriff für Vereine (Tabelle Verein) und deren Ansprechpartner (Tabelle Benutzer).
    /// </summary>
    public class VereinRepository
    {
        private readonly Database _database;

        public VereinRepository(Database database)
        {
            _database = database;
        }

        public List<VereinOption> GetAll()
        {
            var result = new List<VereinOption>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name FROM Verein ORDER BY Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new VereinOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public List<Verein> GetAllVereine()
        {
            var result = new List<Verein>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT v.Id, v.Name, v.AnsprechpartnerId, v.Strasse, v.PLZ, v.Ort, b.Name AS AnsprechpartnerName
                FROM Verein v
                LEFT JOIN Benutzer b ON b.UserId = v.AnsprechpartnerId
                ORDER BY v.Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Map(reader));
            }
            return result;
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

        public long Insert(Verein verein)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Verein (Name, AnsprechpartnerId, Strasse, PLZ, Ort)
                VALUES (@name, @ansprechpartnerId, @strasse, @plz, @ort)
                RETURNING Id;";
            AddParameters(command, verein);
            var id = Convert.ToInt64(command.ExecuteScalar()!);
            verein.Id = id;
            return id;
        }

        public void Update(Verein verein)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Verein SET
                    Name = @name,
                    AnsprechpartnerId = @ansprechpartnerId,
                    Strasse = @strasse,
                    PLZ = @plz,
                    Ort = @ort
                WHERE Id = @id;";
            AddParameters(command, verein);
            command.Parameters.AddWithValue("id", verein.Id);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Verein WHERE Id = @id;";
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(NpgsqlCommand command, Verein verein)
        {
            command.Parameters.AddWithValue("name", (object?)verein.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("ansprechpartnerId", (object?)verein.AnsprechpartnerId ?? DBNull.Value);
            command.Parameters.AddWithValue("strasse", (object?)verein.Strasse ?? DBNull.Value);
            command.Parameters.AddWithValue("plz", (object?)verein.Plz ?? DBNull.Value);
            command.Parameters.AddWithValue("ort", (object?)verein.Ort ?? DBNull.Value);
        }

        private static Verein Map(NpgsqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            AnsprechpartnerId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            Strasse = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Plz = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Ort = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            AnsprechpartnerName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
        };
    }
}

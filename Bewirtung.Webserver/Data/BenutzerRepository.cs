using Microsoft.Data.Sqlite;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Benutzer (Tabelle Benutzer).
    /// </summary>
    public class BenutzerRepository
    {
        private readonly Database _database;

        public BenutzerRepository(Database database)
        {
            _database = database;
        }

        public List<Benutzer> GetAll()
        {
            var result = new List<Benutzer>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT b.UserId, b.Name, b.Email, b.Password, b.Administrator, b.Telefon, b.VereinId, v.Name AS VereinName
                FROM Benutzer b
                LEFT JOIN Verein v ON v.Id = b.VereinId
                ORDER BY b.Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Map(reader));
            }
            return result;
        }

        public Benutzer? GetById(long userId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT b.UserId, b.Name, b.Email, b.Password, b.Administrator, b.Telefon, b.VereinId, v.Name AS VereinName
                FROM Benutzer b
                LEFT JOIN Verein v ON v.Id = b.VereinId
                WHERE b.UserId = $userId;";
            command.Parameters.AddWithValue("$userId", userId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public long Insert(Benutzer benutzer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Benutzer (Name, Email, Password, Administrator, Telefon, VereinId)
                VALUES ($name, $email, $password, $administrator, $telefon, $vereinId);
                SELECT last_insert_rowid();";
            AddParameters(command, benutzer);
            var id = (long)command.ExecuteScalar()!;
            benutzer.UserId = id;
            return id;
        }

        public void Update(Benutzer benutzer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Benutzer SET
                    Name = $name,
                    Email = $email,
                    Password = $password,
                    Administrator = $administrator,
                    Telefon = $telefon,
                    VereinId = $vereinId
                WHERE UserId = $userId;";
            AddParameters(command, benutzer);
            command.Parameters.AddWithValue("$userId", benutzer.UserId);
            command.ExecuteNonQuery();
        }

        public void Delete(long userId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Benutzer WHERE UserId = $userId;";
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(SqliteCommand command, Benutzer benutzer)
        {
            command.Parameters.AddWithValue("$name", benutzer.Name);
            command.Parameters.AddWithValue("$email", benutzer.Email);
            command.Parameters.AddWithValue("$password", benutzer.Password);
            command.Parameters.AddWithValue("$administrator", benutzer.Administrator ? 1 : 0);
            command.Parameters.AddWithValue("$telefon", benutzer.Telefon);
            command.Parameters.AddWithValue("$vereinId", (object?)benutzer.VereinId ?? DBNull.Value);
        }

        private static Benutzer Map(SqliteDataReader reader) => new()
        {
            UserId = reader.GetInt64(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Password = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Administrator = !reader.IsDBNull(4) && reader.GetInt64(4) != 0,
            Telefon = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            VereinId = reader.IsDBNull(6) ? null : reader.GetInt64(6),
            VereinName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
        };
    }
}

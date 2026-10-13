using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für Vereine (Tabelle Verein) und deren Ansprechpartner (Tabelle Benutzer).
    /// </summary>
    public class VereinRepository
    {
        public List<Verein> GetAll()
        {
            var result = new List<Verein>();
            using var connection = Database.CreateConnection();
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

        public List<Benutzer> GetBenutzer()
        {
            var result = new List<Benutzer>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT UserId, Name FROM Benutzer ORDER BY Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Benutzer
                {
                    UserId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public long Insert(Verein verein)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Verein (Name, AnsprechpartnerId, Strasse, PLZ, Ort)
                VALUES ($name, $ansprechpartnerId, $strasse, $plz, $ort);
                SELECT last_insert_rowid();";
            AddParameters(command, verein);
            var id = (long)command.ExecuteScalar()!;
            verein.Id = id;
            return id;
        }

        public void Update(Verein verein)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Verein SET
                    Name = $name,
                    AnsprechpartnerId = $ansprechpartnerId,
                    Strasse = $strasse,
                    PLZ = $plz,
                    Ort = $ort
                WHERE Id = $id;";
            AddParameters(command, verein);
            command.Parameters.AddWithValue("$id", verein.Id);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Verein WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(SqliteCommand command, Verein verein)
        {
            command.Parameters.AddWithValue("$name", verein.Name);
            command.Parameters.AddWithValue("$ansprechpartnerId",
                verein.AnsprechpartnerId.HasValue ? verein.AnsprechpartnerId.Value : DBNull.Value);
            command.Parameters.AddWithValue("$strasse", verein.Strasse);
            command.Parameters.AddWithValue("$plz", verein.Plz);
            command.Parameters.AddWithValue("$ort", verein.Ort);
        }

        private static Verein Map(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt64(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            AnsprechpartnerId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
            Strasse = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Plz = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Ort = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            AnsprechpartnerName = reader.FieldCount > 6 && !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty
        };
    }
}

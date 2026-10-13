using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für Tische (Tabelle Tisch).
    /// </summary>
    public class TischRepository
    {
        public List<Tisch> GetAll()
        {
            var result = new List<Tisch>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Tischnummer, t.BereichId, b.Bezeichnung
                FROM Tisch t
                LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
                ORDER BY t.Tischnummer;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Tisch
                {
                    Tischnummer = (int)reader.GetInt64(0),
                    BereichId = reader.IsDBNull(1) ? null : reader.GetInt64(1),
                    BereichName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                });
            }
            return result;
        }

        public bool Exists(int tischnummer)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Tisch WHERE Tischnummer = $nr;";
            command.Parameters.AddWithValue("$nr", tischnummer);
            return (long)command.ExecuteScalar()! > 0;
        }

        public void Insert(Tisch tisch)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Tisch (Tischnummer, BereichId) VALUES ($nr, $bereichId);";
            command.Parameters.AddWithValue("$nr", tisch.Tischnummer);
            command.Parameters.AddWithValue("$bereichId",
                tisch.BereichId.HasValue ? tisch.BereichId.Value : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Update(int alteNummer, Tisch neu)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tisch SET Tischnummer = $neu, BereichId = $bereichId WHERE Tischnummer = $alt;";
            command.Parameters.AddWithValue("$neu", neu.Tischnummer);
            command.Parameters.AddWithValue("$bereichId",
                neu.BereichId.HasValue ? neu.BereichId.Value : DBNull.Value);
            command.Parameters.AddWithValue("$alt", alteNummer);
            command.ExecuteNonQuery();
        }

        public void Delete(int tischnummer)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tisch WHERE Tischnummer = $nr;";
            command.Parameters.AddWithValue("$nr", tischnummer);
            command.ExecuteNonQuery();
        }
    }
}

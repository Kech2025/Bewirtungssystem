using Npgsql;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Tische (Tabelle Tisch).
    /// </summary>
    public class TischRepository
    {
        private readonly Database _database;

        public TischRepository(Database database)
        {
            _database = database;
        }

        public List<Tisch> GetAll(bool all = true)
        {
            var result = new List<Tisch>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            if(all)
            { 
            command.CommandText = @"
                SELECT t.Tischnummer, t.BereichId, b.Bezeichnung, COALESCE(t.SofortLieferung, false), t.status
                FROM Tisch t
                LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
                ORDER BY t.Tischnummer;";
            }
            else
            {
                command.CommandText = @"
                SELECT t.Tischnummer, t.BereichId, b.Bezeichnung, COALESCE(t.SofortLieferung, false), t.status
                FROM Tisch t
                LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
                WHERE t.Status = true
                ORDER BY t.Tischnummer;";
            }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Tisch
                {
                    Tischnummer = reader.GetInt32(0),
                    BereichId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    BereichName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    SofortLieferung = (!reader.IsDBNull(3) && reader.GetBoolean(3)) ? 1 : 0,
                    Status = !reader.IsDBNull(4) && reader.GetBoolean(4),
                });
            }
            return result;
        }

        public bool Exists(int tischnummer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Tisch WHERE Tischnummer = @nr;";
            command.Parameters.AddWithValue("nr", tischnummer);
            return Convert.ToInt64(command.ExecuteScalar()!) > 0;
        }

        public void Insert(Tisch tisch)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Tisch (Tischnummer, BereichId, SofortLieferung, status) VALUES (@nr, @bereichId, @sofort, @status);";
            command.Parameters.AddWithValue("nr", tisch.Tischnummer);
            command.Parameters.AddWithValue("bereichId", (object?)tisch.BereichId ?? DBNull.Value);
            command.Parameters.AddWithValue("sofort", tisch.SofortLieferung != 0);
            command.Parameters.AddWithValue("status", tisch.Status);
            command.ExecuteNonQuery();
        }

        public void Update(int alteNummer, Tisch neu)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tisch SET Tischnummer = @neu, BereichId = @bereichId, SofortLieferung = @sofort, status = @status WHERE Tischnummer = @alt;";
            command.Parameters.AddWithValue("neu", neu.Tischnummer);
            command.Parameters.AddWithValue("bereichId", (object?)neu.BereichId ?? DBNull.Value);
            command.Parameters.AddWithValue("sofort", neu.SofortLieferung != 0);
            command.Parameters.AddWithValue("alt", alteNummer);
            command.Parameters.AddWithValue("status", neu.Status);
            command.ExecuteNonQuery();
        }

        public void Delete(int tischnummer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tisch SET status = false WHERE Tischnummer = @nr;";
            command.Parameters.AddWithValue("nr", tischnummer);
            command.ExecuteNonQuery();
        }
    }
}

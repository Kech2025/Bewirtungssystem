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

        public List<Tisch> GetAll()
        {
            var result = new List<Tisch>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.Tischnummer, t.BereichId, b.Bezeichnung, COALESCE(t.SofortLieferung, false)
                FROM Tisch t
                LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
                ORDER BY t.Tischnummer;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Tisch
                {
                    Tischnummer = reader.GetInt32(0),
                    BereichId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    BereichName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    SofortLieferung = (!reader.IsDBNull(3) && reader.GetBoolean(3)) ? 1 : 0
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
            command.CommandText = "INSERT INTO Tisch (Tischnummer, BereichId, SofortLieferung) VALUES (@nr, @bereichId, @sofort);";
            command.Parameters.AddWithValue("nr", tisch.Tischnummer);
            command.Parameters.AddWithValue("bereichId", (object?)tisch.BereichId ?? DBNull.Value);
            command.Parameters.AddWithValue("sofort", tisch.SofortLieferung != 0);
            command.ExecuteNonQuery();
        }

        public void Update(int alteNummer, Tisch neu)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tisch SET Tischnummer = @neu, BereichId = @bereichId, SofortLieferung = @sofort WHERE Tischnummer = @alt;";
            command.Parameters.AddWithValue("neu", neu.Tischnummer);
            command.Parameters.AddWithValue("bereichId", (object?)neu.BereichId ?? DBNull.Value);
            command.Parameters.AddWithValue("sofort", neu.SofortLieferung != 0);
            command.Parameters.AddWithValue("alt", alteNummer);
            command.ExecuteNonQuery();
        }

        public void Delete(int tischnummer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tisch WHERE Tischnummer = @nr;";
            command.Parameters.AddWithValue("nr", tischnummer);
            command.ExecuteNonQuery();
        }
    }
}

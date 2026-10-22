using Microsoft.Data.Sqlite;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Tisch-Termin-Zuordnungen (Tabelle TischTermin).
    /// </summary>
    public class TischTerminRepository
    {
        private readonly Database _database;

        public TischTerminRepository(Database database)
        {
            _database = database;
        }

        public List<TischTermin> GetAll()
        {
            var result = new List<TischTermin>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT tt.Bewirtungsdatum, tt.VereinId, tt.Tischnummer, v.Name, b.Bezeichnung
                FROM TischTermin tt
                LEFT JOIN Verein v ON v.Id = tt.VereinId
                LEFT JOIN Tisch t ON t.Tischnummer = tt.Tischnummer
                LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
                ORDER BY tt.Bewirtungsdatum DESC, v.Name, tt.Tischnummer;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new TischTermin
                {
                    Bewirtungsdatum = (int)reader.GetInt64(0),
                    VereinId = reader.GetInt64(1),
                    Tischnummer = (int)reader.GetInt64(2),
                    VereinName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    BereichName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                });
            }
            return result;
        }

        public void Insert(TischTermin eintrag)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO TischTermin (Bewirtungsdatum, VereinId, Tischnummer)
                VALUES ($datum, $vereinId, $tischnummer);";
            command.Parameters.AddWithValue("$datum", eintrag.Bewirtungsdatum);
            command.Parameters.AddWithValue("$vereinId", eintrag.VereinId);
            command.Parameters.AddWithValue("$tischnummer", eintrag.Tischnummer);
            command.ExecuteNonQuery();
        }

        public void Delete(int datum, long vereinId, int tischnummer)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM TischTermin
                WHERE Bewirtungsdatum = $datum AND VereinId = $vereinId AND Tischnummer = $tischnummer;";
            command.Parameters.AddWithValue("$datum", datum);
            command.Parameters.AddWithValue("$vereinId", vereinId);
            command.Parameters.AddWithValue("$tischnummer", tischnummer);
            command.ExecuteNonQuery();
        }
    }
}

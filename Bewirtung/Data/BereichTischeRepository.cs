using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für Tisch-Bereiche (Tabelle BereichTische).
    /// </summary>
    public class BereichTischeRepository
    {
        public List<BereichTische> GetAll()
        {
            var result = new List<BereichTische>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BereichId, Bezeichnung FROM BereichTische ORDER BY Bezeichnung;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new BereichTische
                {
                    BereichId = reader.GetInt64(0),
                    Bezeichnung = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public long Insert(BereichTische bereich)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO BereichTische (Bezeichnung)
                VALUES ($bezeichnung);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$bezeichnung", bereich.Bezeichnung);
            var id = (long)command.ExecuteScalar()!;
            bereich.BereichId = id;
            return id;
        }

        public void Update(BereichTische bereich)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE BereichTische SET
                    Bezeichnung = $bezeichnung
                WHERE BereichId = $id;";
            command.Parameters.AddWithValue("$bezeichnung", bereich.Bezeichnung);
            command.Parameters.AddWithValue("$id", bereich.BereichId);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM BereichTische WHERE BereichId = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}

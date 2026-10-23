namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Tisch-Bereiche (Tabelle BereichTische), Lesezugriff für die Auswahl.
    /// </summary>
    public class BereichTischeRepository
    {
        private readonly Database _database;

        public BereichTischeRepository(Database database)
        {
            _database = database;
        }

        public List<BereichOption> GetAll()
        {
            var result = new List<BereichOption>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BereichId, Bezeichnung FROM BereichTische ORDER BY Bezeichnung;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new BereichOption
                {
                    BereichId = reader.GetInt32(0),
                    Bezeichnung = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public long Insert(BereichOption bereich)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO BereichTische (Bezeichnung)
                VALUES (@bezeichnung)
                RETURNING BereichId;";
            command.Parameters.AddWithValue("bezeichnung", (object?)bereich.Bezeichnung ?? DBNull.Value);
            var id = Convert.ToInt64(command.ExecuteScalar()!);
            bereich.BereichId = id;
            return id;
        }

        public void Update(BereichOption bereich)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE BereichTische SET
                    Bezeichnung = @bezeichnung
                WHERE BereichId = @id;";
            command.Parameters.AddWithValue("bezeichnung", (object?)bereich.Bezeichnung ?? DBNull.Value);
            command.Parameters.AddWithValue("id", bereich.BereichId);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM BereichTische WHERE BereichId = @id;";
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }
    }
}

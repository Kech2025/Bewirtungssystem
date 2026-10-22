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
                    BereichId = reader.GetInt64(0),
                    Bezeichnung = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }
    }
}

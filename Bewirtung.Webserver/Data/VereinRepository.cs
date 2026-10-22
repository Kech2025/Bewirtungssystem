namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Vereine (nur Lesezugriff für die Auswahl).
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
                    Id = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }
    }
}

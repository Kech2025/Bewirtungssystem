namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Küchenstationen (Tabelle kuechenstation). Löschen wird nicht unterstützt.
    /// </summary>
    public class KuechenstationRepository
    {
        private readonly Database _database;

        public KuechenstationRepository(Database database)
        {
            _database = database;
        }

        public List<Kuechenstation> GetAll()
        {
            var result = new List<Kuechenstation>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name FROM kuechenstation ORDER BY id;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Kuechenstation
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public void Insert(Kuechenstation station)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO kuechenstation (id, name) VALUES (@id, @name);";
            command.Parameters.AddWithValue("id", station.Id);
            command.Parameters.AddWithValue("name", (object?)station.Name ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Update(Kuechenstation station)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE kuechenstation SET name = @name WHERE id = @id;";
            command.Parameters.AddWithValue("name", (object?)station.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("id", station.Id);
            command.ExecuteNonQuery();
        }
    }
}

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

        public List<Kuechenstation> GetAll(bool all = true)
        {
            var result = new List<Kuechenstation>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            if(all)
            {
                command.CommandText = "SELECT id, name, status FROM kuechenstation ORDER BY id;";
            }
            else
            {
                command.CommandText = "SELECT id, name, status FROM kuechenstation WHERE status = true ORDER BY id;";
            }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Kuechenstation
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Status = reader.GetBoolean(2)
                });
            }
            return result;
        }

        public void Insert(Kuechenstation station)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO kuechenstation (id, name, status) VALUES (@id, @name, @status);";
            command.Parameters.AddWithValue("id", station.Id);
            command.Parameters.AddWithValue("name", (object?)station.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("status", station.Status);
            command.ExecuteNonQuery();
        }

        public void Update(Kuechenstation station)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE kuechenstation SET name = @name, status = @status WHERE id = @id;";
            command.Parameters.AddWithValue("name", (object?)station.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("status", station.Status);
            command.Parameters.AddWithValue("id", station.Id);
            command.ExecuteNonQuery();
        }
    }
}

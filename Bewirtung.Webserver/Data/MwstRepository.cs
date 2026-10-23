namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Mehrwertsteuersätze (Tabelle Mwst).
    /// </summary>
    public class MwstRepository
    {
        private readonly Database _database;

        public MwstRepository(Database database)
        {
            _database = database;
        }

        public List<MwstSatz> GetAll()
        {
            var result = new List<MwstSatz>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Preiskategorie, Mwst FROM Mwst ORDER BY Preiskategorie;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new MwstSatz
                {
                    Preiskategorie = reader.GetInt32(0),
                    Mwst = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1)
                });
            }
            return result;
        }

        public void Update(MwstSatz satz)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Mwst SET
                    Mwst = @mwst
                WHERE Preiskategorie = @preiskategorie;";
            command.Parameters.AddWithValue("mwst", satz.Mwst);
            command.Parameters.AddWithValue("preiskategorie", satz.Preiskategorie);
            var betroffen = command.ExecuteNonQuery();

            if (betroffen == 0)
            {
                using var insert = connection.CreateCommand();
                insert.CommandText = @"
                    INSERT INTO Mwst (Preiskategorie, Mwst)
                    VALUES (@preiskategorie, @mwst);";
                insert.Parameters.AddWithValue("preiskategorie", satz.Preiskategorie);
                insert.Parameters.AddWithValue("mwst", satz.Mwst);
                insert.ExecuteNonQuery();
            }
        }
    }
}

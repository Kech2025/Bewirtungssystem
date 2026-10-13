using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für die globale Preisliste (Tabelle GlobalePreisliste)
    /// und deren Zusatz-Positionen (Tabelle GlobalePreislisteZusatz).
    /// </summary>
    public class GlobalePreislisteRepository
    {
        public List<GlobalePreisliste> GetAll(int datumAb)
        {
            var result = new List<GlobalePreisliste>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Preiskategorie, BildPfad, Druck, MAX(GueltigAb) AS GueltigAb, Zusatztext
                FROM GlobalePreisliste
                LEFT JOIN GlobalerPreis ON GlobalePreislisteId = Id
                AND GueltigAb <= $DatumAb
                GROUP BY Id, Name, Preiskategorie, BildPfad, Druck, Zusatztext
                ORDER BY Preiskategorie, Name;";
            command.Parameters.AddWithValue("$DatumAb", datumAb);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Map(reader));
            }
            return result;
        }

        public long Insert(GlobalePreisliste eintrag)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalePreisliste (Name, Preiskategorie, BildPfad, Druck, Zusatztext)
                VALUES ($name, $preiskategorie, $bildPfad, $druck, $zusatztext);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$bildPfad", eintrag.BildPfad);
            command.Parameters.AddWithValue("$name", eintrag.Name);
            command.Parameters.AddWithValue("$preiskategorie", eintrag.Preiskategorie);
            command.Parameters.AddWithValue("$druck", eintrag.Druck ? 1 : 0 );
            command.Parameters.AddWithValue("$zusatztext", eintrag.Zusatztext);
            var id = (long)command.ExecuteScalar()!;
            eintrag.Id = id;
            return id;
        }

        public void Update(GlobalePreisliste eintrag)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalePreisliste SET
                    Name = $name,
                    Preiskategorie = $preiskategorie,
                    BildPfad = $bildPfad,
                    Druck = $druck,
                    Zusatztext = $zusatztext
                WHERE Id = $id;";
            AddParameters(command, eintrag);
            command.Parameters.AddWithValue("$id", eintrag.Id);
            command.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var connection = Database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var deleteZusatz = connection.CreateCommand())
            {
                deleteZusatz.CommandText = "DELETE FROM GlobalePreislisteZusatz WHERE SpeisekarteId = $id;";
                deleteZusatz.Parameters.AddWithValue("$id", id);
                deleteZusatz.ExecuteNonQuery();
            }

            using (var deleteEintrag = connection.CreateCommand())
            {
                deleteEintrag.CommandText = "DELETE FROM GlobalePreisliste WHERE Id = $id;";
                deleteEintrag.Parameters.AddWithValue("$id", id);
                deleteEintrag.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public List<GlobalePreislisteZusatz> GetZusatz(long speisekarteId)
        {
            var result = new List<GlobalePreislisteZusatz>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, SpeisekarteId, Bezeichnung, BildPfad, Druck
                FROM GlobalePreislisteZusatz
                WHERE SpeisekarteId = $speisekarteId
                ORDER BY Bezeichnung;";
            command.Parameters.AddWithValue("$speisekarteId", speisekarteId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapZusatz(reader));
            }
            return result;
        }

        public long InsertZusatz(GlobalePreislisteZusatz zusatz)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO GlobalePreislisteZusatz (SpeisekarteId, Bezeichnung, BildPfad, Druck)
                VALUES ($speisekarteId, $bezeichnung, $bildPfad, $druck);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$speisekarteId", zusatz.SpeisekarteId);
            command.Parameters.AddWithValue("$bezeichnung", zusatz.Bezeichnung);
            command.Parameters.AddWithValue("$bildPfad", zusatz.BildPfad);
            command.Parameters.AddWithValue("$druck", zusatz.Druck ? 1 : 0);
            var id = (long)command.ExecuteScalar()!;
            zusatz.Id = id;
            return id;
        }

        public void UpdateZusatz(GlobalePreislisteZusatz zusatz)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE GlobalePreislisteZusatz SET
                    Bezeichnung = $bezeichnung,
                    BildPfad = $bildPfad,
                    Druck = $druck
                WHERE Id = $id;";
            command.Parameters.AddWithValue("$bezeichnung", zusatz.Bezeichnung);
            command.Parameters.AddWithValue("$bildPfad", zusatz.BildPfad);
            command.Parameters.AddWithValue("$druck", zusatz.Druck ? 1 : 0);
            command.Parameters.AddWithValue("$id", zusatz.Id);
            command.ExecuteNonQuery();
        }

        public void DeleteZusatz(long id)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM GlobalePreislisteZusatz WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        private static void AddParameters(SqliteCommand command, GlobalePreisliste eintrag)
        {
            command.Parameters.AddWithValue("$name", eintrag.Name);
            command.Parameters.AddWithValue("$preiskategorie", (int)eintrag.Preiskategorie);
            command.Parameters.AddWithValue("$bildPfad", eintrag.BildPfad);
            command.Parameters.AddWithValue("$druck", eintrag.Druck ? 1 : 0);
            command.Parameters.AddWithValue("$zusatztext", eintrag.Zusatztext);
        }

        private static GlobalePreisliste Map(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt64(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Preiskategorie = reader.IsDBNull(2) ? Preiskategorie.Speisen : (Preiskategorie)reader.GetInt64(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetInt64(4) != 0,
            StartDatum = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
            Zusatztext = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
        };

        private static GlobalePreislisteZusatz MapZusatz(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt64(0),
            SpeisekarteId = reader.GetInt64(1),
            Bezeichnung = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            BildPfad = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Druck = !reader.IsDBNull(4) && reader.GetInt64(4) != 0
        };
    }
}

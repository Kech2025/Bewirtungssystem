using Microsoft.Data.Sqlite;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Termine (Tabelle Termin) und deren Helfer (Tabelle HelferTermin).
    /// </summary>
    public class TerminRepository
    {
        private readonly Database _database;

        public TerminRepository(Database database)
        {
            _database = database;
        }

        public List<Termin> GetAll()
        {
            var result = new List<Termin>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT t.BewirtungsDatum, t.VereinId, v.Name
                FROM Termin t
                LEFT JOIN Verein v ON v.Id = t.VereinId
                ORDER BY t.BewirtungsDatum DESC, v.Name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Termin
                {
                    BewirtungsDatum = (int)reader.GetInt64(0),
                    VereinId = reader.GetInt64(1),
                    VereinName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                });
            }
            return result;
        }

        public void Insert(Termin termin)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Termin (BewirtungsDatum, VereinId)
                VALUES ($datum, $vereinId);";
            command.Parameters.AddWithValue("$datum", termin.BewirtungsDatum);
            command.Parameters.AddWithValue("$vereinId", termin.VereinId);
            command.ExecuteNonQuery();
        }

        public void Update(int altDatum, long altVereinId, Termin neu)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var updateHelfer = connection.CreateCommand())
            {
                updateHelfer.CommandText = @"
                    UPDATE HelferTermin
                    SET Bewirtungsdatum = $neuDatum, VereinId = $neuVerein
                    WHERE Bewirtungsdatum = $altDatum AND VereinId = $altVerein;";
                updateHelfer.Parameters.AddWithValue("$neuDatum", neu.BewirtungsDatum);
                updateHelfer.Parameters.AddWithValue("$neuVerein", neu.VereinId);
                updateHelfer.Parameters.AddWithValue("$altDatum", altDatum);
                updateHelfer.Parameters.AddWithValue("$altVerein", altVereinId);
                updateHelfer.ExecuteNonQuery();
            }

            using (var updateTermin = connection.CreateCommand())
            {
                updateTermin.CommandText = @"
                    UPDATE Termin
                    SET BewirtungsDatum = $neuDatum, VereinId = $neuVerein
                    WHERE BewirtungsDatum = $altDatum AND VereinId = $altVerein;";
                updateTermin.Parameters.AddWithValue("$neuDatum", neu.BewirtungsDatum);
                updateTermin.Parameters.AddWithValue("$neuVerein", neu.VereinId);
                updateTermin.Parameters.AddWithValue("$altDatum", altDatum);
                updateTermin.Parameters.AddWithValue("$altVerein", altVereinId);
                updateTermin.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void Delete(int datum, long vereinId)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var deleteHelfer = connection.CreateCommand())
            {
                deleteHelfer.CommandText =
                    "DELETE FROM HelferTermin WHERE Bewirtungsdatum = $datum AND VereinId = $vereinId;";
                deleteHelfer.Parameters.AddWithValue("$datum", datum);
                deleteHelfer.Parameters.AddWithValue("$vereinId", vereinId);
                deleteHelfer.ExecuteNonQuery();
            }

            using (var deleteTermin = connection.CreateCommand())
            {
                deleteTermin.CommandText =
                    "DELETE FROM Termin WHERE BewirtungsDatum = $datum AND VereinId = $vereinId;";
                deleteTermin.Parameters.AddWithValue("$datum", datum);
                deleteTermin.Parameters.AddWithValue("$vereinId", vereinId);
                deleteTermin.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public List<Benutzer> GetHelfer(int datum, long vereinId)
        {
            var result = new List<Benutzer>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT b.UserId, b.Name, b.VereinId, v.Name AS VereinName
                FROM HelferTermin h
                JOIN Benutzer b ON b.UserId = h.UserId
                LEFT JOIN Verein v ON v.Id = b.VereinId
                WHERE h.Bewirtungsdatum = $datum AND h.VereinId = $vereinId
                ORDER BY b.Name;";
            command.Parameters.AddWithValue("$datum", datum);
            command.Parameters.AddWithValue("$vereinId", vereinId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Benutzer
                {
                    UserId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    VereinId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                    VereinName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
            return result;
        }

        public void AddHelfer(int datum, long vereinId, long userId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO HelferTermin (VereinId, Bewirtungsdatum, UserId)
                VALUES ($vereinId, $datum, $userId);";
            command.Parameters.AddWithValue("$vereinId", vereinId);
            command.Parameters.AddWithValue("$datum", datum);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public void RemoveHelfer(int datum, long vereinId, long userId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM HelferTermin
                WHERE Bewirtungsdatum = $datum AND VereinId = $vereinId AND UserId = $userId;";
            command.Parameters.AddWithValue("$datum", datum);
            command.Parameters.AddWithValue("$vereinId", vereinId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }
    }
}

using Npgsql;
using NpgsqlTypes;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Word-Vorlagen (Tabelle wordvorlage).
    /// Es wird nur die eine Preislisten-Vorlage verwaltet (erster/einziger Datensatz).
    /// </summary>
    public class WordVorlageRepository
    {
        private readonly Database _database;

        public WordVorlageRepository(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Liefert die gespeicherte Vorlage oder null, wenn keine vorhanden ist.
        /// </summary>
        public WordVorlage? Get()
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, dateiname, komplett, bewirtungsdatum, inhalt FROM wordvorlage WHERE COALESCE(komplett, false) = true ORDER BY id LIMIT 1;";
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            byte[] inhalt = Array.Empty<byte>();
            if (!reader.IsDBNull(4))
            {
                inhalt = (byte[])reader.GetValue(4);
            }

            return new WordVorlage
            {
                Id = reader.GetInt32(0),
                Dateiname = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Komplett = !reader.IsDBNull(2) && reader.GetBoolean(2),
                Bewirtungsdatum = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                Inhalt = inhalt
            };
        }

        /// <summary>
        /// Speichert die Vorlage. Vorhandene Vorlage wird aktualisiert, sonst eingefügt.
        /// </summary>
        public void Save(string dateiname, byte[] inhalt)
        {
            using var connection = _database.CreateConnection();

            long? id = null;
            using (var select = connection.CreateCommand())
            {
                select.CommandText = "SELECT id FROM wordvorlage WHERE COALESCE(komplett, false) = false ORDER BY id LIMIT 1;";
                var result = select.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    id = Convert.ToInt64(result);
                }
            }

            using var command = connection.CreateCommand();
            if (id.HasValue)
            {
                command.CommandText = @"
                    UPDATE wordvorlage
                    SET inhalt = @inhalt, dateiname = @dateiname, komplett = true, vorlage = true, bewirtungsdatum = NULL
                    WHERE id = @id;";
                command.Parameters.AddWithValue("id", id.Value);
            }
            else
            {
                command.CommandText = @"
                    INSERT INTO wordvorlage (inhalt, dateiname, komplett, vorlage, bewirtungsdatum)
                    VALUES (@inhalt, @dateiname, true, true, null);";
            }

            command.Parameters.Add("inhalt", NpgsqlDbType.Bytea).Value = inhalt;
            command.Parameters.AddWithValue("dateiname", (object?)dateiname ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Speichert eine fertig generierte Preisliste als neuen Datensatz mit komplett = true.
        /// </summary>
        public void KompletteSpeichern(string dateiname, byte[] inhalt, DateTime bewirtungsdatum)
        {
            using var connection = _database.CreateConnection();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO wordvorlage (inhalt, dateiname, komplett, vorlage, bewirtungsdatum)
                VALUES (@inhalt, @dateiname, true, false, @bewirtungsdatum);";
            command.Parameters.Add("inhalt", NpgsqlDbType.Bytea).Value = inhalt;
            command.Parameters.AddWithValue("dateiname", (object?)dateiname ?? DBNull.Value);
            command.Parameters.AddWithValue("bewirtungsdatum", (object?)bewirtungsdatum ?? DBNull.Value);
            command.ExecuteNonQuery();
        }
    }
}

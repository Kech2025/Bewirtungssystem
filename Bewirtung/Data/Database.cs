using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Zentrale Hilfsklasse für den Zugriff auf die bestehende SQLite-Datenbank
    /// im Verzeichnis \data\Bewirtung.sqlite3.
    /// </summary>
    public static class Database
    {
        private const string RelativeDbPath = @"data\Bewirtung.sqlite3";

        private static readonly Lazy<string> _databasePath = new(ResolveDatabasePath);

        public static string DatabasePath => _databasePath.Value;

        public static string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                Mode = SqliteOpenMode.ReadWrite,
                DefaultTimeout = 10
            }.ToString();

        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Stellt sicher, dass nachträglich hinzugefügte Spalten in der bestehenden
        /// Datenbank vorhanden sind (einfache Schema-Migration ohne separate Tooling).
        /// </summary>
        public static void EnsureSchema()
        {
            using var connection = CreateConnection();
            EnsureColumn(connection, "GlobalePreisliste", "Zusatztext", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(connection, "BewirtungsPosition", "Besonderheit", "TEXT NOT NULL DEFAULT ''");
        }

        private static void EnsureColumn(SqliteConnection connection, string table, string column, string definition)
        {
            bool exists = false;
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = $"PRAGMA table_info({table});";
                using var reader = pragma.ExecuteReader();
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (exists)
            {
                return;
            }

            using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";
            alter.ExecuteNonQuery();
        }

        /// <summary>
        /// Sucht die vorhandene Datenbankdatei ausgehend vom Ausführungsverzeichnis
        /// nach oben, da \data neben dem Projektordner liegt.
        /// </summary>
        private static string ResolveDatabasePath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, RelativeDbPath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
                directory = directory.Parent;
            }

            return Path.Combine(AppContext.BaseDirectory, RelativeDbPath);
        }
    }
}

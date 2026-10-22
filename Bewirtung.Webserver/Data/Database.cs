using Microsoft.Data.Sqlite;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Zentrale Hilfsklasse für den Zugriff auf die bestehende SQLite-Datenbank
    /// im Verzeichnis \data\Bewirtung.sqlite3 (analog zur Desktop-Anwendung).
    /// </summary>
    public class Database
    {
        private const string RelativeDbPath = @"data\Bewirtung.sqlite3";

        private readonly string _databasePath;

        public Database(IConfiguration configuration)
        {
            var configured = configuration["Database:Path"];
            _databasePath = string.IsNullOrWhiteSpace(configured)
                ? ResolveDatabasePath()
                : configured;
        }

        public string DatabasePath => _databasePath;

        public string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWrite,
                DefaultTimeout = 10
            }.ToString();

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
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

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, RelativeDbPath));
        }
    }
}

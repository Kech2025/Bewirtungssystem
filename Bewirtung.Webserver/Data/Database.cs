using Npgsql;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Zentrale Hilfsklasse für den Zugriff auf die gemeinsame PostgreSQL-Datenbank.
    /// Der Connection-String wird aus der Konfiguration gelesen
    /// (ConnectionStrings:Postgres oder Database:ConnectionString).
    /// </summary>
    public class Database
    {
        private const string DefaultConnectionString =
            "Host=localhost;Port=5432;Database=bewirtung;Username=postgres;Password=postgres";

        private readonly string _connectionString;

        public Database(IConfiguration configuration)
        {
            var configured = configuration.GetConnectionString("Postgres")
                ?? configuration["Database:ConnectionString"];
            _connectionString = string.IsNullOrWhiteSpace(configured)
                ? DefaultConnectionString
                : configured;
        }

        public string ConnectionString => _connectionString;

        public NpgsqlConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
    }
}

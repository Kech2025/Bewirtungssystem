using Npgsql;

namespace Bewirtung.Fiskal
{
    /// <summary>
    /// Leichte, projektunabhängige Verbindungsquelle für die Fiskal-Bibliothek.
    /// Kapselt den PostgreSQL-Connection-String, damit sowohl der Webserver
    /// als auch die API das <see cref="FiskalRepository"/> nutzen können, ohne
    /// einen projekteigenen Datenbanktyp zu teilen.
    /// </summary>
    public class FiskalDatabase
    {
        private readonly string _connectionString;

        public FiskalDatabase(string connectionString)
        {
            _connectionString = connectionString;
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

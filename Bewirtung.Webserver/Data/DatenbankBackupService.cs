using System.Diagnostics;
using Npgsql;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Erstellt ein Backup der PostgreSQL-Datenbank per pg_dump und legt es
    /// als komprimierte Datei (Custom-Format) im Zielverzeichnis ab.
    /// Der Dateiname enthält Jahr, Monat und Tag (sowie Uhrzeit).
    /// Voraussetzung: pg_dump ist im Container verfügbar (postgresql-client).
    /// </summary>
    public class DatenbankBackupService
    {
        private readonly Database _database;

        public DatenbankBackupService(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Führt pg_dump aus und speichert das Backup im angegebenen Zielverzeichnis.
        /// Liefert den vollständigen Pfad der geschriebenen Datei zurück.
        /// </summary>
        public async Task<string> ErstelleBackupAsync(string zielVerzeichnis, string dateiPraefix = "backup", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(zielVerzeichnis))
            {
                throw new ArgumentException("Es ist kein Zielverzeichnis konfiguriert.", nameof(zielVerzeichnis));
            }

            if (!Directory.Exists(zielVerzeichnis))
            {
                throw new DirectoryNotFoundException(
                    $"Das Zielverzeichnis \"{zielVerzeichnis}\" wurde nicht gefunden. " +
                    "Ist der USB-Stick eingesteckt und korrekt eingehängt?");
            }

            var csb = new NpgsqlConnectionStringBuilder(_database.ConnectionString);
            var datenbank = string.IsNullOrWhiteSpace(csb.Database) ? "bewirtung" : csb.Database;
            var host = string.IsNullOrWhiteSpace(csb.Host) ? "localhost" : csb.Host;
            var port = csb.Port == 0 ? 5432 : csb.Port;
            var benutzer = string.IsNullOrWhiteSpace(csb.Username) ? "postgres" : csb.Username;

            var praefix = string.IsNullOrWhiteSpace(dateiPraefix) ? "backup" : dateiPraefix;
            var dateiname = $"{praefix}_{datenbank}_{DateTime.Now:yyyy-MM-dd_HHmmss}.dump";
            var zielPfad = Path.Combine(zielVerzeichnis, dateiname);

            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                RedirectStandardError = true,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("--host");
            startInfo.ArgumentList.Add(host);
            startInfo.ArgumentList.Add("--port");
            startInfo.ArgumentList.Add(port.ToString());
            startInfo.ArgumentList.Add("--username");
            startInfo.ArgumentList.Add(benutzer);
            startInfo.ArgumentList.Add("--format");
            startInfo.ArgumentList.Add("custom");
            startInfo.ArgumentList.Add("--no-owner");
            startInfo.ArgumentList.Add("--no-privileges");
            startInfo.ArgumentList.Add("--file");
            startInfo.ArgumentList.Add(zielPfad);
            startInfo.ArgumentList.Add(datenbank);

            // Passwort über Umgebungsvariable an pg_dump übergeben.
            if (!string.IsNullOrEmpty(csb.Password))
            {
                startInfo.Environment["PGPASSWORD"] = csb.Password;
            }

            using var prozess = new Process { StartInfo = startInfo };

            try
            {
                prozess.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "pg_dump konnte nicht gestartet werden. Ist der PostgreSQL-Client im Container installiert?", ex);
            }

            var fehlerAusgabe = await prozess.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await prozess.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (prozess.ExitCode != 0)
            {
                // Unvollständige Datei entfernen
                try { if (File.Exists(zielPfad)) File.Delete(zielPfad); } catch { /* ignorieren */ }

                throw new InvalidOperationException(
                    $"pg_dump ist mit Code {prozess.ExitCode} fehlgeschlagen: {fehlerAusgabe}");
            }

            return zielPfad;
        }

        /// <summary>
        /// Listet die im Zielverzeichnis vorhandenen Backup-Dateien (*.dump),
        /// neueste zuerst. Liefert nur die Dateinamen (ohne Pfad).
        /// </summary>
        public IReadOnlyList<string> ListeBackups(string zielVerzeichnis)
        {
            if (string.IsNullOrWhiteSpace(zielVerzeichnis) || !Directory.Exists(zielVerzeichnis))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(zielVerzeichnis, "*.dump")
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Select(f => f.Name)
                .ToList();
        }

        /// <summary>
        /// Schreibt alle gepufferten Dateisystem-Daten auf den Datenträger (sync),
        /// damit der USB-Stick anschließend sicher abgezogen werden kann.
        /// Ein echtes Aushängen (umount) ist aus dem Container heraus nicht möglich,
        /// da der Stick per hostPath vom Node eingebunden ist.
        /// </summary>
        public async Task UsbSynchronisierenAsync(CancellationToken cancellationToken = default)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sync",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var prozess = new Process { StartInfo = startInfo };

            try
            {
                prozess.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Der Befehl 'sync' konnte nicht gestartet werden.", ex);
            }

            var fehlerAusgabe = await prozess.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await prozess.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (prozess.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"'sync' ist mit Code {prozess.ExitCode} fehlgeschlagen: {fehlerAusgabe}");
            }
        }

        /// <summary>
        /// Stellt die Datenbank aus der angegebenen Backup-Datei wieder her (pg_restore).
        /// Bestehende Objekte werden dabei überschrieben (--clean --if-exists).
        /// Achtung: Vorhandene Daten in der Zieldatenbank gehen verloren.
        /// </summary>
        public async Task RestoreBackupAsync(string zielVerzeichnis, string dateiname, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(zielVerzeichnis))
            {
                throw new ArgumentException("Es ist kein Zielverzeichnis konfiguriert.", nameof(zielVerzeichnis));
            }

            if (string.IsNullOrWhiteSpace(dateiname))
            {
                throw new ArgumentException("Es wurde keine Backup-Datei ausgewählt.", nameof(dateiname));
            }

            // Nur den Dateinamen zulassen, keine Pfadanteile (Schutz vor Traversal).
            var sichererName = Path.GetFileName(dateiname);
            var quellPfad = Path.Combine(zielVerzeichnis, sichererName);

            if (!File.Exists(quellPfad))
            {
                throw new FileNotFoundException($"Die Backup-Datei \"{sichererName}\" wurde nicht gefunden.", quellPfad);
            }

            var csb = new NpgsqlConnectionStringBuilder(_database.ConnectionString);
            var datenbank = string.IsNullOrWhiteSpace(csb.Database) ? "bewirtung" : csb.Database;
            var host = string.IsNullOrWhiteSpace(csb.Host) ? "localhost" : csb.Host;
            var port = csb.Port == 0 ? 5432 : csb.Port;
            var benutzer = string.IsNullOrWhiteSpace(csb.Username) ? "postgres" : csb.Username;

            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_restore",
                RedirectStandardError = true,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("--host");
            startInfo.ArgumentList.Add(host);
            startInfo.ArgumentList.Add("--port");
            startInfo.ArgumentList.Add(port.ToString());
            startInfo.ArgumentList.Add("--username");
            startInfo.ArgumentList.Add(benutzer);
            startInfo.ArgumentList.Add("--dbname");
            startInfo.ArgumentList.Add(datenbank);
            startInfo.ArgumentList.Add("--clean");
            startInfo.ArgumentList.Add("--if-exists");
            startInfo.ArgumentList.Add("--no-owner");
            startInfo.ArgumentList.Add("--no-privileges");
            startInfo.ArgumentList.Add(quellPfad);

            if (!string.IsNullOrEmpty(csb.Password))
            {
                startInfo.Environment["PGPASSWORD"] = csb.Password;
            }

            // Aktive Verbindungen zur Zieldatenbank trennen, damit --clean nicht
            // an "database is being accessed by other users" / "object in use" scheitert.
            await TrenneAktiveVerbindungenAsync(datenbank, cancellationToken).ConfigureAwait(false);

            using var prozess = new Process { StartInfo = startInfo };

            try
            {
                prozess.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "pg_restore konnte nicht gestartet werden. Ist der PostgreSQL-Client im Container installiert?", ex);
            }

            var fehlerAusgabe = await prozess.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await prozess.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (prozess.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"pg_restore ist mit Code {prozess.ExitCode} fehlgeschlagen: {fehlerAusgabe}");
            }
        }

        /// <summary>
        /// Trennt alle anderen aktiven Verbindungen zur angegebenen Datenbank.
        /// Die eigene Verbindung (dieser Befehl) bleibt bestehen.
        /// </summary>
        private async Task TrenneAktiveVerbindungenAsync(string datenbank, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @db
                  AND pid <> pg_backend_pid();";

            try
            {
                await using var verbindung = await _database.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using var befehl = new NpgsqlCommand(sql, verbindung);
                befehl.Parameters.AddWithValue("db", datenbank);
                await befehl.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Wenn das Trennen fehlschlägt, versucht pg_restore es trotzdem.
                // Ein evtl. Fehler wird dort klar gemeldet.
            }
        }
    }
}

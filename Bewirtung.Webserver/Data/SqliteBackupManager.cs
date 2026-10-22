using Microsoft.Data.Sqlite;

namespace Bewirtung.Webserver.Data
{
    public static class SqliteBackupManager
    {
        public static void ExecuteBackup(string sourceDbPathAndDbName, string backupDirectory, int maxBackupsToKeep = 7)
        {
            try
            {
                // 1. Ordner für Backups erstellen, falls er nicht existiert
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                // 2. Eindeutigen Dateinamen mit Zeitstempel generieren
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"backup_{timestamp}.Sqlite3";
                string fullBackupPath = Path.Combine(backupDirectory, backupFileName);

                // 3. Sicheres Online-Backup über Microsoft.Data.Sqlite ausführen
                using (var sourceConnection = new SqliteConnection($"Data Source={sourceDbPathAndDbName}"))
                using (var backupConnection = new SqliteConnection($"Data Source={fullBackupPath}"))
                {
                    sourceConnection.Open();
                    backupConnection.Open();

                    // Dieser interne Befehl führt das Hot-Backup im laufenden Betrieb aus
                    sourceConnection.BackupDatabase(backupConnection);
                }

                Console.WriteLine($"[Backup] Erfolgreich erstellt: {backupFileName}");

                // 4. Automatische Bereinigung: Alte Backups löschen
                CleanOldBackups(backupDirectory, maxBackupsToKeep);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backup-Fehler] Das Backup ist fehlgeschlagen: {ex.Message}");
            }
        }
        private static void CleanOldBackups(string backupDirectory, int maxBackupsToKeep)
        {
            var directoryInfo = new DirectoryInfo(backupDirectory);

            // Holt alle Backup-Dateien, sortiert sie nach dem Erstellungsdatum (älteste zuerst)
            var backupFiles = directoryInfo.GetFiles("backup_*.Sqlite3")
                                          .OrderBy(f => f.CreationTime)
                                          .ToList();

            // Wenn mehr Backups existieren als erlaubt, werden die ältesten gelöscht
            if (backupFiles.Count > maxBackupsToKeep)
            {
                int filesToDeleteCount = backupFiles.Count - maxBackupsToKeep;

                for (int i = 0; i < filesToDeleteCount; i++)
                {
                    try
                    {
                        backupFiles[i].Delete();
                        Console.WriteLine($"[Backup-Bereinigung] Altes Backup gelöscht: {backupFiles[i].Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Backup-Bereinigung] Fehler beim Löschen von {backupFiles[i].Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}

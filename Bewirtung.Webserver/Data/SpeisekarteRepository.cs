using Npgsql;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für die aktuelle Speisekarte eines Termins.
    /// Kombiniert GlobalePreisliste/GlobalePreislisteZusatz mit den zeitraumbezogenen
    /// Preisen aus GlobalerPreis und den terminbezogenen Preisen aus PreislisteTermin.
    /// </summary>
    public class SpeisekarteRepository
    {
        private readonly Database _database;

        public SpeisekarteRepository(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Liefert alle Positionen (inkl. Zusatz-Varianten) der globalen Speisekarte.
        /// GlobalPreis wird aus GlobalerPreis der zum Datum gültigen Periode ermittelt.
        /// Bereits für den Termin gespeicherte Auswahl/Preise werden übernommen.
        /// </summary>
        public List<SpeisekartenZeile> GetSpeisekarte(int bewirtungsdatum, long vereinId)
        {
            var zeilen = new List<SpeisekartenZeile>();
            using var connection = _database.CreateConnection();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT p.Id, p.Name, p.Preiskategorie, z.Id AS ZusatzId, z.Bezeichnung,
                           COALESCE(gp.Preis,0)+COALESCE(gp2.Preis,0) AS GlobalPreis, p.Druck AS DruckParent, z.Druck AS DruckClient
                    FROM GlobalePreisliste p
                    LEFT JOIN GlobalePreislisteZusatz z ON z.SpeisekarteId = p.Id
                    LEFT JOIN GlobalerPreis gp ON gp.GlobalePreislisteId = p.Id AND gp.ZusatzId = 0 AND gp.Preis > 0
                        AND gp.GueltigAb <= @datum AND (gp.GueltigBis IS NULL OR gp.GueltigBis >= @datum)
                    LEFT JOIN GlobalerPreis gp2 ON gp2.GlobalePreislisteId = p.Id AND gp2.ZusatzId = z.Id
                        AND gp2.GueltigAb <= @datum AND (gp2.GueltigBis IS NULL OR gp2.GueltigBis >= @datum)
                    ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";
                command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(bewirtungsdatum));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var kategorie = reader.IsDBNull(2) ? 0L : reader.GetInt32(2);
                    zeilen.Add(new SpeisekartenZeile
                    {
                        PreislisteId = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        KategorieText = kategorie == 1 ? "Getränke" : "Speisen",
                        ZusatzId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        ZusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        GlobalPreis = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                        DruckParent = (!reader.IsDBNull(6) && reader.GetBoolean(6)) ? 1 : 0,
                        DruckClient = (!reader.IsDBNull(7) && reader.GetBoolean(7)) ? 1 : 0
                    });
                }
            }

            var terminPreise = GetPreislisteTermin(connection, bewirtungsdatum, vereinId);

            foreach (var zeile in zeilen)
            {
                var key = (zeile.PreislisteId, zeile.ZusatzId);

                // Vorbelegung: Terminpreis = globaler Preis, sofern kein eigener Terminpreis hinterlegt ist.
                zeile.TerminPreis = zeile.GlobalPreis ?? 0m;

                if (terminPreise.TryGetValue(key, out var tp))
                {
                    zeile.Ausgewaehlt = true;
                    zeile.TerminPreis = tp > 0 ? tp : zeile.GlobalPreis ?? 0m;
                }
            }

            return zeilen;
        }

        private static Dictionary<(long, long), decimal> GetPreislisteTermin(NpgsqlConnection connection, int datum, long vereinId)
        {
            var result = new Dictionary<(long, long), decimal>();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PreislisteId, PreislisteZusatzId, Preis
                FROM PreislisteTermin
                WHERE Bewirtungsdatum = @datum AND VereinId = @vereinId;";
            command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
            command.Parameters.AddWithValue("vereinId", vereinId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = ((long)reader.GetInt32(0), (long)reader.GetInt32(1));
                result[key] = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
            }
            return result;
        }

        /// <summary>
        /// Speichert die Auswahl für einen Termin: Löscht die bisherigen Einträge und
        /// schreibt die aktuell ausgewählten Positionen mit ihrem (überschriebenen) Preis.
        /// </summary>
        public void Speichern(int bewirtungsdatum, long vereinId, IEnumerable<SpeisekartenZeile> zeilen)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.CommandText = @"
                    DELETE FROM PreislisteTermin
                    WHERE Bewirtungsdatum = @datum AND VereinId = @vereinId;";
                delete.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(bewirtungsdatum));
                delete.Parameters.AddWithValue("vereinId", vereinId);
                delete.ExecuteNonQuery();
            }

            foreach (var zeile in zeilen)
            {
                if (!zeile.Ausgewaehlt)
                {
                    continue;
                }

                using var insert = connection.CreateCommand();
                insert.CommandText = @"
                    INSERT INTO PreislisteTermin
                        (PreislisteId, PreislisteZusatzId, VereinId, Bewirtungsdatum, Preis)
                    VALUES (@preislisteId, @zusatzId, @vereinId, @datum, @preis);";
                insert.Parameters.AddWithValue("preislisteId", zeile.PreislisteId);
                insert.Parameters.AddWithValue("zusatzId", zeile.ZusatzId);
                insert.Parameters.AddWithValue("vereinId", vereinId);
                insert.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(bewirtungsdatum));
                insert.Parameters.AddWithValue("preis", zeile.TerminPreis);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}

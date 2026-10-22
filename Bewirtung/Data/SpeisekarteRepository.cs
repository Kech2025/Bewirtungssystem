using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.Sqlite;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für die aktuelle Speisekarte eines Termins.
    /// Kombiniert GlobalePreisliste/GlobalePreislisteZusatz mit den zeitraumbezogenen
    /// Preisen aus GlobalerPreis und den terminbezogenen Preisen aus PreislisteTermin.
    /// </summary>
    public class SpeisekarteRepository
    {
        public List<SpeisekartenZeile> GetFehlerAbrechnung(int bewirtungsdatum)
        {
            var zeilen = new List<SpeisekartenZeile>();
            using var connection = Database.CreateConnection();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT p.Id, p.Name, p.Preiskategorie, z.Id AS ZusatzId, z.Bezeichnung, COALESCE(gp.Preis,0)+COALESCE(gp2.Preis,0)  AS GlobalPreis, p.Druck AS DruckParent, z.Druck AS DruckClient  
                    FROM GlobalePreisliste p
                    LEFT JOIN GlobalePreislisteZusatz z ON z.SpeisekarteId = p.Id --AND z.Druck = 1
                    LEFT JOIN GlobalerPreis gp ON gp.GlobalePreislisteId = p.Id AND gp.ZusatzId = 0 AND gp.Preis > 0 
                        AND gp.GueltigAb <= 20260928 AND (gp.GueltigBis = 0 OR gp.GueltigBis >= 20260928)
                    LEFT JOIN GlobalerPreis gp2 ON gp2.GlobalePreislisteId = p.Id and gp2.ZusatzId = z.Id
                        AND gp2.GueltigAb <= 20260928 AND (gp2.GueltigBis = 0 OR gp2.GueltigBis >= 20260928)
                    --WHERE p.Druck = 1
                    ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";

                command.Parameters.AddWithValue("$datum", bewirtungsdatum);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var kategorie = reader.IsDBNull(2)
                        ? Preiskategorie.Speisen
                        : (Preiskategorie)reader.GetInt64(2);
                    zeilen.Add(new SpeisekartenZeile
                    {
                        PreislisteId = reader.GetInt64(0),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        KategorieText = kategorie == Preiskategorie.Getraenke ? "Getränke" : "Speisen",
                        ZusatzId = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                        ZusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        GlobalPreis = reader.IsDBNull(5) ? null : (decimal?)reader.GetDouble(5),
                        DruckParent = reader.IsDBNull(6) ? 0 : (int)reader.GetInt32(6),
                        DruckClient = reader.IsDBNull(7) ? 0 : (int)reader.GetInt32(7)
                    });
                }
            }
            return zeilen;
        }
        /// <summary>
        /// Liefert alle Positionen (inkl. Zusatz-Varianten) der globalen Speisekarte.
        /// GlobalPreis wird aus GlobalerPreis der zum Datum gültigen Periode ermittelt.
        /// Bereits für den Termin gespeicherte Auswahl/Preise werden übernommen.
        /// </summary>
        public List<SpeisekartenZeile> GetSpeisekarte(int bewirtungsdatum, long vereinId)
        {
            var zeilen = new List<SpeisekartenZeile>();
            using var connection = Database.CreateConnection();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT p.Id, p.Name, p.Preiskategorie, z.Id AS ZusatzId, z.Bezeichnung, COALESCE(gp.Preis,0)+COALESCE(gp2.Preis,0)  AS GlobalPreis, p.Druck AS DruckParent, z.Druck AS DruckClient  
                    FROM GlobalePreisliste p
                    LEFT JOIN GlobalePreislisteZusatz z ON z.SpeisekarteId = p.Id --AND z.Druck = 1
                    LEFT JOIN GlobalerPreis gp ON gp.GlobalePreislisteId = p.Id AND gp.ZusatzId = 0 AND gp.Preis > 0 
                        AND gp.GueltigAb <= 20260928 AND (gp.GueltigBis = 0 OR gp.GueltigBis >= 20260928)
                    LEFT JOIN GlobalerPreis gp2 ON gp2.GlobalePreislisteId = p.Id and gp2.ZusatzId = z.Id
                        AND gp2.GueltigAb <= 20260928 AND (gp2.GueltigBis = 0 OR gp2.GueltigBis >= 20260928)
                    --WHERE p.Druck = 1
                    ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";
               
                command.Parameters.AddWithValue("$datum", bewirtungsdatum);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var kategorie = reader.IsDBNull(2)
                        ? Preiskategorie.Speisen
                        : (Preiskategorie)reader.GetInt64(2);
                    zeilen.Add(new SpeisekartenZeile
                    {
                        PreislisteId = reader.GetInt64(0),
                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        KategorieText = kategorie == Preiskategorie.Getraenke ? "Getränke" : "Speisen",
                        ZusatzId = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                        ZusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        GlobalPreis = reader.IsDBNull(5) ? null : (decimal?)reader.GetDouble(5),
                        DruckParent = reader.IsDBNull(6) ? 0 : (int)reader.GetInt32(6),
                        DruckClient = reader.IsDBNull(7) ? 0 : (int)reader.GetInt32(7)
                    });
                }
            }

            var globalPreise = GetGlobalePreise(connection, bewirtungsdatum);
            var terminPreise = GetPreislisteTermin(connection, bewirtungsdatum, vereinId);

            foreach (var zeile in zeilen)
            {
                var key = (zeile.PreislisteId, zeile.ZusatzId);

                // Vorbelegung: Terminpreis = globaler Preis, sofern kein eigener Terminpreis hinterlegt ist.
                zeile.TerminPreis = zeile.GlobalPreis ?? 0m;

                if (terminPreise.TryGetValue(key, out var tp))
                {
                    zeile.Ausgewaehlt = true;
                    if (tp > 0)
                        zeile.TerminPreis = tp;
                    else
                        zeile.TerminPreis = zeile.GlobalPreis ?? 0m;
                }
            }

            return zeilen;
        }

        private static Dictionary<(long, long), decimal> GetGlobalePreise(SqliteConnection connection, int datum)
        {
            var result = new Dictionary<(long, long), decimal>();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT GlobalePreislisteId, ZusatzId, Preis
                FROM GlobalerPreis
                WHERE GueltigAb <= $datum AND GueltigBis >= $datum;";
            command.Parameters.AddWithValue("$datum", datum);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = (reader.GetInt64(0), reader.GetInt64(1));
                result[key] = reader.IsDBNull(2) ? 0m : (decimal)reader.GetDouble(2);
            }
            return result;
        }

        private static Dictionary<(long, long), decimal> GetPreislisteTermin(SqliteConnection connection, int datum, long vereinId)
        {
            var result = new Dictionary<(long, long), decimal>();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PreislisteId, PreislisteZusatzId, Preis
                FROM PreislisteTermin
                WHERE Bewirtungsdatum = $datum AND VereinId = $vereinId;";
            command.Parameters.AddWithValue("$datum", datum);
            command.Parameters.AddWithValue("$vereinId", vereinId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = (reader.GetInt64(0), reader.GetInt64(1));
                result[key] = reader.IsDBNull(2) ? 0m : (decimal)reader.GetDouble(2);
            }
            return result;
        }

        /// <summary>
        /// Speichert die Auswahl für einen Termin: Löscht die bisherigen Einträge und
        /// schreibt die aktuell ausgewählten Positionen mit ihrem (überschriebenen) Preis.
        /// </summary>
        public void Speichern(int bewirtungsdatum, long vereinId, IEnumerable<SpeisekartenZeile> zeilen)
        {
            using var connection = Database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.CommandText = @"
                    DELETE FROM PreislisteTermin
                    WHERE Bewirtungsdatum = $datum AND VereinId = $vereinId;";
                delete.Parameters.AddWithValue("$datum", bewirtungsdatum);
                delete.Parameters.AddWithValue("$vereinId", vereinId);
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
                    VALUES ($preislisteId, $zusatzId, $vereinId, $datum, $preis);";
                insert.Parameters.AddWithValue("$preislisteId", zeile.PreislisteId);
                insert.Parameters.AddWithValue("$zusatzId", zeile.ZusatzId);
                insert.Parameters.AddWithValue("$vereinId", vereinId);
                insert.Parameters.AddWithValue("$datum", bewirtungsdatum);
                insert.Parameters.AddWithValue("$preis", (double)zeile.TerminPreis);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}

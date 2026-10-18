using Microsoft.Data.Sqlite;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für die Tagesabrechnung (AbrechnungTagForm).
    /// Liefert gleichartige Bewirtungspositionen aggregiert zu einem Termin (Datum).
    /// </summary>
    public class AbrechnungRepository
    {
        /// <summary>
        /// Liefert alle aggregierten Abrechnungszeilen für das angegebene Datum (yyyyMMdd).
        /// Gleichartige Positionen (gleiche PreislisteId, PreislisteZusatzId und Besonderheit)
        /// werden je Benutzer und Tisch zusammengefasst; Menge und Gesamtpreis werden summiert.
        /// </summary>
        public List<AbrechnungZeile> GetForTermin(int bewirtungsDatum)
        {
            var result = new List<AbrechnungZeile>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT a.UserId,
                       COALESCE(u.Name, '') AS Benutzer,
                       bp.PreislisteId,
                       COALESCE(bp.PreislisteZusatzId, 0) AS PreislisteZusatzId,
                       COALESCE(p.Name, '') AS ArtikelName,
                       COALESCE(z.Bezeichnung, '') AS ZusatzText,
                       SUM(bp.Menge) AS MengeSumme,
                       COALESCE(bp.Besonderheit, '') AS Besonderheit,
                       SUM(bp.Preis * bp.Menge) AS GesamtpreisSumme,
                       a.TischNummer,
                       COALESCE(p.Preiskategorie, 0) AS Preiskategorie,
                       COALESCE(m.Mwst, 0) AS MwstSatz
                FROM BewirtungsPosition bp
                INNER JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
                LEFT JOIN Benutzer u ON u.UserId = a.UserId
                LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
                LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
                LEFT JOIN Mwst m ON m.Preiskategorie = p.Preiskategorie
                WHERE a.Bewirtungstermin = $datum
                GROUP BY a.UserId, a.TischNummer, bp.PreislisteId,
                         COALESCE(bp.PreislisteZusatzId, 0), COALESCE(bp.Besonderheit, '')
                ORDER BY a.TischNummer, Benutzer, ArtikelName;";
            command.Parameters.AddWithValue("$datum", bewirtungsDatum);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var artikelName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                var zusatzText = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                var gesamtpreis = reader.IsDBNull(8) ? 0m : (decimal)reader.GetDouble(8);
                var mwstSatz = reader.IsDBNull(11) ? 0m : (decimal)Convert.ToDouble(reader.GetValue(11));
                result.Add(new AbrechnungZeile
                {
                    UserId = reader.GetInt64(0),
                    Benutzer = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    PreislisteId = reader.GetInt64(2),
                    PreislisteZusatzId = reader.GetInt64(3),
                    Artikel = string.IsNullOrEmpty(zusatzText) ? artikelName : $"{artikelName} – {zusatzText}",
                    Menge = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6),
                    Besonderheit = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Gesamtpreis = gesamtpreis,
                    Tischnummer = (int)reader.GetInt64(9),
                    Preiskategorie = (int)reader.GetInt64(10),
                    Mwst = mwstSatz > 0m ? gesamtpreis - (gesamtpreis * 100m / (100m + mwstSatz)) : gesamtpreis
                });
            }
            return result;
        }

        /// <summary>
        /// Liefert die Summe der Gesamtpreise aus der Tabelle Bewirtungsauftrag
        /// für das angegebene Datum (yyyyMMdd), optional eingeschränkt auf einen
        /// Benutzer (userId) und/oder eine Tischnummer (tischnummer).
        /// </summary>
        public decimal GetAuftragSumme(int bewirtungsDatum, long? userId = null, int? tischnummer = null)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            var sql = @"
                SELECT COALESCE(SUM(a.Gesamtpreis), 0)
                FROM Bewirtungsauftrag a
                WHERE a.Bewirtungstermin = $datum";
            command.Parameters.AddWithValue("$datum", bewirtungsDatum);
            if (userId.HasValue)
            {
                sql += " AND a.UserId = $userId";
                command.Parameters.AddWithValue("$userId", userId.Value);
            }
            if (tischnummer.HasValue)
            {
                sql += " AND a.TischNummer = $tisch";
                command.Parameters.AddWithValue("$tisch", tischnummer.Value);
            }
            command.CommandText = sql + ";";

            var result = command.ExecuteScalar();
            return result is null || result is DBNull ? 0m : (decimal)Convert.ToDouble(result);
        }
    }
}

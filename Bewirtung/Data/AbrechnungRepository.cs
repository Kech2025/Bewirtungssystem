using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Bewirtung.Data
{
    /// <summary>
    /// Datenzugriff für die Tagesabrechnung (AbrechnungTagForm).
    /// Liefert gleichartige Bewirtungspositionen aggregiert zu einem Termin (Datum).
    /// </summary>
    public class AbrechnungRepository
    {
        public void DeleteFehlerAbrechnungZeile(FehlerAbrechnungZeile zeile)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM BewirtungsPosition
                WHERE BewirtungsauftragId = $auftragId";
            command.Parameters.AddWithValue("$auftragId", zeile.BewirtungsauftragId);
            command.ExecuteNonQuery();

            using var command2 = connection.CreateCommand();
            command2.CommandText = @"
                DELETE FROM Bewirtungsauftrag 
                WHERE AuftragId = $auftragId;";
            command2.Parameters.AddWithValue("$auftragId", zeile.BewirtungsauftragId);
            command2.ExecuteNonQuery();
        }

        public void UpdateFehlerAbrechnungZeile(FehlerAbrechnungZeile zeile)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            if(zeile.MengeGeliefert < zeile.MengeBezahlt)
            {
                zeile.MengeGeliefert = zeile.MengeBezahlt;
            }
            else
            {
                zeile.MengeBezahlt = zeile.MengeGeliefert;
            }
            command.CommandText = @"
                UPDATE BewirtungsPosition
                SET MengeGeliefert = $mengeGeliefert,
                    MengeBezahlt = $mengeBezahlt
                WHERE BewirtungsauftragId = $auftragId and Bewirtungsposition = $position";
            command.Parameters.AddWithValue("$auftragId", zeile.BewirtungsauftragId);
            command.Parameters.AddWithValue("$position", zeile.Bewirtungsposition);
            command.Parameters.AddWithValue("$mengeGeliefert", zeile.MengeGeliefert);
            command.Parameters.AddWithValue("$mengeBezahlt", zeile.MengeBezahlt);
            command.ExecuteNonQuery();
            using var command2 = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Bewirtungsauftrag
                SET Status = 1, StatusLieferung = 1
                WHERE AuftragId = $auftragId";
            command2.Parameters.AddWithValue("$auftragId", zeile.BewirtungsauftragId);
            command2.ExecuteNonQuery();

            using var command3 = connection.CreateCommand();
            command3.CommandText = @"
                DELETE FROM Bewirtungsauftrag b
                WHERE (SELECT COUNT(*) FROM BewirtungsPosition bp WHERE bp.BewirtungsauftragId = b.AuftragId) = 0";
            command3.ExecuteNonQuery();
        }
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
                       COALESCE(m.Mwst, 0) AS MwstSatz,
                       COALESCE(t.BereichId, 0) AS BereichId,
                       COALESCE(bt.Bezeichnung, '') AS BereichName
                FROM BewirtungsPosition bp
                INNER JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
                LEFT JOIN Benutzer u ON u.UserId = a.UserId
                LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
                LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
                LEFT JOIN Mwst m ON m.Preiskategorie = p.Preiskategorie
                LEFT JOIN Tisch t ON t.Tischnummer = a.TischNummer
                LEFT JOIN BereichTische bt ON bt.BereichId = t.BereichId
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
                    Mwst = mwstSatz > 0m ? gesamtpreis - (gesamtpreis * 100m / (100m + mwstSatz)) : gesamtpreis,
                    BereichId = reader.GetInt64(12),
                    BereichName = reader.IsDBNull(13) ? string.Empty : reader.GetString(13)
                });
            }
           
            return result;
        }
        public List<FehlerAbrechnungZeile> GetFehlerListe(int bewirtungsDatum)
        {
            var result = new List<FehlerAbrechnungZeile>();
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT u.Name AS UserName,
                       bp.Buchungszeitpunkt AS Bewirtungszeitpunkt,
                       COALESCE(a.TischNummer, 0) AS Tisch,
                       COALESCE(bt.Bezeichnung, '') AS Bereich,
                       SUM(bp.Preis * bp.Menge) AS Gesamtpreis,
                       bp.Menge,
                       bp.MengeGeliefert,
                       bp.MengeBezahlt,
                       bp.Besonderheit,
                       CONCAT(COALESCE(p.Name,''), ' - ', COALESCE(z.Bezeichnung, '')) AS ArtikelName,
                       bp.BewirtungsauftragId,
                       bp.Bewirtungsposition
                FROM BewirtungsPosition bp
                INNER JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
                LEFT JOIN Benutzer u ON u.UserId = a.UserId
                LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
                LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
                LEFT JOIN Tisch t ON t.Tischnummer = a.TischNummer
                LEFT JOIN BereichTische bt ON bt.BereichId = t.BereichId
                WHERE a.Bewirtungstermin = $datum AND (COALESCE(bp.mengegeliefert,0) = 0 OR COALESCE(bp.mengebezahlt,0) = 0 OR COALESCE(bp.Mengegeliefert,0) <> COALESCE(bp.Mengebezahlt,0))
                GROUP BY u.Name, bp.Buchungszeitpunkt, bp.Menge, bp.MengeGeliefert, bp.MengeBezahlt, 
                         COALESCE(a.TischNummer, 0), COALESCE(bt.Bezeichnung, '')
                ORDER BY u.Name, bp.Buchungszeitpunkt;";
            command.Parameters.AddWithValue("$datum", bewirtungsDatum);
            string format = "yyyy-MM-dd HH:mm:ss";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string datumsText = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                DateTime bewirtungszeitpunkt = DateTime.MinValue;
                DateTime.TryParseExact(datumsText, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out bewirtungszeitpunkt);
                decimal gesamtpreis = reader.IsDBNull(4) ? 0m : (decimal)reader.GetDouble(4);
                var artikelName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                result.Add(new FehlerAbrechnungZeile
                {
                    UserName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),                
                    Bewirtungszeitpunkt = bewirtungszeitpunkt,
                    Tisch = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Bereich = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Gesamtpreis = gesamtpreis,
                    Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetDouble(5),
                    MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6),
                    MengeBezahlt = reader.IsDBNull(7) ? 0m : (decimal)reader.GetDouble(7),
                    Besonderheit = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    ArtikelName = artikelName,
                    BewirtungsauftragId = reader.IsDBNull(10) ? 0 : reader.GetInt64(10),
                    Bewirtungsposition = reader.IsDBNull(11) ? 0 : reader.GetInt64(11)
                });
            }
            //Wenn ein Auftrag da ist aber keine Positionen, dann Fake-Position hinzufügen
            using var command2 = connection.CreateCommand();
            command2.CommandText = @"
                SELECT b.AuftragId, b.Gesamtpreis, b.Tischnummer, COALESCE(bt.Bezeichnung, '') AS BereichName, COALESCE(u.Name, '') AS Benutzer
                FROM BewirtungsAuftrag b 
                LEFT JOIN Benutzer u ON u.UserId = b.UserId
                LEFT JOIN Tisch t ON t.Tischnummer = b.TischNummer
                LEFT JOIN BereichTische bt ON bt.BereichId = t.BereichId
                WHERE b.Bewirtungstermin = $datum AND (SELECT COUNT(*) FROM BewirtungsPosition bp WHERE bp.BewirtungsauftragId = b.AuftragId) = 0;";
            command2.Parameters.AddWithValue("$datum", bewirtungsDatum);
            var reader2 = command2.ExecuteReader();
            while (reader2.Read())
            {
                result.Add(new FehlerAbrechnungZeile
                {
                    UserName = reader2.IsDBNull(4) ? string.Empty : reader2.GetString(4),
                    Bewirtungszeitpunkt = DateTime.MinValue,
                    Tisch = reader2.IsDBNull(2) ? string.Empty : reader2.GetString(2),
                    Bereich = reader2.IsDBNull(3) ? string.Empty : reader2.GetString(3),
                    Gesamtpreis = reader2.IsDBNull(1) ? 0m : (decimal)reader2.GetDouble(1),
                    Menge = 0,
                    Besonderheit = "Positionen fehlen",
                    ArtikelName = "Nicht vorhanden",
                    BewirtungsauftragId = reader2.IsDBNull(0) ? 0 : reader2.GetInt64(0),
                    Bewirtungsposition = 0,
                    MengeBezahlt = 0,
                    MengeGeliefert = 0
                });
            }
            return result;
        }

        /// <summary>
        /// Liefert die Summe der Gesamtpreise aus der Tabelle Bewirtungsauftrag
        /// für das angegebene Datum (yyyyMMdd), optional eingeschränkt auf einen
        /// Benutzer (userId) und/oder eine Tischnummer (tischnummer).
        /// </summary>
        public decimal GetTrinkgeldSumme(int bewirtungsDatum, long? userId = null, int? tischnummer = null)
        {
            using var connection = Database.CreateConnection();
            using var command = connection.CreateCommand();
            var sql = @"
                SELECT COALESCE(SUM(a.Bezahlt), 0) - COALESCE(SUM(a.Gesamtpreis), 0)
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

using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Datenzugriff für Küchenstation-Termin-Zuordnungen (Tabelle kuechenstationtermin).
    /// Die Spalte bewirtungstermin wird jeweils aus der Tabelle Termin anhand von
    /// Bewirtungsdatum und VereinId aufgelöst.
    /// </summary>
    public class KuechenstationTerminRepository
    {
        private readonly Database _database;

        public KuechenstationTerminRepository(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Liefert die einem Termin (Bewirtungsdatum + VereinId) zugeordneten Küchenstationen.
        /// </summary>
        public List<KuechenstationTermin> GetFuerTermin(int datum, long vereinId)
        {
            var result = new List<KuechenstationTermin>();
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT kst.kuechenstationid, k.name
                FROM kuechenstationtermin kst
                JOIN Termin t ON t.bewirtungsdatum = kst.bewirtungsdatum
                LEFT JOIN kuechenstation k ON k.id = kst.kuechenstationid
                WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId
                ORDER BY k.name;";
            command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
            command.Parameters.AddWithValue("vereinId", vereinId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new KuechenstationTermin
                {
                    Bewirtungsdatum = datum,
                    VereinId = vereinId,
                    KuechenstationId = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }
            return result;
        }

        public void Insert(int datum, long vereinId, int kuechenstationId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO kuechenstationtermin (bewirtungsdatum, kuechenstationid)
                SELECT t.bewirtungsdatum, @kuechenstationId
                FROM Termin t
                WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId;";
            command.Parameters.AddWithValue("kuechenstationId", kuechenstationId);
            command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
            command.Parameters.AddWithValue("vereinId", vereinId);
            command.ExecuteNonQuery();
        }

        public void Delete(int datum, long vereinId, int kuechenstationId)
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM kuechenstationtermin
                WHERE kuechenstationid = @kuechenstationId
                  AND bewirtungsdatum IN (
                      SELECT t.bewirtungsdatum FROM Termin t
                      WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId);";
            command.Parameters.AddWithValue("kuechenstationId", kuechenstationId);
            command.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
            command.Parameters.AddWithValue("vereinId", vereinId);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Ersetzt die Küchenstation-Zuordnungen eines Termins vollständig durch die
        /// übergebene Menge an Küchenstation-IDs.
        /// </summary>
        public void SetFuerTermin(int datum, long vereinId, IEnumerable<int> kuechenstationIds)
        {
            using var connection = _database.CreateConnection();
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.Transaction = transaction;
                delete.CommandText = @"
                    DELETE FROM kuechenstationtermin
                    WHERE bewirtungsdatum IN (
                        SELECT t.bewirtungsdatum FROM Termin t
                        WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId);";
                delete.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
                delete.Parameters.AddWithValue("vereinId", vereinId);
                delete.ExecuteNonQuery();
            }

            foreach (var kuechenstationId in kuechenstationIds.Distinct())
            {
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = @"
                    INSERT INTO kuechenstationtermin (bewirtungsdatum, kuechenstationid)
                    SELECT t.bewirtungsdatum, @kuechenstationId
                    FROM Termin t
                    WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId;";
                insert.Parameters.AddWithValue("kuechenstationId", kuechenstationId);
                insert.Parameters.AddWithValue("datum", Termin.FromYyyyMmDd(datum));
                insert.Parameters.AddWithValue("vereinId", vereinId);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}

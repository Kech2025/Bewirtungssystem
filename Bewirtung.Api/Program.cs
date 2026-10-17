using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Pfad zur SQLite-Datenbank (kann in appsettings.json oder per Umgebungsvariable
// "Database__Path" gesetzt werden, z. B. ein UNC-Pfad \\SERVER\share\data\Bewirtung.sqlite3).
string ResolveDatabasePath()
{
    var configured = app.Configuration["Database:Path"];
    if (string.IsNullOrWhiteSpace(configured))
    {
        configured = @"data\Bewirtung.sqlite3";
    }

    if (Path.IsPathRooted(configured))
    {
        return configured;
    }

    // Relativen Pfad ausgehend vom Ausführungsverzeichnis nach oben suchen,
    // da der \data-Ordner neben dem Projektordner liegt (nicht im bin-Ordner).
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory != null)
    {
        var candidate = Path.GetFullPath(Path.Combine(directory.FullName, configured));
        if (File.Exists(candidate))
        {
            return candidate;
        }
        directory = directory.Parent;
    }

    return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
}

var dbPath = ResolveDatabasePath();

string ConnectionString() =>
    new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadOnly
    }.ToString();

// GET /api/tische  -> [{ Tischnummer, Bereich }, ...]
app.MapGet("/api/tische", () =>
{
    var tische = new List<object>();
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT t.Tischnummer, b.Bezeichnung, COALESCE(t.SofortLieferung, 0)
        FROM Tisch t
        LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
        ORDER BY t.Tischnummer ASC;";
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        if (!reader.IsDBNull(0))
        {
            tische.Add(new
            {
                Tischnummer = (int)reader.GetInt64(0),
                Bereich = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                SofortLieferung = reader.IsDBNull(2) ? 0 : (int)reader.GetInt64(2)
            });
        }
    }
    return Results.Ok(tische);
});

// GET /api/benutzer  ->  Liste der Benutzer (UserId + Name) für die Anmeldung.
app.MapGet("/api/benutzer", () =>
{
    var benutzer = new List<object>();
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT UserId, Name FROM Benutzer ORDER BY Name;";
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        benutzer.Add(new
        {
            UserId = reader.GetInt64(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
        });
    }
    return Results.Ok(benutzer);
});

// GET /api/benutzer/termin?datum=...&vereinId=...  -> Helfer, die über HelferTermin dem Termin zugeordnet sind.
app.MapGet("/api/benutzer/termin", (int datum, long vereinId) =>
{
    var benutzer = new List<object>();
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT b.UserId, b.Name
        FROM HelferTermin h
        JOIN Benutzer b ON b.UserId = h.UserId
        WHERE h.Bewirtungsdatum = $datum AND h.VereinId = $vereinId
        ORDER BY b.Name;";
    command.Parameters.AddWithValue("$datum", datum);
    command.Parameters.AddWithValue("$vereinId", vereinId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        benutzer.Add(new
        {
            UserId = reader.GetInt64(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
        });
    }
    return Results.Ok(benutzer);
});
app.MapGet("/api/termine", () =>
{
    var termine = new List<object>();
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT t.BewirtungsDatum, t.VereinId, v.Name
        FROM Termin t
        LEFT JOIN Verein v ON v.Id = t.VereinId
        ORDER BY t.BewirtungsDatum DESC, v.Name;";
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        termine.Add(new
        {
            BewirtungsDatum = (int)reader.GetInt64(0),
            VereinId = reader.GetInt64(1),
            VereinName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
        });
    }
    return Results.Ok(termine);
});

// GET /api/speisekarte?datum=YYYYMMDD&vereinId=123
//   -> Für den Termin ausgewählte Preislisten-Positionen (Produkt + Preis).
app.MapGet("/api/speisekarte", (int datum, long vereinId) =>
{
    var positionen = new List<object>();
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT p.Id AS PreislisteId,
               z.Id AS ZusatzId,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               p.Preiskategorie,
               pt.Preis + COALESCE(hp.Preis, 0) AS Preis
        FROM PreislisteTermin pt
        JOIN GlobalePreisliste p ON p.Id = pt.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = pt.PreislisteZusatzId AND z.SpeisekarteId = p.Id
        LEFT JOIN PreislisteTermin hp ON hp.PreislisteId = pt.PreislisteId
            AND hp.PreislisteZusatzId = 0
            AND hp.VereinId = pt.VereinId
            AND hp.Bewirtungsdatum = pt.Bewirtungsdatum
            AND pt.PreislisteZusatzId <> 0
        WHERE pt.Bewirtungsdatum = $datum AND pt.VereinId = $vereinId
        ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("$datum", datum);
    command.Parameters.AddWithValue("$vereinId", vereinId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        var kategorie = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);
        var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
        var zusatzText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        positionen.Add(new
        {
            PreislisteId = reader.GetInt64(0),
            ZusatzId = reader.IsDBNull(1) ? 0L : reader.GetInt64(1),
            Name = name,
            ZusatzText = zusatzText,
            Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
            KategorieText = kategorie == 1 ? "Getränke" : "Speisen",
            Preiskategorie = kategorie,
            Preis = reader.IsDBNull(5) ? 0m : (decimal)reader.GetDouble(5)
        });
    }
    return Results.Ok(positionen);
});

// POST /api/bestellung  -> Schreibt einen Bewirtungsauftrag samt Positionen und liefert die AuftragId.
app.MapPost("/api/bestellung", (BestellungRequest bestellung) =>
{
    if (bestellung is null || bestellung.Positionen is null || bestellung.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }

    string writeConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWrite
    }.ToString();

    using var connection = new SqliteConnection(writeConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    long auftragId;
    var positionsNummer = 1;

    // Vorhandenen offenen Auftrag (Status = 0) für Termin + Tisch suchen und wiederverwenden.
    using (var sucheCmd = connection.CreateCommand())
    {
        sucheCmd.Transaction = transaction;
        sucheCmd.CommandText = @"
            SELECT AuftragId
            FROM Bewirtungsauftrag
            WHERE Bewirtungstermin = $termin AND TischNummer = $tisch AND Status = 0
            ORDER BY AuftragId
            LIMIT 1;";
        sucheCmd.Parameters.AddWithValue("$termin", bestellung.Bewirtungstermin);
        sucheCmd.Parameters.AddWithValue("$tisch", bestellung.TischNummer);
        var vorhanden = sucheCmd.ExecuteScalar();
        auftragId = vorhanden is long ? Convert.ToInt32(vorhanden) : 0L;
    }

    if (auftragId > 0)
    {
        // Vorhandenen Auftrag verwenden: Gesamtpreis erhöhen und Positionsnummer fortsetzen.
        using (var updateCmd = connection.CreateCommand())
        {
            updateCmd.Transaction = transaction;
            updateCmd.CommandText = @"
                UPDATE Bewirtungsauftrag
                SET Gesamtpreis = Gesamtpreis + $gesamt,
                    StatusLieferung = 0
                WHERE AuftragId = $auftragId;";
            updateCmd.Parameters.AddWithValue("$gesamt", (double)bestellung.Gesamtpreis);
            updateCmd.Parameters.AddWithValue("$auftragId", auftragId);
            updateCmd.ExecuteNonQuery();
        }

        using (var maxCmd = connection.CreateCommand())
        {
            maxCmd.Transaction = transaction;
            maxCmd.CommandText = @"
                SELECT COALESCE(MAX(Bewirtungsposition), 0)
                FROM BewirtungsPosition
                WHERE BewirtungsauftragId = $auftragId;";
            maxCmd.Parameters.AddWithValue("$auftragId", auftragId);
            positionsNummer = Convert.ToInt32(maxCmd.ExecuteScalar()) + 1;
        }
    }
    else
    {
        // Keinen offenen Auftrag gefunden: neuen Bewirtungsauftrag anlegen.
        using var auftragCmd = connection.CreateCommand();
        auftragCmd.Transaction = transaction;
        auftragCmd.CommandText = @"
            INSERT INTO Bewirtungsauftrag
                (Bewirtungstermin, UserId, Gesamtpreis, Bezahlt, Status, Start, TischNummer)
            VALUES
                ($termin, $userId, $gesamt, 0, 0, $start, $tisch);
            SELECT last_insert_rowid();";
        auftragCmd.Parameters.AddWithValue("$termin", bestellung.Bewirtungstermin);
        auftragCmd.Parameters.AddWithValue("$userId", bestellung.UserId);
        auftragCmd.Parameters.AddWithValue("$gesamt", (double)bestellung.Gesamtpreis);
        auftragCmd.Parameters.AddWithValue("$start", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        auftragCmd.Parameters.AddWithValue("$tisch", bestellung.TischNummer);
        auftragId = (long)auftragCmd.ExecuteScalar()!;
    }

    foreach (var pos in bestellung.Positionen)
    {
        using var posCmd = connection.CreateCommand();
        posCmd.Transaction = transaction;
        posCmd.CommandText = @"
            INSERT INTO BewirtungsPosition
                (BewirtungsauftragId, Bewirtungsposition, PreislisteId, PreislisteZusatzId, Preis, Status, Menge, Besonderheit)
            VALUES
                ($auftragId, $posNr, $preislisteId, $zusatzId, $preis, 0, $menge, $besonderheit);";
        posCmd.Parameters.AddWithValue("$auftragId", auftragId);
        posCmd.Parameters.AddWithValue("$posNr", positionsNummer++);
        posCmd.Parameters.AddWithValue("$preislisteId", pos.PreislisteId);
        posCmd.Parameters.AddWithValue("$zusatzId", pos.ZusatzId);
        posCmd.Parameters.AddWithValue("$preis", (double)pos.Preis);
        posCmd.Parameters.AddWithValue("$menge", (double)pos.Menge);
        posCmd.Parameters.AddWithValue("$besonderheit", (object?)pos.Besonderheit ?? string.Empty);
        posCmd.ExecuteNonQuery();
    }

    transaction.Commit();
    return Results.Ok(new { AuftragId = auftragId });
});

// GET /api/bestellung/offen?termin=YYYYMMDD&tisch=5
//   -> Alle offenen Aufträge (Status = 0) für Tisch/Termin samt Positionen (flach, mit Produktnamen).
app.MapGet("/api/bestellung/offen", (int termin, int tisch) =>
{
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();

    var auftragIds = new List<long>();
    double gesamt = 0d;
    using (var auftragCmd = connection.CreateCommand())
    {
        auftragCmd.CommandText = @"
            SELECT AuftragId, Gesamtpreis
            FROM Bewirtungsauftrag
            WHERE Bewirtungstermin = $termin AND TischNummer = $tisch AND Status = 0
            ORDER BY AuftragId;";
        auftragCmd.Parameters.AddWithValue("$termin", termin);
        auftragCmd.Parameters.AddWithValue("$tisch", tisch);
        using var reader = auftragCmd.ExecuteReader();
        while (reader.Read())
        {
            auftragIds.Add(reader.GetInt64(0));
            gesamt += reader.IsDBNull(1) ? 0d : reader.GetDouble(1);
        }
    }

    if (auftragIds.Count == 0)
    {
        return Results.Ok(null as object);
    }

    var inClause = string.Join(",", auftragIds.Select((_, i) => $"$id{i}"));
    var positionen = new List<object>();
    using (var posCmd = connection.CreateCommand())
    {
        posCmd.CommandText = $@"
            SELECT bp.PreislisteId,
                   bp.PreislisteZusatzId,
                   p.Name,
                   COALESCE(z.Bezeichnung, '') AS ZusatzText,
                   bp.Preis,
                   bp.Menge,
                   COALESCE(bp.MengeBezahlt, 0) AS MengeBezahlt,
                   bp.BewirtungsauftragId,
                   bp.Bewirtungsposition
            FROM BewirtungsPosition bp
            LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
            LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
            WHERE bp.BewirtungsauftragId IN ({inClause})
            ORDER BY bp.BewirtungsauftragId, bp.Bewirtungsposition;";
        for (var i = 0; i < auftragIds.Count; i++)
        {
            posCmd.Parameters.AddWithValue($"$id{i}", auftragIds[i]);
        }
        using var reader = posCmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var zusatzText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            positionen.Add(new
            {
                PreislisteId = reader.GetInt64(0),
                ZusatzId = reader.IsDBNull(1) ? 0L : reader.GetInt64(1),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Preis = reader.IsDBNull(4) ? 0m : (decimal)reader.GetDouble(4),
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetDouble(5),
                MengeBezahlt = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6),
                AuftragId = reader.GetInt64(7),
                PosNr = reader.GetInt64(8)
            });
        }
    }

    return Results.Ok(new
    {
        AuftragId = auftragIds[0],
        AuftragIds = auftragIds,
        Gesamtpreis = (decimal)gesamt,
        Positionen = positionen
    });
});

// POST /api/bestellung/bezahlen  -> Setzt Status = 1 und den bezahlten Betrag für einen oder mehrere Aufträge.
app.MapPost("/api/bestellung/bezahlen", (BezahlenRequest req) =>
{
    // Zu bezahlende Aufträge zusammenstellen (Liste hat Vorrang, sonst einzelne AuftragId).
    var auftragIds = (req?.AuftragIds is { Count: > 0 })
        ? req.AuftragIds
        : (req is { AuftragId: > 0 } ? new List<long> { req.AuftragId } : new List<long>());

    if (auftragIds.Count == 0)
    {
        return Results.BadRequest("Ungültiger Auftrag.");
    }

    string writeConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWrite
    }.ToString();

    using var connection = new SqliteConnection(writeConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    var totalRows = 0;
    // Betrag nur auf dem ersten (Haupt-)Auftrag verbuchen; weitere Aufträge werden nur abgeschlossen.
    for (var i = 0; i < auftragIds.Count; i++)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE Bewirtungsauftrag
            SET Status = 1, Bezahlt = $betrag
            WHERE AuftragId = $auftragId AND Status = 0;";
        command.Parameters.AddWithValue("$betrag", i == 0 ? (double)req!.Betrag : 0d);
        command.Parameters.AddWithValue("$auftragId", auftragIds[i]);
        totalRows += command.ExecuteNonQuery();

        // Vollzahlung: alle Positionen des Auftrags als vollständig bezahlt markieren.
        using var posCommand = connection.CreateCommand();
        posCommand.Transaction = transaction;
        posCommand.CommandText = @"
            UPDATE BewirtungsPosition
            SET MengeBezahlt = Menge
            WHERE BewirtungsauftragId = $auftragId;";
        posCommand.Parameters.AddWithValue("$auftragId", auftragIds[i]);
        posCommand.ExecuteNonQuery();
    }

    transaction.Commit();

    if (totalRows == 0)
    {
        return Results.NotFound("Kein offener Auftrag gefunden.");
    }
    return Results.Ok(new { AuftragIds = auftragIds, Abgeschlossen = totalRows });
});

// POST /api/bestellung/geliefert  -> Setzt je Position MengeGeliefert = Menge
//   und StatusLieferung = 1 für Aufträge, deren Positionen alle vollständig geliefert sind.
app.MapPost("/api/bestellung/geliefert", (GeliefertRequest req) =>
{
    if (req?.Positionen is null || req.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }

    string writeConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWrite
    }.ToString();

    using var connection = new SqliteConnection(writeConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    var aktualisiert = 0;
    var betroffeneAuftraege = new HashSet<long>();
    foreach (var pos in req.Positionen)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE BewirtungsPosition
            SET MengeGeliefert = Menge
            WHERE BewirtungsauftragId = $auftragId AND Bewirtungsposition = $posNr;";
        command.Parameters.AddWithValue("$auftragId", pos.AuftragId);
        command.Parameters.AddWithValue("$posNr", pos.PosNr);
        aktualisiert += command.ExecuteNonQuery();
        betroffeneAuftraege.Add(pos.AuftragId);
    }

    // Aufträge, deren Positionen alle vollständig geliefert sind, auf StatusLieferung = 1 setzen.
    foreach (var auftragId in betroffeneAuftraege)
    {
        using var pruefCmd = connection.CreateCommand();
        pruefCmd.Transaction = transaction;
        pruefCmd.CommandText = @"
            SELECT COUNT(*)
            FROM BewirtungsPosition
            WHERE BewirtungsauftragId = $auftragId
              AND COALESCE(MengeGeliefert, 0) < Menge;";
        pruefCmd.Parameters.AddWithValue("$auftragId", auftragId);
        var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

        if (offeneRest == 0)
        {
            using var statusCmd = connection.CreateCommand();
            statusCmd.Transaction = transaction;
            statusCmd.CommandText = @"
                UPDATE Bewirtungsauftrag
                SET StatusLieferung = 1
                WHERE AuftragId = $auftragId;";
            statusCmd.Parameters.AddWithValue("$auftragId", auftragId);
            statusCmd.ExecuteNonQuery();
        }
    }

    transaction.Commit();
    return Results.Ok(new { Aktualisiert = aktualisiert });
});

// POST /api/bestellung/teilzahlen  -> Erhöht die bezahlte Menge je Position (Teilrechnung).
//   Aufträge, deren Positionen alle voll bezahlt sind, werden auf Status = 1 gesetzt.
app.MapPost("/api/bestellung/teilzahlen", (TeilzahlenRequest req) =>
{
    if (req?.Positionen is null || req.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }

    string writeConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWrite
    }.ToString();

    using var connection = new SqliteConnection(writeConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    // Bezahlte Mengen je Position erhöhen (nie über die bestellte Menge hinaus).
    var betroffeneAuftraege = new HashSet<long>();
    foreach (var pos in req.Positionen.Where(p => p.MengeZahlen > 0m))
    {
        using var updateCmd = connection.CreateCommand();
        updateCmd.Transaction = transaction;
        updateCmd.CommandText = @"
            UPDATE BewirtungsPosition
            SET MengeBezahlt = MIN(Menge, COALESCE(MengeBezahlt, 0) + $mengeZahlen)
            WHERE BewirtungsauftragId = $auftragId AND Bewirtungsposition = $posNr;";
        updateCmd.Parameters.AddWithValue("$mengeZahlen", (double)pos.MengeZahlen);
        updateCmd.Parameters.AddWithValue("$auftragId", pos.AuftragId);
        updateCmd.Parameters.AddWithValue("$posNr", pos.PosNr);
        updateCmd.ExecuteNonQuery();

        // Bezahlten Betrag des Auftrags um (Teilmenge × Positionspreis) erhöhen.
        using var betragCmd = connection.CreateCommand();
        betragCmd.Transaction = transaction;
        betragCmd.CommandText = @"
            UPDATE Bewirtungsauftrag
            SET Bezahlt = COALESCE(Bezahlt, 0) + (
                $mengeZahlen * (
                    SELECT Preis FROM BewirtungsPosition
                    WHERE BewirtungsauftragId = $auftragId AND Bewirtungsposition = $posNr
                )
            )
            WHERE AuftragId = $auftragId;";
        betragCmd.Parameters.AddWithValue("$mengeZahlen", (double)pos.MengeZahlen);
        betragCmd.Parameters.AddWithValue("$auftragId", pos.AuftragId);
        betragCmd.Parameters.AddWithValue("$posNr", pos.PosNr);
        betragCmd.ExecuteNonQuery();

        betroffeneAuftraege.Add(pos.AuftragId);
    }

    // Aufträge abschließen, deren Positionen alle voll bezahlt sind.
    var abgeschlossen = new List<long>();
    foreach (var auftragId in betroffeneAuftraege)
    {
        using var pruefCmd = connection.CreateCommand();
        pruefCmd.Transaction = transaction;
        pruefCmd.CommandText = @"
            SELECT COUNT(*)
            FROM BewirtungsPosition
            WHERE BewirtungsauftragId = $auftragId
              AND COALESCE(MengeBezahlt, 0) < Menge;";
        pruefCmd.Parameters.AddWithValue("$auftragId", auftragId);
        var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

        if (offeneRest == 0)
        {
            using var abschlussCmd = connection.CreateCommand();
            abschlussCmd.Transaction = transaction;
            abschlussCmd.CommandText = @"
                UPDATE Bewirtungsauftrag
                SET Status = 1
                WHERE AuftragId = $auftragId AND Status = 0;";
            abschlussCmd.Parameters.AddWithValue("$auftragId", auftragId);
            if (abschlussCmd.ExecuteNonQuery() > 0)
            {
                abgeschlossen.Add(auftragId);
            }
        }
    }

    transaction.Commit();
    return Results.Ok(new { Abgeschlossen = abgeschlossen });
});

// GET /api/auslieferung/getraenke?termin=YYYYMMDD
//   -> Offene Getränke-Positionen (Preiskategorie=1, MengeGeliefert < Menge) je Tisch gruppiert.
app.MapGet("/api/auslieferung/getraenke", (int termin) =>
{
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT a.TischNummer,
               bp.BewirtungsauftragId,
               bp.Bewirtungsposition,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               bp.Menge,
               COALESCE(bp.MengeGeliefert, 0) AS MengeGeliefert
        FROM BewirtungsPosition bp
        JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
        JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
        WHERE a.Bewirtungstermin = $termin
          AND p.Preiskategorie = 1
          AND COALESCE(bp.MengeGeliefert, 0) < bp.Menge
        ORDER BY a.TischNummer, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("$termin", termin);

    var tische = new Dictionary<int, List<object>>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var tischNummer = (int)reader.GetInt64(0);
            var name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var zusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var position = new
            {
                AuftragId = reader.GetInt64(1),
                PosNr = reader.GetInt64(2),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetDouble(5),
                MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6)
            };
            if (!tische.TryGetValue(tischNummer, out var liste))
            {
                liste = new List<object>();
                tische[tischNummer] = liste;
            }
            liste.Add(position);
        }
    }

    var ergebnis = tische
        .OrderBy(t => t.Key)
        .Select(t => new { TischNummer = t.Key, Positionen = t.Value })
        .ToList();
    return Results.Ok(ergebnis);
});

// GET /api/auslieferung/speisen?termin=YYYYMMDD
//   -> Offene Speisen-Positionen (Preiskategorie=0, MengeGeliefert < Menge) je Tisch gruppiert.
app.MapGet("/api/auslieferung/speisen", (int termin) =>
{
    using var connection = new SqliteConnection(ConnectionString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT a.TischNummer,
               bp.BewirtungsauftragId,
               bp.Bewirtungsposition,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               bp.Menge,
               COALESCE(bp.MengeGeliefert, 0) AS MengeGeliefert
        FROM BewirtungsPosition bp
        JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
        JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
        WHERE a.Bewirtungstermin = $termin
          AND p.Preiskategorie = 0
          AND COALESCE(bp.MengeGeliefert, 0) < bp.Menge
        ORDER BY a.TischNummer, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("$termin", termin);

    var tische = new Dictionary<int, List<object>>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var tischNummer = (int)reader.GetInt64(0);
            var name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var zusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var position = new
            {
                AuftragId = reader.GetInt64(1),
                PosNr = reader.GetInt64(2),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetDouble(5),
                MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetDouble(6)
            };
            if (!tische.TryGetValue(tischNummer, out var liste))
            {
                liste = new List<object>();
                tische[tischNummer] = liste;
            }
            liste.Add(position);
        }
    }

    var ergebnis = tische
        .OrderBy(t => t.Key)
        .Select(t => new { TischNummer = t.Key, Positionen = t.Value })
        .ToList();
    return Results.Ok(ergebnis);
});

// POST /api/auslieferung/fertig  -> Setzt je Position MengeGeliefert (und ggf. Menge) für einen Tisch.
//   Pro Position gilt die auf der Seite erfasste Zusatzmenge (Delta):
//     Delta == 0  -> MengeGeliefert = Menge (alles wie bestellt geliefert)
//     Delta != 0  -> Menge = Menge + Delta und MengeGeliefert = Delta
app.MapPost("/api/auslieferung/fertig", (AuslieferungFertigRequest req) =>
{
    if (req?.Positionen is null || req.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }

    string writeConnectionString = new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadWrite
    }.ToString();

    using var connection = new SqliteConnection(writeConnectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    var aktualisiert = 0;
    var betroffeneAuftraege = new HashSet<long>();
    foreach (var pos in req.Positionen)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        // Menge bleibt unberührt; die gelieferte Menge wird um die auf der Seite
        // erfasste Menge erhöht (nie über die bestellte Menge hinaus).
        command.CommandText = @"
            UPDATE BewirtungsPosition
            SET MengeGeliefert = CASE 
                WHEN MengeGeliefert IS NULL OR MengeGeliefert = '' THEN 0 
                ELSE MengeGeliefert 
            END + $delta
            WHERE BewirtungsauftragId = $auftragId AND Bewirtungsposition = $posNr;";
        command.Parameters.AddWithValue("$delta", (double)pos.SeitenMenge);
        command.Parameters.AddWithValue("$auftragId", pos.AuftragId);
        command.Parameters.AddWithValue("$posNr", pos.PosNr);
        aktualisiert += command.ExecuteNonQuery();
        betroffeneAuftraege.Add(pos.AuftragId);
    }

    // Aufträge, deren Positionen alle vollständig geliefert sind, auf StatusLieferung = 1 setzen.
    foreach (var auftragId in betroffeneAuftraege)
    {
        using var pruefCmd = connection.CreateCommand();
        pruefCmd.Transaction = transaction;
        pruefCmd.CommandText = @"
            SELECT COUNT(*)
            FROM BewirtungsPosition
            WHERE BewirtungsauftragId = $auftragId
              AND COALESCE(MengeGeliefert, 0) < Menge;";
        pruefCmd.Parameters.AddWithValue("$auftragId", auftragId);
        var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

        if (offeneRest == 0)
        {
            using var statusCmd = connection.CreateCommand();
            statusCmd.Transaction = transaction;
            statusCmd.CommandText = @"
                UPDATE Bewirtungsauftrag
                SET StatusLieferung = 1
                WHERE AuftragId = $auftragId;";
            statusCmd.Parameters.AddWithValue("$auftragId", auftragId);
            statusCmd.ExecuteNonQuery();
        }
    }

    transaction.Commit();
    return Results.Ok(new { Aktualisiert = aktualisiert });
});

app.Run();

record BestellungRequest(int Bewirtungstermin, long UserId, decimal Gesamtpreis, int TischNummer, List<BestellungPosition> Positionen);
record BestellungPosition(long PreislisteId, long ZusatzId, decimal Preis, decimal Menge, string? Besonderheit = null);
record BezahlenRequest(long AuftragId, decimal Betrag, List<long>? AuftragIds = null);
record TeilzahlenRequest(decimal Betrag, List<TeilzahlenPosition> Positionen);
record TeilzahlenPosition(long AuftragId, long PosNr, decimal MengeZahlen);
record AuslieferungFertigRequest(List<AuslieferungFertigPosition> Positionen);
record AuslieferungFertigPosition(long AuftragId, long PosNr, decimal SeitenMenge);
record GeliefertRequest(List<GeliefertPosition> Positionen);
record GeliefertPosition(long AuftragId, long PosNr);
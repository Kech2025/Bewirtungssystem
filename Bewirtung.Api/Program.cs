using Npgsql;
using Bewirtung.Fiskal;
using Bewirtung.Fiskal.Tse;

var builder = WebApplication.CreateBuilder(args);

// TSE / Fiskalisierung (KassenSichV) – gemeinsame Bibliothek Bewirtung.Fiskal.
// Läuft aktuell im Cloud-TSE-Dummy-Modus (CloudTse:DummyModus) – NICHT fiskalkonform.
builder.Services.Configure<CloudTseOptions>(
    builder.Configuration.GetSection(CloudTseOptions.SectionName));
builder.Services.AddSingleton<ITseProvider, CloudTseProvider>();
builder.Services.AddSingleton<FiskalDatabase>(_ =>
{
    const string fallback =
        "Host=localhost;Port=5432;Database=bewirtung;Username=postgres;Password=postgres";
    var configured = builder.Configuration.GetConnectionString("Postgres")
        ?? builder.Configuration["Database:ConnectionString"];
    return new FiskalDatabase(string.IsNullOrWhiteSpace(configured) ? fallback : configured);
});
builder.Services.AddSingleton<FiskalRepository>();

var app = builder.Build();

// Connection-String zur gemeinsamen PostgreSQL-Datenbank
// (ConnectionStrings:Postgres oder Database:ConnectionString; Passwort via User Secrets).
const string DefaultConnectionString =
    "Host=localhost;Port=5432;Database=bewirtung;Username=postgres;Password=postgres";

string ConnectionString()
{
    var configured = app.Configuration.GetConnectionString("Postgres")
        ?? app.Configuration["Database:ConnectionString"];
    return string.IsNullOrWhiteSpace(configured) ? DefaultConnectionString : configured;
}

// Öffnet eine neue PostgreSQL-Verbindung.
NpgsqlConnection OpenConnection()
{
    var connection = new NpgsqlConnection(ConnectionString());
    connection.Open();
    return connection;
}

// Datums-Grenze: API nutzt int yyyyMMdd, PostgreSQL nutzt date.
static DateTime FromYyyyMmDd(int value)
{
    var year = value / 10000;
    var month = value / 100 % 100;
    var day = value % 100;
    if (year < 1 || month < 1 || month > 12 || day < 1 || day > 31)
    {
        return DateTime.Today;
    }
    try
    {
        return new DateTime(year, month, day);
    }
    catch
    {
        return DateTime.Today;
    }
}

static int ToYyyyMmDd(DateTime date) =>
    date.Year * 10000 + date.Month * 100 + date.Day;

// Baut aus den bezahlten Aufträgen einen fiskalischen Beleg (Kopf, Positionen,
// Steueraufteilung) für die TSE-Signatur. Nutzt die vorhandene offene Verbindung.
static FiskalBeleg BaueFiskalBeleg(NpgsqlConnection connection, List<long> auftragIds, BezahlenRequest req)
{
    var beleg = new FiskalBeleg
    {
        KasseId = string.IsNullOrWhiteSpace(req.HandyName) ? "UNBEKANNT" : req.HandyName!,
        Bediener = req.HandyName ?? string.Empty,
        VorgangsTyp = FiskalVorgangsTyp.Beleg,
        Zahlart = FiskalZahlart.Bar,
        StartZeitpunkt = DateTime.Now,
        GesamtBrutto = req.Betrag
    };

    var inClause = string.Join(",", auftragIds.Select((_, i) => $"@id{i}"));
    using var cmd = connection.CreateCommand();
    cmd.CommandText = $@"
        SELECT p.Name,
               bp.Preis,
               bp.Menge,
               COALESCE(m.mwst, 0) AS MwstSatz
        FROM BewirtungsPosition bp
        LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN Mwst m ON m.Preiskategorie = p.Preiskategorie
        WHERE bp.BewirtungsauftragId IN ({inClause})
        ORDER BY bp.BewirtungsauftragId, bp.Bewirtungsposition;";
    for (var i = 0; i < auftragIds.Count; i++)
    {
        cmd.Parameters.AddWithValue($"@id{i}", (int)auftragIds[i]);
    }

    var positionsNr = 0;
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            var artikel = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var einzelpreis = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
            var menge = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
            var satz = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
            beleg.Positionen.Add(new FiskalBelegPosition
            {
                PositionsNummer = ++positionsNr,
                Artikel = artikel,
                Menge = menge,
                EinzelpreisBrutto = einzelpreis,
                GesamtBrutto = einzelpreis * menge,
                MwstSatz = satz
            });
        }
    }

    // Steueraufteilung je Steuersatz aus den Positionen berechnen.
    foreach (var gruppe in beleg.Positionen.GroupBy(p => p.MwstSatz))
    {
        var brutto = gruppe.Sum(p => p.GesamtBrutto);
        var satz = gruppe.Key;
        var netto = satz > 0 ? Math.Round(brutto / (1 + satz / 100m), 2) : brutto;
        beleg.SteuerPositionen.Add(new FiskalSteuerPosition
        {
            SteuerSchluessel = satz == 19m ? 1 : satz == 7m ? 2 : 0,
            MwstSatz = satz,
            Brutto = brutto,
            Netto = netto,
            Steuer = brutto - netto
        });
    }

    // Falls keine Positionen ermittelbar sind, zumindest den Gesamtbetrag abbilden.
    if (beleg.GesamtBrutto == 0m && beleg.Positionen.Count > 0)
    {
        beleg.GesamtBrutto = beleg.Positionen.Sum(p => p.GesamtBrutto);
    }

    return beleg;
}

// Baut einen Fiskalbeleg für eine Teilzahlung aus den tatsächlich bezahlten Teilmengen.
static FiskalBeleg BaueFiskalBelegTeilzahlung(NpgsqlConnection connection, NpgsqlTransaction transaction, TeilzahlenRequest req)
{
    var beleg = new FiskalBeleg
    {
        KasseId = string.IsNullOrWhiteSpace(req.HandyName) ? "UNBEKANNT" : req.HandyName!,
        Bediener = req.HandyName ?? string.Empty,
        VorgangsTyp = FiskalVorgangsTyp.Beleg,
        Zahlart = FiskalZahlart.Bar,
        StartZeitpunkt = DateTime.Now,
        GesamtBrutto = req.Betrag
    };

    var positionsNr = 0;
    foreach (var pos in req.Positionen.Where(p => p.MengeZahlen > 0m))
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = @"
            SELECT p.Name,
                   bp.Preis,
                   COALESCE(m.mwst, 0) AS MwstSatz
            FROM BewirtungsPosition bp
            LEFT JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
            LEFT JOIN Mwst m ON m.Preiskategorie = p.Preiskategorie
            WHERE bp.BewirtungsauftragId = @auftragId AND bp.Bewirtungsposition = @posNr;";
        cmd.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
        cmd.Parameters.AddWithValue("@posNr", (int)pos.PosNr);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var artikel = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var einzelpreis = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
            var satz = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
            beleg.Positionen.Add(new FiskalBelegPosition
            {
                PositionsNummer = ++positionsNr,
                Artikel = artikel,
                Menge = pos.MengeZahlen,
                EinzelpreisBrutto = einzelpreis,
                GesamtBrutto = einzelpreis * pos.MengeZahlen,
                MwstSatz = satz
            });
        }
    }

    // Steueraufteilung je Steuersatz aus den Positionen berechnen.
    foreach (var gruppe in beleg.Positionen.GroupBy(p => p.MwstSatz))
    {
        var brutto = gruppe.Sum(p => p.GesamtBrutto);
        var satz = gruppe.Key;
        var netto = satz > 0 ? Math.Round(brutto / (1 + satz / 100m), 2) : brutto;
        beleg.SteuerPositionen.Add(new FiskalSteuerPosition
        {
            SteuerSchluessel = satz == 19m ? 1 : satz == 7m ? 2 : 0,
            MwstSatz = satz,
            Brutto = brutto,
            Netto = netto,
            Steuer = brutto - netto
        });
    }

    if (beleg.GesamtBrutto == 0m && beleg.Positionen.Count > 0)
    {
        beleg.GesamtBrutto = beleg.Positionen.Sum(p => p.GesamtBrutto);
    }

    return beleg;
}

// GET /api/tische  -> [{ Tischnummer, Bereich }, ...]
app.MapGet("/api/tische", (int datum) =>
{
    var tische = new List<object>();
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT t.Tischnummer, b.Bezeichnung, COALESCE(t.SofortLieferung, false),
               CASE WHEN EXISTS (
                   SELECT 1 FROM Bewirtungsauftrag ba
                   WHERE ba.TischNummer = t.Tischnummer
                     AND ba.Bewirtungstermin = @datum
                     AND ba.Status = false
               ) THEN true ELSE false END AS OffeneRechnung
        FROM Tisch t
        INNER JOIN TischTermin tt ON tt.Tischnummer = t.Tischnummer AND tt.Bewirtungsdatum = @datum
        LEFT JOIN BereichTische b ON b.BereichId = t.BereichId
        ORDER BY t.Tischnummer ASC;";
    command.Parameters.AddWithValue("@datum", FromYyyyMmDd(datum));
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        if (!reader.IsDBNull(0))
        {
            tische.Add(new
            {
                Tischnummer = reader.GetInt32(0),
                Bereich = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                SofortLieferung = reader.IsDBNull(2) ? 0 : (reader.GetBoolean(2) ? 1 : 0),
                OffeneRechnung = !reader.IsDBNull(3) && reader.GetBoolean(3)
            });
        }
    }
    return Results.Ok(tische);
});

// GET /api/benutzer  ->  Liste der Benutzer (UserId + Name) für die Anmeldung.
app.MapGet("/api/benutzer", () =>
{
    var benutzer = new List<object>();
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT UserId, Name FROM Benutzer ORDER BY Name;";
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        benutzer.Add(new
        {
            UserId = (long)reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
        });
    }
    return Results.Ok(benutzer);
});

// GET /api/benutzer/termin?datum=...&vereinId=...  -> Helfer, die über HelferTermin dem Termin zugeordnet sind.
app.MapGet("/api/benutzer/termin", (int datum, long vereinId) =>
{
    var benutzer = new List<object>();
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT b.UserId, b.Name
        FROM HelferTermin h
        JOIN Benutzer b ON b.UserId = h.UserId
        WHERE h.Bewirtungsdatum = @datum AND h.VereinId = @vereinId
        ORDER BY b.Name;";
    command.Parameters.AddWithValue("@datum", FromYyyyMmDd(datum));
    command.Parameters.AddWithValue("@vereinId", (int)vereinId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        benutzer.Add(new
        {
            UserId = (long)reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
        });
    }
    return Results.Ok(benutzer);
});

// GET /api/handy/validate?seriennummer=...  -> Prüft die Seriennummer gegen die Tabelle handy.
//   Liefert bei Treffer { Seriennummer, Name }, sonst 404.
app.MapGet("/api/handy/validate", (string? seriennummer) =>
{
    if (string.IsNullOrWhiteSpace(seriennummer))
    {
        return Results.BadRequest("Keine Seriennummer übergeben.");
    }

    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT seriennummer, name, bestellung
        FROM handy
        WHERE seriennummer = @seriennummer
        LIMIT 1;";
    command.Parameters.AddWithValue("@seriennummer", seriennummer.Trim());
    using var reader = command.ExecuteReader();
    if (!reader.Read())
    {
        return Results.NotFound("Seriennummer nicht gefunden.");
    }

    return Results.Ok(new
    {
        Seriennummer = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
        Bestellung = !reader.IsDBNull(2) && reader.GetBoolean(2)
    });
});

// GET /api/kuechenstationen  -> [{ Id, Name }, ...] aus Tabelle kuechenstation.
// Optional: ?datum=YYYYMMDD&vereinId=123 -> nur die dem Termin zugeordneten Küchenstationen.
app.MapGet("/api/kuechenstationen", (int? datum, long? vereinId) =>
{
    var stationen = new List<object>();
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    if (datum.HasValue && vereinId.HasValue)
    {
        command.CommandText = @"
            SELECT k.id, k.name
            FROM kuechenstationtermin kst
            JOIN Termin t ON t.bewirtungsdatum = kst.bewirtungsdatum
            JOIN kuechenstation k ON k.id = kst.kuechenstationid
            WHERE t.BewirtungsDatum = @datum AND t.VereinId = @vereinId
            ORDER BY k.name;";
        command.Parameters.AddWithValue("@datum", FromYyyyMmDd(datum.Value));
        command.Parameters.AddWithValue("@vereinId", vereinId.Value);
    }
    else
    {
        command.CommandText = "SELECT id, name FROM kuechenstation ORDER BY name;";
    }
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        stationen.Add(new
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
        });
    }
    return Results.Ok(stationen);
});

app.MapGet("/api/termine", () =>
{
    var termine = new List<object>();
    int nowDate = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT t.BewirtungsDatum, t.VereinId, v.Name
        FROM Termin t
        LEFT JOIN Verein v ON v.Id = t.VereinId
        WHERE t.BewirtungsDatum >= @datum
        ORDER BY t.BewirtungsDatum DESC, v.Name;";
    command.Parameters.AddWithValue("@datum", FromYyyyMmDd(nowDate));
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        termine.Add(new
        {
            BewirtungsDatum = ToYyyyMmDd(reader.GetDateTime(0)),
            VereinId = (long)reader.GetInt32(1),
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
    using var connection = OpenConnection();
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
        WHERE pt.Bewirtungsdatum = @datum AND pt.VereinId = @vereinId
        ORDER BY p.Preiskategorie, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("@datum", FromYyyyMmDd(datum));
    command.Parameters.AddWithValue("@vereinId", (int)vereinId);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        var kategorie = reader.IsDBNull(4) ? 0L : (long)reader.GetInt32(4);
        var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
        var zusatzText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
        positionen.Add(new
        {
            PreislisteId = (long)reader.GetInt32(0),
            ZusatzId = reader.IsDBNull(1) ? 0L : (long)reader.GetInt32(1),
            Name = name,
            ZusatzText = zusatzText,
            Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
            KategorieText = kategorie == 1 ? "Getränke" : "Speisen",
            Preiskategorie = kategorie,
            Preis = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
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

    long auftragId;
    using var connection = OpenConnection();
    {
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var positionsNummer = 1;

                // Vorhandenen offenen Auftrag (Status = false) für Termin + Tisch suchen und wiederverwenden.
                using (var sucheCmd = connection.CreateCommand())
                {
                    sucheCmd.Transaction = transaction;
                    sucheCmd.CommandText = @"
                    SELECT AuftragId
                    FROM Bewirtungsauftrag
                    WHERE Bewirtungstermin = @termin AND TischNummer = @tisch AND Status = false
                    ORDER BY AuftragId
                    LIMIT 1;";
                    sucheCmd.Parameters.AddWithValue("@termin", FromYyyyMmDd(bestellung.Bewirtungstermin));
                    sucheCmd.Parameters.AddWithValue("@tisch", bestellung.TischNummer);
                    var vorhanden = sucheCmd.ExecuteScalar();
                    auftragId = vorhanden is int i ? i : 0L;
                }

                if (auftragId > 0)
                {
                    // Vorhandenen Auftrag verwenden: Gesamtpreis erhöhen und Positionsnummer fortsetzen.
                    using (var updateCmd = connection.CreateCommand())
                    {
                        updateCmd.Transaction = transaction;
                        updateCmd.CommandText = @"
                        UPDATE Bewirtungsauftrag
                        SET Gesamtpreis = Gesamtpreis + @gesamt,
                            StatusLieferung = false
                        WHERE AuftragId = @auftragId;";
                        updateCmd.Parameters.AddWithValue("@gesamt", bestellung.Gesamtpreis);
                        updateCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                        updateCmd.ExecuteNonQuery();
                    }

                    using (var maxCmd = connection.CreateCommand())
                    {
                        maxCmd.Transaction = transaction;
                        maxCmd.CommandText = @"
                        SELECT COALESCE(MAX(Bewirtungsposition), 0)
                        FROM BewirtungsPosition
                        WHERE BewirtungsauftragId = @auftragId;";
                        maxCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
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
                        (Bewirtungstermin, UserId, Gesamtpreis, Bezahlt, Status, StartDateTime, TischNummer)
                    VALUES
                        (@termin, @userId, @gesamt, 0, false, @start, @tisch)
                    RETURNING AuftragId;";
                    auftragCmd.Parameters.AddWithValue("@termin", FromYyyyMmDd(bestellung.Bewirtungstermin));
                    auftragCmd.Parameters.AddWithValue("@userId", (int)bestellung.UserId);
                    auftragCmd.Parameters.AddWithValue("@gesamt", bestellung.Gesamtpreis);
                    auftragCmd.Parameters.AddWithValue("@start", DateTime.Now);
                    auftragCmd.Parameters.AddWithValue("@tisch", bestellung.TischNummer);
                    auftragId = Convert.ToInt64(auftragCmd.ExecuteScalar()!);
                }

                foreach (var pos in bestellung.Positionen)
                {
                    using var posCmd = connection.CreateCommand();
                    posCmd.Transaction = transaction;
                    posCmd.CommandText = @"
                        INSERT INTO BewirtungsPosition
                            (BewirtungsauftragId, Bewirtungsposition, PreislisteId, PreislisteZusatzId, Preis, Status, Menge, Besonderheit, Buchungszeitpunkt)
                        VALUES
                            (@auftragId, @posNr, @preislisteId, @zusatzId, @preis, false, @menge, @besonderheit, @buchungszeitpunkt);";
                    posCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                    posCmd.Parameters.AddWithValue("@posNr", positionsNummer++);
                    posCmd.Parameters.AddWithValue("@preislisteId", (int)pos.PreislisteId);
                    posCmd.Parameters.AddWithValue("@zusatzId", (int)pos.ZusatzId);
                    posCmd.Parameters.AddWithValue("@preis", pos.Preis);
                    posCmd.Parameters.AddWithValue("@menge", (int)pos.Menge);
                    posCmd.Parameters.AddWithValue("@besonderheit", (object?)pos.Besonderheit ?? string.Empty);
                    posCmd.Parameters.AddWithValue("@buchungszeitpunkt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    posCmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback(); // Alles rückgängig machen
                throw;
            }
        }
    }
    return Results.Ok(new { AuftragId = auftragId });
});

// GET /api/bestellung/offen?termin=YYYYMMDD&tisch=5
//   -> Alle offenen Aufträge (Status = 0) für Tisch/Termin samt Positionen (flach, mit Produktnamen).
app.MapGet("/api/bestellung/offen", (int termin, int tisch) =>
{
    using var connection = OpenConnection();
    var auftragIds = new List<long>();
    decimal gesamt = 0m;
    using (var auftragCmd = connection.CreateCommand())
    {
        auftragCmd.CommandText = @"
            SELECT AuftragId, Gesamtpreis
            FROM Bewirtungsauftrag
            WHERE Bewirtungstermin = @termin AND TischNummer = @tisch AND Status = false
            ORDER BY AuftragId;";
        auftragCmd.Parameters.AddWithValue("@termin", FromYyyyMmDd(termin));
        auftragCmd.Parameters.AddWithValue("@tisch", tisch);
        using var reader = auftragCmd.ExecuteReader();
        while (reader.Read())
        {
            auftragIds.Add(reader.GetInt32(0));
            gesamt += reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
        }
    }

    if (auftragIds.Count == 0)
    {
        return Results.Ok(null as object);
    }

    var inClause = string.Join(",", auftragIds.Select((_, i) => $"@id{i}"));
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
            posCmd.Parameters.AddWithValue($"@id{i}", (int)auftragIds[i]);
        }
        using var reader = posCmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var zusatzText = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            positionen.Add(new
            {
                PreislisteId = (long)reader.GetInt32(0),
                ZusatzId = reader.IsDBNull(1) ? 0L : (long)reader.GetInt32(1),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Preis = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetInt32(5),
                MengeBezahlt = reader.IsDBNull(6) ? 0m : (decimal)reader.GetInt32(6),
                AuftragId = (long)reader.GetInt32(7),
                PosNr = (long)reader.GetInt32(8)
            });
        }
    }

    return Results.Ok(new
    {
        AuftragId = auftragIds[0],
        AuftragIds = auftragIds,
        Gesamtpreis = gesamt,
        Positionen = positionen
    });
});

// POST /api/bestellung/bezahlen  -> Setzt Status = 1 und den bezahlten Betrag für einen oder mehrere Aufträge.
app.MapPost("/api/bestellung/bezahlen", async (BezahlenRequest req, FiskalRepository fiskal) =>
{
    // Zu bezahlende Aufträge zusammenstellen (Liste hat Vorrang, sonst einzelne AuftragId).
    var auftragIds = (req?.AuftragIds is { Count: > 0 })
        ? req.AuftragIds
        : (req is { AuftragId: > 0 } ? new List<long> { req.AuftragId } : new List<long>());

    if (auftragIds.Count == 0)
    {
        return Results.BadRequest("Ungültiger Auftrag.");
    }
    var totalRows = 0;
    using var connection = OpenConnection();
    {
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // Betrag nur auf dem ersten (Haupt-)Auftrag verbuchen; weitere Aufträge werden nur abgeschlossen.
                for (var i = 0; i < auftragIds.Count; i++)
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = @"
                        UPDATE Bewirtungsauftrag
                        SET Status = true, Bezahlt = @betrag, enddatetime = now(), handyname = @handyname
                        WHERE AuftragId = @auftragId AND Status = false;";
                    command.Parameters.AddWithValue("@betrag", i == 0 ? req!.Betrag : 0m);
                    command.Parameters.AddWithValue("@handyname", (object?)req!.HandyName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@auftragId", (int)auftragIds[i]);
                    totalRows += command.ExecuteNonQuery();

                    // Vollzahlung: alle Positionen des Auftrags als vollständig bezahlt markieren.
                    using var posCommand = connection.CreateCommand();
                    posCommand.Transaction = transaction;
                    posCommand.CommandText = @"
                        UPDATE BewirtungsPosition
                        SET MengeBezahlt = Menge, Status = true
                        WHERE BewirtungsauftragId = @auftragId;";
                    posCommand.Parameters.AddWithValue("@auftragId", (int)auftragIds[i]);
                    posCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback(); // Alles rückgängig machen
                throw;
            }
        }

        if (totalRows == 0)
        {
            return Results.NotFound("Kein offener Auftrag gefunden.");
        }

        // Fiskalisierung: signierten Kassenbeleg über die TSE erzeugen und speichern.
        // Schlägt dies fehl, bleibt die Zahlung bestehen; der Fehler wird protokolliert
        // zurückgemeldet, damit er nicht unbemerkt bleibt.
        try
        {
            var beleg = BaueFiskalBeleg(connection, auftragIds, req!);
            await fiskal.SpeichereBelegMitSignaturAsync(beleg);
            return Results.Ok(new
            {
                AuftragIds = auftragIds,
                Abgeschlossen = totalRows,
                beleg.BelegNummer,
                Signatur = beleg.Signatur?.Signatur,
                beleg.Signatur?.TransaktionsNummer,
                QrCode = beleg.Signatur?.QrCodeDaten
            });
        }
        catch (Exception ex)
        {
            return Results.Ok(new
            {
                AuftragIds = auftragIds,
                Abgeschlossen = totalRows,
                FiskalFehler = ex.Message
            });
        }
    }
});

// POST /api/bestellung/geliefert  -> Setzt je Position MengeGeliefert = Menge
//   und StatusLieferung = 1 für Aufträge, deren Positionen alle vollständig geliefert sind.
app.MapPost("/api/bestellung/geliefert", (GeliefertRequest req) =>
{
    if (req?.Positionen is null || req.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }
    var aktualisiert = 0;
    using var connection = OpenConnection();
    {
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                var betroffeneAuftraege = new HashSet<long>();
                foreach (var pos in req.Positionen)
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = @"
                        UPDATE BewirtungsPosition
                        SET MengeGeliefert = Menge
                        WHERE BewirtungsauftragId = @auftragId AND Bewirtungsposition = @posNr;";
                    command.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
                    command.Parameters.AddWithValue("@posNr", (int)pos.PosNr);
                    aktualisiert += command.ExecuteNonQuery();
                    betroffeneAuftraege.Add(pos.AuftragId);
                }

                // Aufträge, deren Positionen alle vollständig geliefert sind, auf StatusLieferung = true setzen.
                foreach (var auftragId in betroffeneAuftraege)
                {
                    using var pruefCmd = connection.CreateCommand();
                    pruefCmd.Transaction = transaction;
                    pruefCmd.CommandText = @"
                        SELECT COUNT(*)
                        FROM BewirtungsPosition
                        WHERE BewirtungsauftragId = @auftragId
                          AND COALESCE(MengeGeliefert, 0) < Menge;";
                    pruefCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                    var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

                    if (offeneRest == 0)
                    {
                        using var statusCmd = connection.CreateCommand();
                        statusCmd.Transaction = transaction;
                        statusCmd.CommandText = @"
                            UPDATE Bewirtungsauftrag
                            SET StatusLieferung = true
                            WHERE AuftragId = @auftragId;";
                        statusCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                        statusCmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback(); // Alles rückgängig machen
                throw;
            }
        }
        return Results.Ok(new { Aktualisiert = aktualisiert });
    }
});

// POST /api/bestellung/teilzahlen  -> Erhöht die bezahlte Menge je Position (Teilrechnung).
//   Aufträge, deren Positionen alle voll bezahlt sind, werden auf Status = 1 gesetzt.
app.MapPost("/api/bestellung/teilzahlen", async (TeilzahlenRequest req, FiskalRepository fiskal) =>
{
    if (req?.Positionen is null || req.Positionen.Count == 0)
    {
        return Results.BadRequest("Keine Positionen übergeben.");
    }

    var abgeschlossen = new List<long>();
    FiskalBeleg? fiskalBeleg = null;
    using var connection = OpenConnection();
    {
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // Bezahlte Mengen je Position erhöhen (nie über die bestellte Menge hinaus).
                var betroffeneAuftraege = new HashSet<long>();
                foreach (var pos in req.Positionen.Where(p => p.MengeZahlen > 0m))
                {
                    using var updateCmd = connection.CreateCommand();
                    updateCmd.Transaction = transaction;
                    updateCmd.CommandText = @"
                        UPDATE BewirtungsPosition
                        SET MengeBezahlt = LEAST(Menge, COALESCE(MengeBezahlt, 0) + @mengeZahlen)
                        WHERE BewirtungsauftragId = @auftragId AND Bewirtungsposition = @posNr;";
                    updateCmd.Parameters.AddWithValue("@mengeZahlen", (int)pos.MengeZahlen);
                    updateCmd.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
                    updateCmd.Parameters.AddWithValue("@posNr", (int)pos.PosNr);
                    updateCmd.ExecuteNonQuery();

                    // Bezahlten Betrag des Auftrags um (Teilmenge × Positionspreis) erhöhen.
                    using var betragCmd = connection.CreateCommand();
                    betragCmd.Transaction = transaction;
                    betragCmd.CommandText = @"
                        UPDATE Bewirtungsauftrag
                        SET Bezahlt = COALESCE(Bezahlt, 0) + (
                            @mengeZahlen * (
                                SELECT Preis FROM BewirtungsPosition
                                WHERE BewirtungsauftragId = @auftragId AND Bewirtungsposition = @posNr
                            )
                        ),
                        handyname = @handyname
                        WHERE AuftragId = @auftragId;";
                    betragCmd.Parameters.AddWithValue("@mengeZahlen", pos.MengeZahlen);
                    betragCmd.Parameters.AddWithValue("@handyname", (object?)req.HandyName ?? DBNull.Value);
                    betragCmd.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
                    betragCmd.Parameters.AddWithValue("@posNr", (int)pos.PosNr);
                    betragCmd.ExecuteNonQuery();

                    // Bezahlten Betrag des Auftrags um (Teilmenge × Positionspreis) erhöhen.
                    using var statusCmd = connection.CreateCommand();
                    statusCmd.Transaction = transaction;
                    statusCmd.CommandText = @"
                        UPDATE Bewirtungsposition
                        SET Status = true
                        WHERE BewirtungsAuftragId = @auftragId
                        AND Bewirtungsposition = @posNr AND Menge = MengeGeliefert AND Menge = MengeBezahlt;";
                    statusCmd.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
                    statusCmd.Parameters.AddWithValue("@posNr", (int)pos.PosNr);
                    statusCmd.ExecuteNonQuery();

                    betroffeneAuftraege.Add(pos.AuftragId);
                }
                // Aufträge abschließen, deren Positionen alle voll bezahlt sind.
                foreach (var auftragId in betroffeneAuftraege)
                {
                    using var pruefCmd = connection.CreateCommand();
                    pruefCmd.Transaction = transaction;
                    pruefCmd.CommandText = @"
                        SELECT COUNT(*)
                        FROM BewirtungsPosition
                        WHERE BewirtungsauftragId = @auftragId
                          AND COALESCE(MengeBezahlt, 0) < Menge;";
                    pruefCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                    var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

                    if (offeneRest == 0)
                    {
                        using var abschlussCmd = connection.CreateCommand();
                        abschlussCmd.Transaction = transaction;
                        abschlussCmd.CommandText = @"
                            UPDATE Bewirtungsauftrag
                            SET Status = true, enddatetime = now()
                            WHERE AuftragId = @auftragId AND Status = false;";
                        abschlussCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                        if (abschlussCmd.ExecuteNonQuery() > 0)
                        {
                            abgeschlossen.Add(auftragId);
                        }
                    }
                }

                // Fiskalbeleg aus den bezahlten Teilmengen erzeugen (noch innerhalb der Transaktion lesen).
                fiskalBeleg = BaueFiskalBelegTeilzahlung(connection, transaction, req);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback(); // Alles rückgängig machen
                throw;
            }
        }
    }

    // Fiskalisierung: signierten Kassenbeleg über die TSE erzeugen und speichern.
    try
    {
        await fiskal.SpeichereBelegMitSignaturAsync(fiskalBeleg!);
        return Results.Ok(new
        {
            Abgeschlossen = abgeschlossen.Count,
            fiskalBeleg!.BelegNummer,
            Signatur = fiskalBeleg.Signatur?.Signatur,
            fiskalBeleg.Signatur?.TransaktionsNummer,
            QrCode = fiskalBeleg.Signatur?.QrCodeDaten
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            Abgeschlossen = abgeschlossen.Count,
            FiskalFehler = ex.Message
        });
    }
});

// GET /api/auslieferung/getraenke?termin=YYYYMMDD
//   -> Offene Getränke-Positionen (Preiskategorie=1, MengeGeliefert < Menge) je Tisch gruppiert.
app.MapGet("/api/auslieferung/station", (int termin, int station) =>
{
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT a.TischNummer,
               bp.BewirtungsauftragId,
               bp.Bewirtungsposition,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               bp.Menge,
               COALESCE(bp.MengeGeliefert, 0) AS MengeGeliefert,
               COALESCE(bp.Besonderheit, '') AS Besonderheit
        FROM BewirtungsPosition bp
        JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
        JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
        JOIN PreislisteTermin pt ON pt.PreislisteId = bp.PreislisteId
            AND pt.PreislisteZusatzId = bp.PreislisteZusatzId
            AND pt.Bewirtungsdatum = a.Bewirtungstermin
        WHERE a.Bewirtungstermin = @termin
          AND pt.kuechenstation = @station
          AND COALESCE(bp.MengeGeliefert, 0) < bp.Menge
        ORDER BY a.TischNummer, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("@termin", FromYyyyMmDd(termin));
    command.Parameters.AddWithValue("@station", station);

    var tische = new Dictionary<int, List<object>>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var tischNummer = reader.GetInt32(0);
            var name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var zusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var position = new
            {
                AuftragId = (long)reader.GetInt32(1),
                PosNr = (long)reader.GetInt32(2),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetInt32(5),
                MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetInt32(6),
                Besonderheit = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
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

// GET /api/auslieferung/getraenke?termin=YYYYMMDD
app.MapGet("/api/auslieferung/getraenke", (int termin) =>
{
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT a.TischNummer,
               bp.BewirtungsauftragId,
               bp.Bewirtungsposition,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               bp.Menge,
               COALESCE(bp.MengeGeliefert, 0) AS MengeGeliefert,
               COALESCE(bp.Besonderheit, '') AS Besonderheit
        FROM BewirtungsPosition bp
        JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
        JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
        WHERE a.Bewirtungstermin = @termin
          AND p.Preiskategorie = 1
          AND COALESCE(bp.MengeGeliefert, 0) < bp.Menge
        ORDER BY a.TischNummer, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("@termin", FromYyyyMmDd(termin));

    var tische = new Dictionary<int, List<object>>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var tischNummer = reader.GetInt32(0);
            var name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var zusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var position = new
            {
                AuftragId = (long)reader.GetInt32(1),
                PosNr = (long)reader.GetInt32(2),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetInt32(5),
                MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetInt32(6),
                Besonderheit = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
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
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT a.TischNummer,
               bp.BewirtungsauftragId,
               bp.Bewirtungsposition,
               p.Name,
               COALESCE(z.Bezeichnung, '') AS ZusatzText,
               bp.Menge,
               COALESCE(bp.MengeGeliefert, 0) AS MengeGeliefert,
               COALESCE(bp.Besonderheit, '') AS Besonderheit
        FROM BewirtungsPosition bp
        JOIN Bewirtungsauftrag a ON a.AuftragId = bp.BewirtungsauftragId
        JOIN GlobalePreisliste p ON p.Id = bp.PreislisteId
        LEFT JOIN GlobalePreislisteZusatz z ON z.Id = bp.PreislisteZusatzId AND z.SpeisekarteId = bp.PreislisteId
        WHERE a.Bewirtungstermin = @termin
          AND p.Preiskategorie = 0
          AND COALESCE(bp.MengeGeliefert, 0) < bp.Menge
        ORDER BY a.TischNummer, p.Name, z.Bezeichnung;";
    command.Parameters.AddWithValue("@termin", FromYyyyMmDd(termin));

    var tische = new Dictionary<int, List<object>>();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var tischNummer = reader.GetInt32(0);
            var name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var zusatzText = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            var position = new
            {
                AuftragId = (long)reader.GetInt32(1),
                PosNr = (long)reader.GetInt32(2),
                Produkt = string.IsNullOrEmpty(zusatzText) ? name : $"{name} – {zusatzText}",
                Menge = reader.IsDBNull(5) ? 0m : (decimal)reader.GetInt32(5),
                MengeGeliefert = reader.IsDBNull(6) ? 0m : (decimal)reader.GetInt32(6),
                Besonderheit = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
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
    var aktualisiert = 0;
    var betroffeneAuftraege = new HashSet<long>();
    using var connection = OpenConnection();
    {
        //using var transaction = connection.BeginTransaction();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                foreach (var pos in req.Positionen)
                {
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    // Menge bleibt unberührt; die gelieferte Menge wird um die auf der Seite
                    // erfasste Menge erhöht (nie über die bestellte Menge hinaus).
                    command.CommandText = @"
                        UPDATE BewirtungsPosition
                        SET MengeGeliefert = COALESCE(MengeGeliefert, 0) + @delta
                        WHERE BewirtungsauftragId = @auftragId AND Bewirtungsposition = @posNr;";
                    command.Parameters.AddWithValue("@delta", (int)pos.SeitenMenge);
                    command.Parameters.AddWithValue("@auftragId", (int)pos.AuftragId);
                    command.Parameters.AddWithValue("@posNr", (int)pos.PosNr);
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
                        WHERE BewirtungsauftragId = @auftragId
                          AND COALESCE(MengeGeliefert, 0) < Menge;";
                    pruefCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                    var offeneRest = Convert.ToInt32(pruefCmd.ExecuteScalar());

                    if (offeneRest == 0)
                    {
                        using var statusCmd = connection.CreateCommand();
                        statusCmd.Transaction = transaction;
                        statusCmd.CommandText = @"
                            UPDATE Bewirtungsauftrag
                            SET StatusLieferung = true
                            WHERE AuftragId = @auftragId;";
                        statusCmd.Parameters.AddWithValue("@auftragId", (int)auftragId);
                        statusCmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback(); // Alles rückgängig machen
                throw;
            }
        }
    }
    return Results.Ok(new { Aktualisiert = aktualisiert });
});

// POST /api/journal/login  -> Protokolliert einen Login (userid, eingeloggt=now, modus, termin).
app.MapPost("/api/journal/login", (JournalLoginRequest req) =>
{
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO journal (userid, eingeloggt, modus, termin)
        VALUES (@userid, @eingeloggt, @modus, @termin);";
    command.Parameters.AddWithValue("@userid", (int)req.UserId);
    command.Parameters.AddWithValue("@eingeloggt", DateTime.Now);
    command.Parameters.AddWithValue("@modus", (object?)req.Modus ?? DBNull.Value);
    command.Parameters.AddWithValue("@termin",
        req.Termin is int t ? FromYyyyMmDd(t) : (object)DBNull.Value);
    command.ExecuteNonQuery();
    return Results.Ok();
});

// POST /api/journal/logout  -> Setzt ausgeloggt=now beim letzten noch offenen Login des Benutzers.
app.MapPost("/api/journal/logout", (JournalLogoutRequest req) =>
{
    using var connection = OpenConnection();
    using var command = connection.CreateCommand();
    command.CommandText = @"
        UPDATE journal
        SET ausgeloggt = @ausgeloggt
        WHERE userid = @userid
          AND eingeloggt = (
              SELECT MAX(eingeloggt) FROM journal
              WHERE userid = @userid AND ausgeloggt IS NULL
          );";
    command.Parameters.AddWithValue("@ausgeloggt", DateTime.Now);
    command.Parameters.AddWithValue("@userid", (int)req.UserId);
    var aktualisiert = command.ExecuteNonQuery();
    return Results.Ok(new { Aktualisiert = aktualisiert });
});

app.Run();

record BestellungRequest(int Bewirtungstermin, long UserId, decimal Gesamtpreis, int TischNummer, List<BestellungPosition> Positionen);
record BestellungPosition(long PreislisteId, long ZusatzId, decimal Preis, decimal Menge, string? Besonderheit = null);
record BezahlenRequest(long AuftragId, decimal Betrag, List<long>? AuftragIds = null, string? HandyName = null);
record TeilzahlenRequest(decimal Betrag, List<TeilzahlenPosition> Positionen, string? HandyName = null);
record TeilzahlenPosition(long AuftragId, long PosNr, decimal MengeZahlen);
record AuslieferungFertigRequest(List<AuslieferungFertigPosition> Positionen);
record AuslieferungFertigPosition(long AuftragId, long PosNr, decimal SeitenMenge);
record GeliefertRequest(List<GeliefertPosition> Positionen);
record JournalLoginRequest(long UserId, int? Modus, int? Termin);
record JournalLogoutRequest(long UserId);
record GeliefertPosition(long AuftragId, long PosNr);
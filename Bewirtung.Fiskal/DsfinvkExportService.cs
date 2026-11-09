using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Bewirtung.Fiskal
{
    /// <summary>
    /// Erzeugt einen DSFinV-K-nahen Datenexport (Kern-CSV-Dateien + index.xml),
    /// verpackt als ZIP. Die erzeugte Struktur orientiert sich an der amtlichen
    /// DSFinV-K, ist aber bewusst reduziert (Kerndateien) und beruht derzeit auf
    /// simulierten TSE-Daten – daher NICHT zertifizierungs-/prüfungskonform.
    /// Vor dem Echteinsatz müssen Dateinamen, Pflichtspalten, Datentypen,
    /// die vollständige Dateiliste sowie echte TSE-Signaturen gemäß der
    /// verbindlichen DSFinV-K-Spezifikation ergänzt/geprüft werden.
    /// </summary>
    public class DsfinvkExportService
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private readonly FiskalRepository _repository;

        public DsfinvkExportService(FiskalRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Baut das Export-ZIP für den Zeitraum [von, bis) (bis exklusiv) und liefert
        /// die ZIP-Bytes zurück.
        /// </summary>
        public byte[] ErstelleExportZip(DateTime vonInklusive, DateTime bisExklusive, string? kasseId = null)
        {
            var belege = _repository.GetBelegeMitDetails(vonInklusive, bisExklusive, kasseId);

            var transactions = BaueTransactionsCsv(belege);
            var lines = BaueLinesCsv(belege);
            var vat = BaueVatCsv(belege);
            var tse = BaueTseCsv(belege);
            var cashpointclosing = BaueCashpointclosingCsv(belege, vonInklusive, bisExklusive);
            var indexXml = BaueIndexXml(vonInklusive, bisExklusive, belege.Count);

            using var speicher = new MemoryStream();
            using (var archiv = new ZipArchive(speicher, ZipArchiveMode.Create, leaveOpen: true))
            {
                SchreibeEintrag(archiv, "transactions.csv", transactions);
                SchreibeEintrag(archiv, "lines.csv", lines);
                SchreibeEintrag(archiv, "transactions_vat.csv", vat);
                SchreibeEintrag(archiv, "transactions_tse.csv", tse);
                SchreibeEintrag(archiv, "cashpointclosing.csv", cashpointclosing);
                SchreibeEintrag(archiv, "index.xml", indexXml);
                SchreibeEintrag(archiv, "HINWEIS.txt",
                    "Reduzierter DSFinV-K-naher Export mit simulierten TSE-Daten.\r\n" +
                    "NICHT zertifizierungs-/pruefungskonform. Vor Echteinsatz vollstaendige\r\n" +
                    "DSFinV-K-Dateiliste, Pflichtfelder und echte TSE-Signaturen ergaenzen.");
            }

            return speicher.ToArray();
        }

        private static void SchreibeEintrag(ZipArchive archiv, string name, string inhalt)
        {
            var eintrag = archiv.CreateEntry(name, CompressionLevel.Optimal);
            using var schreiber = new StreamWriter(eintrag.Open(), new UTF8Encoding(false));
            schreiber.Write(inhalt);
        }

        private static string BaueTransactionsCsv(List<FiskalBeleg> belege)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BON_ID;KASSE_ID;BON_NR;BON_TYP;BEDIENER;ZAHLART;START;ENDE;BRUTTO");
            foreach (var b in belege)
            {
                sb.Append(Feld(b.Id)).Append(';')
                  .Append(Feld(b.KasseId)).Append(';')
                  .Append(Feld(b.BelegNummer)).Append(';')
                  .Append(Feld(b.VorgangsTyp.ToString())).Append(';')
                  .Append(Feld(b.Bediener)).Append(';')
                  .Append(Feld(b.Zahlart == FiskalZahlart.Bar ? "Bar" : "Unbar")).Append(';')
                  .Append(Feld(DatumZeit(b.StartZeitpunkt))).Append(';')
                  .Append(Feld(DatumZeit(b.EndZeitpunkt))).Append(';')
                  .Append(Feld(Betrag(b.GesamtBrutto)))
                  .Append("\r\n");
            }
            return sb.ToString();
        }

        private static string BaueLinesCsv(List<FiskalBeleg> belege)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BON_ID;POS_NR;ARTIKEL;MENGE;EINZELPREIS_BRUTTO;GESAMT_BRUTTO;MWST_SATZ");
            foreach (var b in belege)
            {
                foreach (var p in b.Positionen)
                {
                    sb.Append(Feld(b.Id)).Append(';')
                      .Append(Feld(p.PositionsNummer)).Append(';')
                      .Append(Feld(p.Artikel)).Append(';')
                      .Append(Feld(Betrag(p.Menge))).Append(';')
                      .Append(Feld(Betrag(p.EinzelpreisBrutto))).Append(';')
                      .Append(Feld(Betrag(p.GesamtBrutto))).Append(';')
                      .Append(Feld(Betrag(p.MwstSatz)))
                      .Append("\r\n");
                }
            }
            return sb.ToString();
        }

        private static string BaueVatCsv(List<FiskalBeleg> belege)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BON_ID;STEUER_SCHLUESSEL;MWST_SATZ;BRUTTO;NETTO;STEUER");
            foreach (var b in belege)
            {
                foreach (var s in b.SteuerPositionen)
                {
                    sb.Append(Feld(b.Id)).Append(';')
                      .Append(Feld(s.SteuerSchluessel)).Append(';')
                      .Append(Feld(Betrag(s.MwstSatz))).Append(';')
                      .Append(Feld(Betrag(s.Brutto))).Append(';')
                      .Append(Feld(Betrag(s.Netto))).Append(';')
                      .Append(Feld(Betrag(s.Steuer)))
                      .Append("\r\n");
                }
            }
            return sb.ToString();
        }

        private static string BaueTseCsv(List<FiskalBeleg> belege)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BON_ID;TSE_SERIENNUMMER;TRANSAKTIONSNUMMER;SIGNATURZAEHLER;SIGNATURALGORITHMUS;TSE_START;TSE_ENDE;PROZESSTYP;SIGNATUR");
            foreach (var b in belege)
            {
                var g = b.Signatur;
                if (g is null)
                {
                    continue;
                }
                sb.Append(Feld(b.Id)).Append(';')
                  .Append(Feld(g.TseSerienNummer)).Append(';')
                  .Append(Feld(g.TransaktionsNummer)).Append(';')
                  .Append(Feld(g.SignaturZaehler)).Append(';')
                  .Append(Feld(g.SignaturAlgorithmus)).Append(';')
                  .Append(Feld(DatumZeit(g.TseStartZeit))).Append(';')
                  .Append(Feld(DatumZeit(g.TseEndZeit))).Append(';')
                  .Append(Feld(g.ProzessTyp)).Append(';')
                  .Append(Feld(g.Signatur))
                  .Append("\r\n");
            }
            return sb.ToString();
        }

        private static string BaueCashpointclosingCsv(List<FiskalBeleg> belege, DateTime von, DateTime bis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("KASSE_ID;ERSTER_BELEG;LETZTER_BELEG;SUMME_BRUTTO;SUMME_BAR;SUMME_UNBAR;VON;BIS");
            foreach (var gruppe in belege.GroupBy(b => b.KasseId))
            {
                var erste = gruppe.Min(b => b.BelegNummer);
                var letzte = gruppe.Max(b => b.BelegNummer);
                var summeBrutto = gruppe.Sum(b => b.GesamtBrutto);
                var summeBar = gruppe.Where(b => b.Zahlart == FiskalZahlart.Bar).Sum(b => b.GesamtBrutto);
                var summeUnbar = gruppe.Where(b => b.Zahlart == FiskalZahlart.Unbar).Sum(b => b.GesamtBrutto);

                sb.Append(Feld(gruppe.Key)).Append(';')
                  .Append(Feld(erste)).Append(';')
                  .Append(Feld(letzte)).Append(';')
                  .Append(Feld(Betrag(summeBrutto))).Append(';')
                  .Append(Feld(Betrag(summeBar))).Append(';')
                  .Append(Feld(Betrag(summeUnbar))).Append(';')
                  .Append(Feld(DatumZeit(von))).Append(';')
                  .Append(Feld(DatumZeit(bis)))
                  .Append("\r\n");
            }
            return sb.ToString();
        }

        private static string BaueIndexXml(DateTime von, DateTime bis, int anzahlBelege)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<DataSet>");
            sb.AppendLine("  <Media>");
            sb.AppendLine("    <Name>Bewirtung DSFinV-K Export (reduziert)</Name>");
            sb.AppendLine($"    <ZeitraumVon>{DatumZeit(von)}</ZeitraumVon>");
            sb.AppendLine($"    <ZeitraumBis>{DatumZeit(bis)}</ZeitraumBis>");
            sb.AppendLine($"    <AnzahlBelege>{anzahlBelege}</AnzahlBelege>");
            sb.AppendLine("    <Tables>");
            foreach (var datei in new[] { "transactions.csv", "lines.csv", "transactions_vat.csv", "transactions_tse.csv", "cashpointclosing.csv" })
            {
                sb.AppendLine($"      <Table><URL>{datei}</URL></Table>");
            }
            sb.AppendLine("    </Tables>");
            sb.AppendLine("    <Hinweis>Reduzierter Export mit simulierten TSE-Daten. Nicht pruefungskonform.</Hinweis>");
            sb.AppendLine("  </Media>");
            sb.AppendLine("</DataSet>");
            return sb.ToString();
        }

        private static string DatumZeit(DateTime wert) =>
            wert == default || wert == DateTime.MinValue
                ? string.Empty
                : wert.ToString("yyyy-MM-ddTHH:mm:ss", Inv);

        private static string Betrag(decimal wert) => wert.ToString("0.00", Inv);

        /// <summary>CSV-Feld maskieren (Semikolon/Anführungszeichen/Zeilenumbruch).</summary>
        private static string Feld(object? wert)
        {
            var text = wert?.ToString() ?? string.Empty;
            if (text.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0)
            {
                text = "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }
    }
}

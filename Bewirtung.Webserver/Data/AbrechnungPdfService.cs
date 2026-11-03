using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Erzeugt aus aggregierten Abrechnungszeilen ein Excel-artiges DOCX
    /// (Headline + Tabelle + Summenzeile) für die Tagesabrechnung.
    /// </summary>
    public class AbrechnungPdfService
    {
        /// <summary>Art des zu erzeugenden Dokuments.</summary>
        public enum ReportArt
        {
            Tagesumsaetze,
            RechnungAnVerein,
            Einkaufsrechnung
        }

        /// <summary>
        /// Baut das DOCX-Dokument. Die Spalten und Werte richten sich nach <paramref name="art"/>.
        /// </summary>
        public byte[] Erstellen(ReportArt art, string titel, string termin, string verein, IEnumerable<AbrechnungZeile> zeilen)
        {
            var liste = zeilen.ToList();

            string preisHeader;
            string mwstHeader;
            Func<AbrechnungZeile, decimal> preisWahl;
            Func<AbrechnungZeile, decimal> mwstWahl;

            switch (art)
            {
                case ReportArt.RechnungAnVerein:
                    preisHeader = "Verkaufspreis";
                    mwstHeader = "Mwst aus Verkaufspreis";
                    preisWahl = z => z.Gesamtpreis;
                    mwstWahl = z => BerechneMwst(z.Gesamtpreis, z.MwstSatz);
                    break;
                case ReportArt.Einkaufsrechnung:
                    preisHeader = "Einkaufspreis";
                    mwstHeader = "Mwst aus Einkaufspreis";
                    preisWahl = z => z.Einkaufspreis;
                    mwstWahl = z => BerechneMwst(z.Einkaufspreis, z.MwstSatz);
                    break;
                default:
                    preisHeader = "Gesamtpreis";
                    mwstHeader = "Mwst";
                    preisWahl = z => z.Gesamtpreis;
                    mwstWahl = z => z.Mwst;
                    break;
            }

            using var ms = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var speisen = liste.Where(z => z.Preiskategorie == 0).ToList();
                var getraenke = liste.Where(z => z.Preiskategorie == 1).ToList();

                // Seite 1: Speisen
                SeiteAufbauen(body, titel, termin, verein, "Speisen", speisen,
                    preisHeader, mwstHeader, preisWahl, mwstWahl);

                // Seitenumbruch
                body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

                // Seite 2: Getränke
                SeiteAufbauen(body, titel, termin, verein, "Getränke", getraenke,
                    preisHeader, mwstHeader, preisWahl, mwstWahl);

                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        private static void SeiteAufbauen(Body body, string titel, string termin, string verein, string kategorie,
            List<AbrechnungZeile> zeilen, string preisHeader, string mwstHeader,
            Func<AbrechnungZeile, decimal> preisWahl, Func<AbrechnungZeile, decimal> mwstWahl)
        {
            // Headline: Titel, Termin, Verein, Kategorie
            body.AppendChild(AbsatzUeberschrift(titel, 28));
            body.AppendChild(AbsatzUeberschrift($"Termin: {termin}", 22));
            body.AppendChild(AbsatzUeberschrift($"Verein: {verein}", 22));
            body.AppendChild(AbsatzUeberschrift(kategorie, 24));
            body.AppendChild(new Paragraph());

            var tabelle = new Table();
            tabelle.AppendChild(TabellenEigenschaften());

            // Kopfzeile
            tabelle.AppendChild(TabellenZeile(true,
                "Artikel", "Menge", preisHeader, mwstHeader));

            decimal summeMenge = 0m;
            decimal summePreis = 0m;
            decimal summeMwst = 0m;

            foreach (var z in zeilen)
            {
                var preis = preisWahl(z);
                var mwst = mwstWahl(z);
                summeMenge += z.Menge;
                summePreis += preis;
                summeMwst += mwst;

                tabelle.AppendChild(TabellenZeile(false,
                    z.Artikel,
                    z.Menge.ToString("0.##"),
                    preis.ToString("N2"),
                    mwst.ToString("N2")));
            }

            // Summenzeile
            tabelle.AppendChild(TabellenZeile(true,
                "Summen",
                summeMenge.ToString("0.##"),
                summePreis.ToString("N2"),
                summeMwst.ToString("N2")));

            body.AppendChild(tabelle);
        }

        private static decimal BerechneMwst(decimal betrag, decimal satz)
            => satz > 0m ? betrag - (betrag * 100m / (100m + satz)) : 0m;

        private static Paragraph AbsatzUeberschrift(string text, int halbpunkt)
        {
            var run = new Run();
            run.AppendChild(new RunProperties(new Bold(), new FontSize { Val = halbpunkt.ToString() }));
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            return new Paragraph(run);
        }

        private static TableProperties TabellenEigenschaften()
        {
            return new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct });
        }

        private static TableRow TabellenZeile(bool fett, params string[] werte)
        {
            var zeile = new TableRow();
            for (var i = 0; i < werte.Length; i++)
            {
                var run = new Run();
                var runProps = new RunProperties();
                if (fett)
                {
                    runProps.AppendChild(new Bold());
                }
                run.AppendChild(runProps);
                run.AppendChild(new Text(werte[i] ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });

                var absatzProps = new ParagraphProperties();
                // Erste Spalte linksbündig, Rest rechtsbündig.
                if (i > 0)
                {
                    absatzProps.AppendChild(new Justification { Val = JustificationValues.Right });
                }

                var zelle = new TableCell(new Paragraph(absatzProps, run));
                zeile.AppendChild(zelle);
            }
            return zeile;
        }
    }
}

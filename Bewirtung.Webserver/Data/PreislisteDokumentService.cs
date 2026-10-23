using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Befüllt die in der Datenbank gespeicherte Word-Vorlage (Tabelle wordvorlage)
    /// mit den Positionen einer Speisekarte und liefert das fertige .docx als Bytes.
    /// </summary>
    public class PreislisteDokumentService
    {
        private readonly WordVorlageRepository _vorlageRepo;

        public PreislisteDokumentService(WordVorlageRepository vorlageRepo)
        {
            _vorlageRepo = vorlageRepo;
        }

        public bool VorlageVorhanden()
        {
            var vorlage = _vorlageRepo.Get();
            return vorlage != null && vorlage.Inhalt.Length > 0;
        }

        /// <summary>
        /// Erzeugt aus der DB-Vorlage ein befülltes Dokument. Gibt null zurück,
        /// wenn keine Vorlage in der Datenbank vorhanden ist.
        /// </summary>
        public byte[]? Erstellen(IEnumerable<SpeisekartenZeile> zeilen, string druckTitel)
        {
            var vorlage = _vorlageRepo.Get();
            if (vorlage == null || vorlage.Inhalt.Length == 0)
            {
                return null;
            }

            var speisen = zeilen
                .Where(z => z.KategorieText == "Speisen")
                .ToList();
            var getraenke = zeilen
                .Where(z => z.KategorieText == "Getränke")
                .ToList();

            using var ms = new MemoryStream();
            ms.Write(vorlage.Inhalt, 0, vorlage.Inhalt.Length);
            ms.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(ms, true))
            {
                var body = wordDoc.MainDocumentPart?.Document?.Body;
                if (body == null)
                {
                    return null;
                }

                foreach (var textElement in body.Descendants<Text>())
                {
                    if (textElement.Text.Contains("Termin"))
                    {
                        textElement.Text = textElement.Text.Replace("Termin", druckTitel);
                    }
                    if (textElement.Text.Contains("Verein"))
                    {
                        textElement.Text = textElement.Text.Replace("Verein", druckTitel);
                    }
                }

                ZeilenEinfuegen(body, "SpeisenArtikelSpeisenPreis", "SpeisenArtikel", "SpeisenPreis", speisen);
                ZeilenEinfuegen(body, "GetränkeArtikelGetränkePreis", "GetränkeArtikel", "GetränkePreis", getraenke);

                wordDoc.MainDocumentPart!.Document.Save();
            }

            return ms.ToArray();
        }

        private static void ZeilenEinfuegen(Body body, string musterText, string artikelPlatzhalter, string preisPlatzhalter, List<SpeisekartenZeile> positionen)
        {
            TableRow? musterZeile = body.Descendants<TableRow>().FirstOrDefault(tr =>
                string.Concat(tr.Descendants<Text>().Select(t => t.Text)).Contains(musterText));

            if (musterZeile == null)
            {
                return;
            }

            var tabelle = (Table)musterZeile.Parent!;
            foreach (var pos in positionen)
            {
                var neueZeile = (TableRow)musterZeile.CloneNode(true);
                ErsetzeInElement(neueZeile, artikelPlatzhalter, pos.Produkt);
                ErsetzeInElement(neueZeile, preisPlatzhalter, pos.TerminPreis.ToString("N2"));
                tabelle.InsertBefore(neueZeile, musterZeile);
            }
            musterZeile.Remove();
        }

        private static void ErsetzeInElement(OpenXmlElement element, string platzhalter, string ersatzText)
        {
            foreach (var textElement in element.Descendants<Text>())
            {
                if (textElement.Text.Contains(platzhalter))
                {
                    textElement.Text = textElement.Text.Replace(platzhalter, ersatzText);
                }
            }
        }
    }
}

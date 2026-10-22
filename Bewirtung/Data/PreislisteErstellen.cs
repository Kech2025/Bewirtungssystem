using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Bewirtung.Data
{
    public class Position
    {
        public string Artikel { get; set; }
        public string Preis { get; set; }
        public bool DruckParent { get; set; }
        public bool DruckClient { get; set; }
    }
    public static class PreislisteErstellen
    {
        public static void KopierenInPDF(List<Position> getränke, List<Position> speisen, string druckTitel)
        {
            string path = "E:\\Rathaus\\Projekt\\data\\";
            string vorlagePfad = Path.Combine(path, "Preisliste_Vorlage.docx");
            string ausgabePfad = Path.Combine(path, "Preisliste_fertig.docx");
            if (File.Exists(ausgabePfad))
            {
                File.Delete(ausgabePfad);
            }
            if (File.Exists(ausgabePfad.Substring(0, ausgabePfad.Length - 4) + "PDF"))
            {
                File.Delete(ausgabePfad.Substring(0, ausgabePfad.Length - 4) + "PDF");
            }
            File.Copy(vorlagePfad, ausgabePfad, true);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(ausgabePfad, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

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
                // 1. Finde die Tabellenzeile (TableRow), die unseren Platzhalter enthält
                TableRow musterZeile = body.Descendants<TableRow>().FirstOrDefault(t =>
                {
                    // Holt den gesamten, kombinierten Text der aktuellen Zeile
                    string zeilenText = string.Concat(t.Descendants<Text>().Select(t => t.Text));
                    return zeilenText.Contains("SpeisenArtikelSpeisenPreis");
                });

                if (musterZeile != null)
                {
                    // Die Tabelle ermitteln, in der sich die Zeile befindet
                    Table tabelle = (Table)musterZeile.Parent;

                    // 2. Für jede Position eine neue Zeile basierend auf dem Muster erstellen
                    foreach (var pos in speisen)
                    {
                        // Klone die Musterzeile inklusive aller Formatierungen
                        TableRow neueZeile = (TableRow)musterZeile.CloneNode(true);

                        // Platzhalter in der neuen Zeile ersetzen
                        ErsetzeInElement(neueZeile, "SpeisenArtikel", pos.Artikel);
                        ErsetzeInElement(neueZeile, "SpeisenPreis", pos.Preis);
                        // Füge die neue Zeile direkt vor der Musterzeile ein
                        tabelle.InsertBefore(neueZeile, musterZeile);
                    }
                    musterZeile.Remove();

                    musterZeile = body.Descendants<TableRow>().FirstOrDefault(tr => tr.InnerText.Contains("GetränkeArtikelGetränkePreis"));
                    if (musterZeile != null)
                    {
                        tabelle = (Table)musterZeile.Parent;
                        foreach (var pos in getränke)
                        {
                            // Klone die Musterzeile inklusive aller Formatierungen
                            TableRow neueZeile = (TableRow)musterZeile.CloneNode(true);

                            // Platzhalter in der neuen Zeile ersetzen
                            ErsetzeInElement(neueZeile, "GetränkeArtikel", pos.Artikel);
                            ErsetzeInElement(neueZeile, "GetränkePreis", pos.Preis);

                            // Füge die neue Zeile direkt vor der Musterzeile ein
                            tabelle.InsertBefore(neueZeile, musterZeile);
                        }
                    }
                    // 3. Die leere Musterzeile aus der Tabelle entfernen
                    musterZeile.Remove();
                }
                //string titel = body.Descendants<BookmarkStart>().FirstOrDefault(tr => tr.InnerText.Contains("[RANDLEISTENTITEL]"));
                

                wordDoc.MainDocumentPart.Document.Save();
            }
            string libreOfficePfad = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libreoffice", "program", "soffice.exe");
            //string libreOfficePfad = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "program", "soffice.exe");
            // 1. PDF im Hintergrund generieren
            string nurPfad = Path.GetDirectoryName(ausgabePfad);
            ProcessStartInfo convertInfo = new ProcessStartInfo
            {
                FileName = libreOfficePfad,
                // --headless sorgt dafür, dass kein LibreOffice-Fenster aufflackert
                Arguments = $"--headless --convert-to pdf --outdir \"{nurPfad}\" \"{ausgabePfad}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (Process proc = Process.Start(convertInfo))
            {
                proc.WaitForExit(); // Warten, bis die Konvertierung fertig ist
            }
            if (File.Exists(Path.Combine(nurPfad, "Preisliste_fertig.pdf")))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(nurPfad, "Preisliste_fertig.pdf"),
                    UseShellExecute = true
                });
            }
            Console.WriteLine("Tabelle erfolgreich befüllt!");
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

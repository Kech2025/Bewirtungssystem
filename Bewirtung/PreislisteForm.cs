using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using Bewirtung.Data;
using DocumentFormat.OpenXml.Office.Word;

namespace Bewirtung
{
    /// <summary>
    /// Zeigt und druckt die Preisliste eines Termins als flache Liste mit Preis.
    /// Oben links eine Combobox zur Auswahl von Termin/Verein, ein Button zum Drucken.
    /// </summary>
    public partial class PreislisteForm : Form
    {
        private readonly TerminRepository _terminRepository = new();
        private readonly SpeisekarteRepository _speisekarteRepository = new();

        private List<SpeisekartenZeile> _zeilen = new();

        // Druck-Status: eine Seitengruppe je Kategorie (Seite 1 = Speisen, Seite 2 = Getränke)
        private readonly List<(string Titel, List<SpeisekartenZeile> Zeilen)> _druckSeiten = new();
        private int _druckSeiteIndex;
        private int _druckIndex;
        private string _druckTitel = string.Empty;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int TerminPreisliste { get; set; } = 0;

        public PreislisteForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            ConfigureGrid();
            cmbTermine.SelectedIndexChanged += CmbTermine_SelectedIndexChanged;
            Load += PreislisteForm_Load;
        }

        private void ConfigureGrid()
        {
            gridPreisliste.AutoGenerateColumns = false;
            gridPreisliste.Columns.Clear();
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.KategorieText),
                HeaderText = "Kategorie",
                Width = 140
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Produkt),
                HeaderText = "Position",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.TerminPreis),
                HeaderText = "Preis",
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120
            });
        }

        private void PreislisteForm_Load(object? sender, EventArgs e)
        {
            LoadTermine();
            if (TerminPreisliste != 0)
            {
                var termine = _terminRepository.GetAll();
                var termin = termine.FirstOrDefault(t => t.BewirtungsDatum == TerminPreisliste);

                if (termin != null)
                {
                    foreach (var item in cmbTermine.Items)
                    {
                        if (item is Termin t && t.BewirtungsDatum == TerminPreisliste)
                        {
                            cmbTermine.SelectedItem = item;
                            Application.DoEvents(); // Ensure the selection change is processed before printing
                            System.Threading.Thread.Sleep(1000); // Optional: Add a small delay to ensure the UI updates
                            Application.DoEvents();
                            btnDrucken_Click(sender, e);
                            break;
                        }
                    }
                }
            }
        }

        private void LoadTermine()
        {
            var termine = _terminRepository.GetAll();
            _terminRepository.LoadTerminCombobox(cmbTermine, termine);
        }

        private void CmbTermine_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadPreisliste();
        }

        private void LoadPreisliste()
        {
            if (cmbTermine.SelectedItem is not Termin termin)
            {
                _zeilen = new List<SpeisekartenZeile>();
                gridPreisliste.DataSource = null;
                return;
            }

            _zeilen = _speisekarteRepository
                .GetSpeisekarte(termin.BewirtungsDatum, termin.VereinId)
                .Where(z => z.Ausgewaehlt)
                .ToList();
            gridPreisliste.DataSource = new BindingSource { DataSource = _zeilen };
        }

        private void btnDrucken_Click(object? sender, EventArgs e)
        {
                       
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
                if (cmbTermine.SelectedItem is not Termin termin)
                {
                    MessageBox.Show("Bitte einen Termin auswählen.", "Preisliste",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }


                if (_zeilen.Count == 0)
                {
                    MessageBox.Show("Für diesen Termin sind keine Positionen vorhanden.", "Preisliste",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                List<SpeisekartenZeile> zeilen = new List<SpeisekartenZeile>();
                long lastDruckParent = -1;
                foreach (var zeile in _zeilen)
                {
                    if(zeile.PreislisteId == lastDruckParent)
                    {
                        continue;
                    }
                    if (zeile.DruckClient == 1)
                    {                                          
                        zeilen.Add(zeile);
                        lastDruckParent = -1;
                    }
                    else if (zeile.DruckParent == 1)
                    {
                        zeilen.Add(zeile);
                        lastDruckParent = zeile.PreislisteId;
                    }
                }
                _druckTitel = $"Preisliste – {termin.Anzeige}";

                PreislisteErstellen.KopierenInPDF(
                    zeilen.Where(IstGetraenk).Select(z => new Position { Artikel = z.DruckClient == 1 ? z.ZusatzText : z.Produkt, Preis = z.TerminPreis.ToString("N2"), DruckParent = z.DruckParent == 1, DruckClient = z.DruckClient == 1 }).ToList(),
                    zeilen.Where(z => !IstGetraenk(z)).Select(z => new Position { Artikel = z.DruckClient == 1 ? z.ZusatzText : z.Produkt, Preis = z.TerminPreis.ToString("N2"), DruckParent = z.DruckParent == 1, DruckClient = z.DruckClient == 1 }).ToList(), _druckTitel);
                if(TerminPreisliste != 0)
                {
                    timer1.Enabled = true;
                }
                
                return;

            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private static bool IstGetraenk(SpeisekartenZeile zeile) =>
            string.Equals(zeile.KategorieText, "Getränke", StringComparison.OrdinalIgnoreCase);

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            var bounds = e.MarginBounds;

            using var titelFont = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var kopfFont = new Font("Segoe UI", 10f, FontStyle.Bold);
            using var zeileFont = new Font("Segoe UI", 10f, FontStyle.Regular);

            float y = bounds.Top;
            float left = bounds.Left;
            float right = bounds.Right;

            var seite = _druckSeiten[_druckSeiteIndex];

            // Titel inkl. Kategorie der Seite
            g.DrawString($"{_druckTitel} – {seite.Titel}", titelFont, Brushes.Black, left, y);
            y += titelFont.GetHeight(g) + 10f;

            // Kopfzeile
            g.DrawString("Kategorie", kopfFont, Brushes.Black, left, y);
            g.DrawString("Position", kopfFont, Brushes.Black, left + 140f, y);
            var preisKopf = "Preis";
            var preisKopfBreite = g.MeasureString(preisKopf, kopfFont).Width;
            g.DrawString(preisKopf, kopfFont, Brushes.Black, right - preisKopfBreite, y);
            y += kopfFont.GetHeight(g) + 4f;
            g.DrawLine(Pens.Black, left, y, right, y);
            y += 6f;

            float zeilenHoehe = zeileFont.GetHeight(g) + 6f;
            var zeilen = seite.Zeilen;

            while (_druckIndex < zeilen.Count)
            {
                if (y + zeilenHoehe > bounds.Bottom)
                {
                    // Aktuelle Kategorie passt nicht komplett auf die Seite -> Folgeseite.
                    e.HasMorePages = true;
                    return;
                }

                var zeile = zeilen[_druckIndex];
                g.DrawString(zeile.KategorieText, zeileFont, Brushes.Black, left, y);
                g.DrawString(zeile.Produkt, zeileFont, Brushes.Black, left + 140f, y,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

                var preisText = zeile.TerminPreis.ToString("N2");
                var preisBreite = g.MeasureString(preisText, zeileFont).Width;
                g.DrawString(preisText, zeileFont, Brushes.Black, right - preisBreite, y);

                y += zeilenHoehe;
                _druckIndex++;
            }

            // Kategorie fertig -> nächste Seite/Kategorie beginnen.
            _druckSeiteIndex++;
            _druckIndex = 0;
            e.HasMorePages = _druckSeiteIndex < _druckSeiten.Count;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Close();
        }
    }
}

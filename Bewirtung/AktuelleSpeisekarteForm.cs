using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Aktuelle Speisekarte für einen Termin.
    /// Oben ein Termin-Auswahlfeld, links die globale Speisekarte (inkl. Zusatz-Varianten)
    /// mit Auswahl-Checkbox und Preis aus GlobalerPreis, rechts die ausgewählten Positionen
    /// mit überschreibbarem Preis. Speichert in die Tabelle PreislisteTermin.
    /// </summary>
    public partial class AktuelleSpeisekarteForm : Form
    {
        private readonly TerminRepository _terminRepository = new();
        private readonly SpeisekarteRepository _speisekarteRepository = new();

        private List<SpeisekartenZeile> _zeilen = new();
        private bool _laden;

        public AktuelleSpeisekarteForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            ConfigureGlobalGrid();
            ConfigureAuswahlGrid();
            GridSortHelper.EnableHeaderSorting(gridGlobal);
            GridSortHelper.EnableHeaderSorting(gridAuswahl);
            Load += AktuelleSpeisekarteForm_Load;
        }

        private void ConfigureGlobalGrid()
        {
            gridGlobal.AutoGenerateColumns = false;
            gridGlobal.Columns.Clear();
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Produkt),
                HeaderText = "Produkt",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.ZusatzText),
                HeaderText = "Zusatz",
                ReadOnly = true,
                Width = 160
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.KategorieText),
                HeaderText = "Kategorie",
                ReadOnly = true,
                Width = 120
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.GlobalPreis),
                HeaderText = "Globaler Preis",
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 130
            });
            gridGlobal.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Ausgewaehlt),
                HeaderText = "Auswahl",
                Name = "Ausgewaehlt",
                Width = 90
            });
        }

        private void ConfigureAuswahlGrid()
        {
            gridAuswahl.AutoGenerateColumns = false;
            gridAuswahl.Columns.Clear();
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.KategorieText),
                HeaderText = "Kategorie",
                ReadOnly = true,
                Width = 120
            });
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Produkt),
                HeaderText = "Position",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.TerminPreis),
                HeaderText = "Preis",
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120
            });
        }

        private void AktuelleSpeisekarteForm_Load(object? sender, EventArgs e)
        {
            LoadTermine();
        }

        private void LoadTermine()
        {
            var termine = _terminRepository.GetAll();
            _terminRepository.LoadTerminCombobox(cmbTermine, termine);
        }

        private void CmbTermine_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadSpeisekarte();
        }

        private void LoadSpeisekarte()
        {
            if (cmbTermine.SelectedItem is not Termin termin)
            {
                _zeilen = new List<SpeisekartenZeile>();
                gridGlobal.DataSource = null;
                gridAuswahl.DataSource = null;
                return;
            }

            _laden = true;
            _zeilen = _speisekarteRepository.GetSpeisekarte(termin.BewirtungsDatum, termin.VereinId);
            gridGlobal.DataSource = new BindingSource { DataSource = new SortableBindingList<SpeisekartenZeile>(_zeilen) };
            _laden = false;
            RefreshAuswahl();
        }

        private void GridGlobal_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (gridGlobal.IsCurrentCellDirty && gridGlobal.CurrentCell is DataGridViewCheckBoxCell)
            {
                gridGlobal.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void GridGlobal_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_laden || e.RowIndex < 0)
            {
                return;
            }

            if (gridGlobal.Columns[e.ColumnIndex].Name == "Ausgewaehlt")
            {
                // Beim Anhaken den Terminpreis mit dem globalen Preis vorbelegen,
                // falls noch kein (eigener) Preis gesetzt ist.
                if (e.RowIndex < _zeilen.Count)
                {
                    var zeile = gridGlobal.Rows[e.RowIndex].DataBoundItem as SpeisekartenZeile;
                    if (zeile is { Ausgewaehlt: true } && zeile.TerminPreis <= 0m)
                    {
                        zeile.TerminPreis = zeile.GlobalPreis ?? 0m;
                    }
                }

                RefreshAuswahl();
            }
        }

        private void RefreshAuswahl()
        {
            var ausgewaehlt = _zeilen.Where(z => z.Ausgewaehlt).ToList();
            gridAuswahl.DataSource = new BindingSource { DataSource = new SortableBindingList<SpeisekartenZeile>(ausgewaehlt) };
            cmdDrucken.Enabled = gridAuswahl.RowCount > 0;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (cmbTermine.SelectedItem is not Termin termin)
            {
                MessageBox.Show("Bitte zuerst einen Termin auswählen.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            gridAuswahl.EndEdit();
            gridGlobal.EndEdit();

            _speisekarteRepository.Speichern(termin.BewirtungsDatum, termin.VereinId, _zeilen);
            MessageBox.Show("Die Speisekarte für den Termin wurde gespeichert.", "Gespeichert",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cmdDrucken_Click(object sender, EventArgs e)
        {
            if (cmbTermine.SelectedItem is not Termin termin)
            {
                MessageBox.Show("Bitte zuerst einen Termin auswählen.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var existing = MdiChildren.OfType<PreislisteForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new PreislisteForm { MdiParent = this.ParentForm };
            form.TerminPreisliste = termin.BewirtungsDatum;
            form.Show();
        }
    }
}

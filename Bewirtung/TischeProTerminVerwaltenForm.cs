using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Verwaltung der Tisch-Termin-Zuordnungen (Tabelle TischTermin): Neu und Bearbeiten
    /// über Popup-Dialog, Löschen über Button.
    /// </summary>
    public partial class TischeProTerminVerwaltenForm : Form
    {
        private readonly TischTerminRepository _repository = new();
        private readonly TerminRepository _terminRepository = new();
        private readonly TischRepository _tischRepository = new();
        private readonly BindingSource _bindingSource = new();
        private List<TischTermin> _eintraege = new();
        

        public TischeProTerminVerwaltenForm()
        {
            InitializeComponent();
            Load += TischeProTerminVerwaltenForm_Load;
        }

        private void TischeProTerminVerwaltenForm_Load(object? sender, EventArgs e)
        {
            dataGridViewTischTermin.AutoGenerateColumns = false;
            dataGridViewTischTermin.Columns.Clear();
            dataGridViewTischTermin.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.DatumText),
                HeaderText = "Datum",
                Width = 140
            });
            dataGridViewTischTermin.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.Tischnummer),
                HeaderText = "Tischnummer",
                Width = 160
            });
            dataGridViewTischTermin.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.BereichName),
                HeaderText = "Bereich",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTischTermin.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewTischTermin);

            LoadFilterTermine();
            LoadData();
        }

        private void LoadFilterTermine()
        {
            var termine = _terminRepository.GetAll();

            cmbFilterTermin.SelectedIndexChanged -= cmbFilterTermin_SelectedIndexChanged;
            cmbFilterTermin.DisplayMember = nameof(Termin.Anzeige);
            cmbFilterTermin.Items.Clear();
            foreach (var termin in termine)
            {
                cmbFilterTermin.Items.Add(termin);
            }
            cmbFilterTermin.SelectedIndex = 0;
            cmbFilterTermin.SelectedIndexChanged += cmbFilterTermin_SelectedIndexChanged;
        }

        private void cmbFilterTermin_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<TischTermin> gefiltert = _eintraege;
            if (cmbFilterTermin.SelectedItem is Termin termin)
            {
                gefiltert = _eintraege.Where(
                    x => x.Bewirtungsdatum == termin.BewirtungsDatum && x.VereinId == termin.VereinId);
            }

            _bindingSource.DataSource = new SortableBindingList<TischTermin>(gefiltert.ToList());
            if (dataGridViewTischTermin.Rows.Count > 0)
            {
                dataGridViewTischTermin.ClearSelection();
                dataGridViewTischTermin.Rows[0].Selected = true;
            }
        }

        private void LoadData()
        {
            _eintraege = _repository.GetAll();
            ApplyFilter();
        }

        private TischTermin? SelectedEintrag =>
            dataGridViewTischTermin.SelectedRows.Count > 0
                ? dataGridViewTischTermin.SelectedRows[0].DataBoundItem as TischTermin
                : null;

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new TischTerminBearbeitenDialog(
                _terminRepository.GetAll(), _tischRepository.GetAll());
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            int eingefuegt = 0;
            int uebersprungen = 0;
            foreach (var tischnummer in dialog.AusgewaehlteTischnummern)
            {
                var neu = new TischTermin
                {
                    Bewirtungsdatum = dialog.Eintrag.Bewirtungsdatum,
                    VereinId = dialog.Eintrag.VereinId,
                    Tischnummer = tischnummer
                };
                try
                {
                    _repository.Insert(neu);
                    eingefuegt++;
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    uebersprungen++;
                }
            }

            if (uebersprungen > 0)
            {
                MessageBox.Show(
                    $"{eingefuegt} Tisch(e) zugeordnet, {uebersprungen} bereits vorhanden.",
                    "Tische pro Termin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LoadData();
            SelectEintrag(dialog.Eintrag.Bewirtungsdatum, dialog.Eintrag.VereinId, dialog.Eintrag.Tischnummer);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null)
            {
                MessageBox.Show("Bitte zuerst eine Zuordnung zum Ändern auswählen.", "Tische pro Termin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var altDatum = eintrag.Bewirtungsdatum;
            var altVereinId = eintrag.VereinId;

            // Alle bereits zugeordneten Tische dieses Termins ermitteln und vorab anhaken.
            var vorhandene = _eintraege
                .Where(x => x.Bewirtungsdatum == altDatum && x.VereinId == altVereinId)
                .Select(x => x.Tischnummer)
                .ToHashSet();

            using var dialog = new TischTerminBearbeitenDialog(
                _terminRepository.GetAll(), _tischRepository.GetAll(),
                new TischTermin
                {
                    Bewirtungsdatum = altDatum,
                    VereinId = altVereinId,
                    Tischnummer = eintrag.Tischnummer
                },
                vorhandene);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var neuDatum = dialog.Eintrag.Bewirtungsdatum;
            var neuVereinId = dialog.Eintrag.VereinId;
            var neueAuswahl = dialog.AusgewaehlteTischnummern.ToHashSet();

            try
            {
                // Falls der Termin unverändert blieb: Diff bilden (hinzufügen / entfernen).
                if (neuDatum == altDatum && neuVereinId == altVereinId)
                {
                    foreach (var nummer in vorhandene.Except(neueAuswahl))
                    {
                        _repository.Delete(altDatum, altVereinId, nummer);
                    }
                    foreach (var nummer in neueAuswahl.Except(vorhandene))
                    {
                        _repository.Insert(new TischTermin
                        {
                            Bewirtungsdatum = neuDatum,
                            VereinId = neuVereinId,
                            Tischnummer = nummer
                        });
                    }
                }
                else
                {
                    // Termin gewechselt: alte Zuordnungen entfernen, neue anlegen.
                    foreach (var nummer in vorhandene)
                    {
                        _repository.Delete(altDatum, altVereinId, nummer);
                    }
                    foreach (var nummer in neueAuswahl)
                    {
                        _repository.Insert(new TischTermin
                        {
                            Bewirtungsdatum = neuDatum,
                            VereinId = neuVereinId,
                            Tischnummer = nummer
                        });
                    }
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                MessageBox.Show("Dieser Tisch ist diesem Termin bereits zugeordnet.",
                    "Tische pro Termin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadData();
            SelectEintrag(dialog.Eintrag.Bewirtungsdatum, dialog.Eintrag.VereinId, dialog.Eintrag.Tischnummer);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Tisch {eintrag.Tischnummer} von Termin {eintrag.TerminAnzeige} wirklich entfernen?",
                "Tische pro Termin", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(eintrag.Bewirtungsdatum, eintrag.VereinId, eintrag.Tischnummer);
            LoadData();
        }

        private void dataGridViewTischTermin_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void SelectEintrag(int datum, long vereinId, int tischnummer)
        {
            for (int i = 0; i < _eintraege.Count; i++)
            {
                if (_eintraege[i].Bewirtungsdatum == datum
                    && _eintraege[i].VereinId == vereinId
                    && _eintraege[i].Tischnummer == tischnummer)
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }
    }
}

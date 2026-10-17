using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Verwaltung der Tische (Tabelle Tisch): Neu und Bearbeiten über Popup-Dialog,
    /// Löschen über Button.
    /// </summary>
    public partial class TischeVerwaltenForm : Form
    {
        private readonly TischRepository _repository = new();
        private readonly BereichTischeRepository _bereichRepository = new();
        private readonly BindingSource _bindingSource = new();
        private List<Tisch> _tische = new();

        public TischeVerwaltenForm()
        {
            InitializeComponent();
            Load += TischeVerwaltenForm_Load;
        }

        private void TischeVerwaltenForm_Load(object? sender, EventArgs e)
        {
            dataGridViewTische.AutoGenerateColumns = false;
            dataGridViewTische.Columns.Clear();
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Tisch.Tischnummer),
                HeaderText = "Tischnummer",
                Width = 160
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Tisch.BereichName),
                HeaderText = "Bereich",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Tisch.SofortLieferung),
                HeaderText = "Sofortlieferung",
                Width = 160
            });
            dataGridViewTische.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewTische);

            dataGridViewTische.CellFormatting += DataGridViewTische_CellFormatting;

            LoadData();
        }

        private void DataGridViewTische_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dataGridViewTische.Columns[e.ColumnIndex].DataPropertyName != nameof(Tisch.SofortLieferung))
            {
                return;
            }

            if (e.Value is int wert)
            {
                e.Value = wert == 1 ? "Ja" : "Nein";
                e.FormattingApplied = true;
            }
        }

        private void LoadData()
        {
            _tische = _repository.GetAll();
            _bindingSource.DataSource = new SortableBindingList<Tisch>(_tische);
            if (_tische.Count > 0)
            {
                dataGridViewTische.ClearSelection();
                dataGridViewTische.Rows[0].Selected = true;
            }
        }

        private Tisch? SelectedTisch =>
            dataGridViewTische.SelectedRows.Count > 0
                ? dataGridViewTische.SelectedRows[0].DataBoundItem as Tisch
                : null;

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new TischBearbeitenDialog(_bereichRepository.GetAll());
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (_repository.Exists(dialog.Tisch.Tischnummer))
            {
                MessageBox.Show("Diese Tischnummer existiert bereits.", "Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _repository.Insert(dialog.Tisch);
            LoadData();
            SelectByNummer(dialog.Tisch.Tischnummer);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var tisch = SelectedTisch;
            if (tisch == null)
            {
                MessageBox.Show("Bitte zuerst einen Tisch zum Ändern auswählen.", "Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var alteNummer = tisch.Tischnummer;
            using var dialog = new TischBearbeitenDialog(
                _bereichRepository.GetAll(),
                new Tisch { Tischnummer = alteNummer, BereichId = tisch.BereichId, SofortLieferung = tisch.SofortLieferung });
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (dialog.Tisch.Tischnummer != alteNummer && _repository.Exists(dialog.Tisch.Tischnummer))
            {
                MessageBox.Show("Diese Tischnummer existiert bereits.", "Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _repository.Update(alteNummer, dialog.Tisch);
            LoadData();
            SelectByNummer(dialog.Tisch.Tischnummer);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var tisch = SelectedTisch;
            if (tisch == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Tisch {tisch.Tischnummer} wirklich löschen?", "Tische",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(tisch.Tischnummer);
            LoadData();
        }

        private void dataGridViewTische_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void SelectByNummer(int tischnummer)
        {
            for (int i = 0; i < _tische.Count; i++)
            {
                if (_tische[i].Tischnummer == tischnummer)
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }
    }
}

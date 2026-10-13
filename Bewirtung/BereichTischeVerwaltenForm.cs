using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Verwaltung der Tisch-Bereiche (Tabelle BereichTische): Neu und Bearbeiten über
    /// Popup-Dialog, Löschen über Button.
    /// </summary>
    public partial class BereichTischeVerwaltenForm : Form
    {
        private readonly BereichTischeRepository _repository = new();
        private readonly BindingSource _bindingSource = new();
        private List<BereichTische> _bereiche = new();

        public BereichTischeVerwaltenForm()
        {
            InitializeComponent();
            Load += BereichTischeVerwaltenForm_Load;
        }

        private void BereichTischeVerwaltenForm_Load(object? sender, EventArgs e)
        {
            dataGridViewBereiche.AutoGenerateColumns = false;
            dataGridViewBereiche.Columns.Clear();
            dataGridViewBereiche.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BereichTische.Bezeichnung),
                HeaderText = "Bezeichnung",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewBereiche.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewBereiche);

            LoadData();
        }

        private void LoadData()
        {
            _bereiche = _repository.GetAll();
            _bindingSource.DataSource = new SortableBindingList<BereichTische>(_bereiche);
            if (_bereiche.Count > 0)
            {
                dataGridViewBereiche.ClearSelection();
                dataGridViewBereiche.Rows[0].Selected = true;
            }
        }

        private BereichTische? SelectedBereich =>
            dataGridViewBereiche.SelectedRows.Count > 0
                ? dataGridViewBereiche.SelectedRows[0].DataBoundItem as BereichTische
                : null;

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new BereichTischeBearbeitenDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Insert(dialog.Bereich);
            var id = dialog.Bereich.BereichId;
            LoadData();
            SelectById(id);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var bereich = SelectedBereich;
            if (bereich == null || bereich.BereichId == 0)
            {
                MessageBox.Show("Bitte zuerst einen Bereich zum Ändern auswählen.", "Bereich Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new BereichTischeBearbeitenDialog(bereich);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Update(dialog.Bereich);
            var id = dialog.Bereich.BereichId;
            LoadData();
            SelectById(id);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var bereich = SelectedBereich;
            if (bereich == null || bereich.BereichId == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Bereich \"{bereich.Bezeichnung}\" wirklich löschen?", "Bereich Tische",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(bereich.BereichId);
            LoadData();
        }

        private void dataGridViewBereiche_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void SelectById(long id)
        {
            for (int i = 0; i < _bereiche.Count; i++)
            {
                if (_bereiche[i].BereichId == id)
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }
    }
}

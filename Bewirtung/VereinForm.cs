using Bewirtung.Data;
using System.ComponentModel;

namespace Bewirtung
{
    public partial class VereinForm : Form
    {
        private readonly VereinRepository _repository = new();
        private readonly BindingSource _bindingSource = new();
        private List<Verein> _vereine = new();

        public VereinForm()
        {
            InitializeComponent();
            Load += VereinForm_Load;
        }

        private void VereinForm_Load(object? sender, EventArgs e)
        {
            dataGridViewVereine.AutoGenerateColumns = false;
            dataGridViewVereine.Columns.Clear();

            dataGridViewVereine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Verein.Name),
                HeaderText = "Name",
                Width = 210
            });
            dataGridViewVereine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Verein.Ort),
                HeaderText = "Ort",
                Width = 180
            });
            dataGridViewVereine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Verein.Strasse),
                HeaderText = "Straße",
                Width = 180
            });
            dataGridViewVereine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Verein.AnsprechpartnerName),
                HeaderText = "Ansprechpartner",
                Width = 200
            });
            dataGridViewVereine.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewVereine);

            LoadData();
        }

        private void LoadData()
        {
            _vereine = _repository.GetAll();
            _bindingSource.DataSource = new SortableBindingList<Verein>(_vereine);
            if (_vereine.Count > 0)
            {
                dataGridViewVereine.ClearSelection();
                dataGridViewVereine.Rows[0].Selected = true;
            }
        }

        private Verein? SelectedVerein => _bindingSource.Current as Verein;

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new VereinBearbeitenDialog(_repository.GetBenutzer());
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Insert(dialog.Verein);
            var id = dialog.Verein.Id;
            LoadData();
            SelectById(id);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var verein = SelectedVerein;
            if (verein == null || verein.Id == 0)
            {
                return;
            }

            using var dialog = new VereinBearbeitenDialog(_repository.GetBenutzer(), verein);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Update(dialog.Verein);
            var id = dialog.Verein.Id;
            LoadData();
            SelectById(id);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var verein = SelectedVerein;
            if (verein == null || verein.Id == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Verein \"{verein.Name}\" wirklich löschen?", "Vereine",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(verein.Id);
            LoadData();
        }

        private void dataGridViewVereine_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void SelectById(long id)
        {
            for (int i = 0; i < _vereine.Count; i++)
            {
                if (_vereine[i].Id == id)
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }
    }
}

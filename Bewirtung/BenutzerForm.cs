using Bewirtung.Data;
using System.ComponentModel;

namespace Bewirtung
{
    public partial class BenutzerForm : Form
    {
        private readonly BenutzerRepository _repository = new();
        private readonly BindingSource _bindingSource = new();
        private List<Benutzer> _benutzer = new();

        public BenutzerForm()
        {
            InitializeComponent();
            Load += BenutzerForm_Load;
        }

        private void BenutzerForm_Load(object? sender, EventArgs e)
        {
            dataGridViewBenutzer.AutoGenerateColumns = false;
            dataGridViewBenutzer.Columns.Clear();
            dataGridViewBenutzer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Name),
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewBenutzer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Email),
                HeaderText = "E-Mail",
                Width = 200
            });
            dataGridViewBenutzer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Telefon),
                HeaderText = "Telefon",
                Width = 130
            });
            dataGridViewBenutzer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.VereinName),
                HeaderText = "Verein",
                Width = 150
            });
            dataGridViewBenutzer.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Administrator),
                HeaderText = "Admin",
                Width = 80
            });
            dataGridViewBenutzer.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewBenutzer);

            LoadData();
        }

        private void LoadData()
        {
            _benutzer = _repository.GetAll();
            _bindingSource.DataSource = new SortableBindingList<Benutzer>(_benutzer);
            if (_benutzer.Count > 0)
            {
                dataGridViewBenutzer.ClearSelection();
                dataGridViewBenutzer.Rows[0].Selected = true;
            }
        }

        private Benutzer? SelectedBenutzer => _bindingSource.Current as Benutzer;

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new BenutzerBearbeitenDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Insert(dialog.Benutzer);
            var id = dialog.Benutzer.UserId;
            LoadData();
            SelectById(id);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var benutzer = SelectedBenutzer;
            if (benutzer == null || benutzer.UserId == 0)
            {
                return;
            }

            using var dialog = new BenutzerBearbeitenDialog(benutzer);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Update(dialog.Benutzer);
            var id = dialog.Benutzer.UserId;
            LoadData();
            SelectById(id);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var benutzer = SelectedBenutzer;
            if (benutzer == null || benutzer.UserId == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Benutzer \"{benutzer.Name}\" wirklich löschen?", "Benutzer",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(benutzer.UserId);
            LoadData();
        }

        private void dataGridViewBenutzer_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void SelectById(long userId)
        {
            for (int i = 0; i < _benutzer.Count; i++)
            {
                if (_benutzer[i].UserId == userId)
                {
                    _bindingSource.Position = i;
                    break;
                }
            }
        }
    }
}

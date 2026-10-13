using Bewirtung.Data;
using System.ComponentModel;

namespace Bewirtung
{
    public partial class TerminForm : Form
    {
        private readonly TerminRepository _repository = new();
        private readonly VereinRepository _vereinRepository = new();
        private readonly BenutzerRepository _benutzerRepository = new();

        private readonly BindingSource _terminBinding = new();
        private readonly BindingSource _helferBinding = new();

        private List<Termin> _termine = new();
        private List<Benutzer> _helfer = new();

        public TerminForm()
        {
            InitializeComponent();
            Load += TerminForm_Load;
        }

        private void TerminForm_Load(object? sender, EventArgs e)
        {
            SetupTermineGrid();
            SetupHelferGrid();
            LoadData();
        }

        private void SetupTermineGrid()
        {
            dataGridViewTermine.AutoGenerateColumns = false;
            dataGridViewTermine.Columns.Clear();
            dataGridViewTermine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Termin.DatumText),
                HeaderText = "Datum",
                Width = 140
            });
            dataGridViewTermine.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Termin.VereinName),
                HeaderText = "Verein",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTermine.DataSource = _terminBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewTermine);
        }

        private void SetupHelferGrid()
        {
            dataGridViewHelfer.AutoGenerateColumns = false;
            dataGridViewHelfer.Columns.Clear();
            dataGridViewHelfer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Name),
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewHelfer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.VereinName),
                HeaderText = "Verein",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewHelfer.DataSource = _helferBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewHelfer);
        }

        private void LoadData()
        {
            _termine = _repository.GetAll();
            _terminBinding.DataSource = new SortableBindingList<Termin>(_termine);
            if (_termine.Count > 0)
            {
                dataGridViewTermine.ClearSelection();
                dataGridViewTermine.Rows[0].Selected = true;
            }
            else
            {
                LoadHelfer();
            }
        }

        private Termin? SelectedTermin =>
            dataGridViewTermine.SelectedRows.Count > 0
                ? dataGridViewTermine.SelectedRows[0].DataBoundItem as Termin
                : null;

        private void dataGridViewTermine_SelectionChanged(object? sender, EventArgs e)
        {
            LoadHelfer();
        }

        private void dataGridViewTermine_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnAendern_Click(sender, EventArgs.Empty);
            }
        }

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new TerminBearbeitenDialog(_vereinRepository.GetAll());
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.VereinId is not long vereinId)
            {
                return;
            }

            var termin = new Termin { Datum = dialog.Datum, VereinId = vereinId };

            try
            {
                _repository.Insert(termin);
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                MessageBox.Show("Für diesen Verein existiert bereits ein Termin an diesem Datum.",
                    "Termine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadData();
            SelectTermin(termin.BewirtungsDatum, termin.VereinId);
        }

        private void btnAendern_Click(object? sender, EventArgs e)
        {
            var termin = SelectedTermin;
            if (termin == null)
            {
                MessageBox.Show("Bitte zuerst einen Termin zum Ändern auswählen.", "Termine",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new TerminBearbeitenDialog(_vereinRepository.GetAll(), termin);
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.VereinId is not long vereinId)
            {
                return;
            }

            var altDatum = termin.BewirtungsDatum;
            var altVereinId = termin.VereinId;
            var neu = new Termin { Datum = dialog.Datum, VereinId = vereinId };

            try
            {
                _repository.Update(altDatum, altVereinId, neu);
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                MessageBox.Show("Für diesen Verein existiert bereits ein Termin an diesem Datum.",
                    "Termine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadData();
            SelectTermin(neu.BewirtungsDatum, neu.VereinId);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var termin = SelectedTermin;
            if (termin == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Termin am {termin.DatumText} ({termin.VereinName}) inklusive Helfer wirklich löschen?",
                "Termine", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(termin.BewirtungsDatum, termin.VereinId);
            LoadData();
        }

        // ----- Helfer -----

        private void LoadHelfer()
        {
            var termin = SelectedTermin;
            if (termin == null)
            {
                _helfer = new List<Benutzer>();
                _helferBinding.DataSource = new SortableBindingList<Benutzer>(_helfer);
                SetHelferEnabled(false);
                return;
            }

            SetHelferEnabled(true);
            _helfer = _repository.GetHelfer(termin.BewirtungsDatum, termin.VereinId);
            _helferBinding.DataSource = new SortableBindingList<Benutzer>(_helfer);
        }

        private Benutzer? SelectedHelfer =>
            dataGridViewHelfer.SelectedRows.Count > 0
                ? dataGridViewHelfer.SelectedRows[0].DataBoundItem as Benutzer
                : null;

        private void btnHelferHinzufuegen_Click(object? sender, EventArgs e)
        {
            var termin = SelectedTermin;
            if (termin == null)
            {
                MessageBox.Show("Bitte zuerst einen Termin auswählen bzw. speichern.", "Helfer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var vorhandeneIds = _helfer.Select(h => h.UserId).ToHashSet();
            var verfuegbar = _benutzerRepository.GetAll()
                .Where(b => !vorhandeneIds.Contains(b.UserId))
                .ToList();

            if (verfuegbar.Count == 0)
            {
                MessageBox.Show("Alle Benutzer sind diesem Termin bereits als Helfer zugeordnet.", "Helfer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new HelferAuswahlDialog(verfuegbar);
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedUserIds.Count == 0)
            {
                return;
            }

            foreach (var userId in dialog.SelectedUserIds)
            {
                _repository.AddHelfer(termin.BewirtungsDatum, termin.VereinId, userId);
            }
            LoadHelfer();
        }

        private void btnHelferEntfernen_Click(object? sender, EventArgs e)
        {
            var termin = SelectedTermin;
            var helfer = SelectedHelfer;
            if (termin == null || helfer == null)
            {
                return;
            }

            _repository.RemoveHelfer(termin.BewirtungsDatum, termin.VereinId, helfer.UserId);
            LoadHelfer();
        }

        private void SetHelferEnabled(bool enabled)
        {
            btnHelferHinzufuegen.Enabled = enabled;
            btnHelferEntfernen.Enabled = enabled;
        }

        private void SelectTermin(int datum, long vereinId)
        {
            for (int i = 0; i < _termine.Count; i++)
            {
                if (_termine[i].BewirtungsDatum == datum && _termine[i].VereinId == vereinId)
                {
                    _terminBinding.Position = i;
                    break;
                }
            }
        }
    }
}

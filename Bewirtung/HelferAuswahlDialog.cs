using Bewirtung.Data;

namespace Bewirtung
{
    public partial class HelferAuswahlDialog : Form
    {
        /// <summary>Die UserIds aller in der Liste angehakten Benutzer.</summary>
        public IReadOnlyList<long> SelectedUserIds { get; private set; } = new List<long>();

        public HelferAuswahlDialog(IEnumerable<Benutzer> benutzer)
        {
            InitializeComponent();

            foreach (var b in benutzer)
            {
                int index = gridBenutzer.Rows.Add(false, b.Name, b.Email, b.Telefon, b.VereinName);
                gridBenutzer.Rows[index].Tag = b;
            }

            gridBenutzer.CurrentCellDirtyStateChanged += GridBenutzer_CurrentCellDirtyStateChanged;
            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void GridBenutzer_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (gridBenutzer.IsCurrentCellDirty && gridBenutzer.CurrentCell is DataGridViewCheckBoxCell)
            {
                gridBenutzer.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            var ids = new List<long>();
            foreach (DataGridViewRow row in gridBenutzer.Rows)
            {
                if (row.Cells[colAuswahl.Index].Value is true && row.Tag is Benutzer b)
                {
                    ids.Add(b.UserId);
                }
            }

            if (ids.Count == 0)
            {
                MessageBox.Show("Bitte mindestens einen Benutzer auswählen.", "Helfer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            SelectedUserIds = ids;
        }

        private void btnNeuenBenutzerHinzufügen_Click(object sender, EventArgs e)
        {
            using var dialog = new BenutzerBearbeitenDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                BenutzerRepository repository = new();
                repository.Insert(dialog.Benutzer);
                int index = gridBenutzer.Rows.Add(false, dialog.Benutzer.Name, dialog.Benutzer.Email, dialog.Benutzer.Telefon, dialog.Benutzer.VereinName);
                gridBenutzer.Rows[index].Tag = dialog.Benutzer;
            }
        }
    }
}

using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten einer Tisch-Termin-Zuordnung.
    /// Die Tische werden über Checkboxen (Mehrfachauswahl) ausgewählt.
    /// </summary>
    public partial class TischTerminBearbeitenDialog : Form
    {
        private readonly TischTermin _eintrag;
        private const string AuswahlColumn = "Auswahl";
        private HashSet<int>? _pendingCheckTischnummern;

        /// <summary>Die bearbeitete bzw. neu erfasste Zuordnung (erster gewählter Tisch).</summary>
        public TischTermin Eintrag => _eintrag;

        /// <summary>Die über Checkboxen ausgewählten Tischnummern.</summary>
        public List<int> AusgewaehlteTischnummern { get; } = new();

        /// <summary>Erstellt den Dialog. Ohne Eintrag wird ein neuer angelegt.</summary>
        public TischTerminBearbeitenDialog(
            IEnumerable<Termin> termine, IEnumerable<Tisch> tische, TischTermin? eintrag = null,
            IEnumerable<int>? vorausgewaehlteTischnummern = null)
        {
            InitializeComponent();

            _eintrag = eintrag ?? new TischTermin();
            Text = eintrag == null ? "Neue Tisch-Zuordnung" : "Tisch-Zuordnung ändern";

            cmbTermin.DisplayMember = nameof(Termin.Anzeige);
            cmbTermin.DataSource = termine.ToList();

            SetupTischeGrid();
            dataGridViewTische.DataSource = new SortableBindingList<Tisch>(tische.ToList());

            if (eintrag != null)
            {
                SelectTermin(_eintrag.Bewirtungsdatum, _eintrag.VereinId);

                _pendingCheckTischnummern = vorausgewaehlteTischnummern?.ToHashSet()
                    ?? new HashSet<int> { _eintrag.Tischnummer };
            }

            btnSpeichern.Click += BtnSpeichern_Click;
            Shown += TischTerminBearbeitenDialog_Shown;
        }

        private void TischTerminBearbeitenDialog_Shown(object? sender, EventArgs e)
        {
            if (_pendingCheckTischnummern == null)
            {
                return;
            }

            foreach (var nummer in _pendingCheckTischnummern)
            {
                CheckTisch(nummer);
            }
            _pendingCheckTischnummern = null;
        }

        private void SetupTischeGrid()
        {
            dataGridViewTische.AutoGenerateColumns = false;
            dataGridViewTische.Columns.Clear();
            dataGridViewTische.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = AuswahlColumn,
                HeaderText = string.Empty,
                Width = 40
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Tisch.Tischnummer),
                HeaderText = "Tischnummer",
                Width = 150,
                ReadOnly = true
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Tisch.BereichName),
                HeaderText = "Bereich",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            GridSortHelper.EnableHeaderSorting(dataGridViewTische);
        }

        private void CheckTisch(int tischnummer)
        {
            foreach (DataGridViewRow row in dataGridViewTische.Rows)
            {
                if (row.DataBoundItem is Tisch t && t.Tischnummer == tischnummer)
                {
                    row.Cells[AuswahlColumn].Value = true;
                    break;
                }
            }
        }

        private void dataGridViewTische_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Doppelklick auf eine Zeile schaltet die Checkbox um.
            if (e.RowIndex >= 0)
            {
                var cell = dataGridViewTische.Rows[e.RowIndex].Cells[AuswahlColumn];
                cell.Value = !(cell.Value is bool b && b);
            }
        }

        private void SelectTermin(int datum, long vereinId)
        {
            for (int i = 0; i < cmbTermin.Items.Count; i++)
            {
                if (cmbTermin.Items[i] is Termin t
                    && t.BewirtungsDatum == datum && t.VereinId == vereinId)
                {
                    cmbTermin.SelectedIndex = i;
                    break;
                }
            }
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            dataGridViewTische.EndEdit();

            if (cmbTermin.SelectedItem is not Termin termin)
            {
                MessageBox.Show("Bitte einen Termin auswählen.", "Tische pro Termin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            AusgewaehlteTischnummern.Clear();
            foreach (DataGridViewRow row in dataGridViewTische.Rows)
            {
                if (row.Cells[AuswahlColumn].Value is bool b && b && row.DataBoundItem is Tisch t)
                {
                    AusgewaehlteTischnummern.Add(t.Tischnummer);
                }
            }

            if (AusgewaehlteTischnummern.Count == 0)
            {
                MessageBox.Show("Bitte mindestens einen Tisch auswählen.", "Tische pro Termin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            _eintrag.Bewirtungsdatum = termin.BewirtungsDatum;
            _eintrag.VereinId = termin.VereinId;
            _eintrag.Tischnummer = AusgewaehlteTischnummern[0];
        }
    }
}

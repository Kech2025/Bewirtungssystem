using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten eines Tisch-Bereichs.
    /// </summary>
    public partial class BereichTischeBearbeitenDialog : Form
    {
        private readonly BereichTische _bereich;

        /// <summary>Der bearbeitete bzw. neu erfasste Bereich.</summary>
        public BereichTische Bereich => _bereich;

        /// <summary>Erstellt den Dialog. Ohne Bereich wird ein neuer angelegt.</summary>
        public BereichTischeBearbeitenDialog(BereichTische? bereich = null)
        {
            InitializeComponent();

            _bereich = bereich ?? new BereichTische();
            Text = bereich == null ? "Neuer Bereich" : "Bereich ändern";

            txtBezeichnung.Text = _bereich.Bezeichnung;

            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBezeichnung.Text))
            {
                MessageBox.Show("Bitte eine Bezeichnung eingeben.", "Bereich Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBezeichnung.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            _bereich.Bezeichnung = txtBezeichnung.Text.Trim();
        }
    }
}

using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten eines Tisches.
    /// </summary>
    public partial class TischBearbeitenDialog : Form
    {
        private readonly Tisch _tisch;

        /// <summary>Der bearbeitete bzw. neu erfasste Tisch.</summary>
        public Tisch Tisch => _tisch;

        /// <summary>Erstellt den Dialog. Ohne Tisch wird ein neuer angelegt.</summary>
        public TischBearbeitenDialog(IEnumerable<BereichTische> bereiche, Tisch? tisch = null)
        {
            InitializeComponent();

            _tisch = tisch ?? new Tisch();
            Text = tisch == null ? "Neuer Tisch" : "Tisch ändern";

            if (_tisch.Tischnummer > 0)
            {
                numTischnummer.Value = _tisch.Tischnummer;
            }

            var liste = new List<BereichTische> { new() { BereichId = 0, Bezeichnung = "(kein Bereich)" } };
            liste.AddRange(bereiche);
            cmbBereich.DisplayMember = nameof(BereichTische.Bezeichnung);
            cmbBereich.ValueMember = nameof(BereichTische.BereichId);
            cmbBereich.DataSource = liste;
            cmbBereich.SelectedValue = _tisch.BereichId ?? 0L;

            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            var nummer = (int)numTischnummer.Value;
            if (nummer <= 0)
            {
                MessageBox.Show("Bitte eine gültige Tischnummer eingeben.", "Tische",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numTischnummer.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            _tisch.Tischnummer = nummer;
            _tisch.BereichId =
                cmbBereich.SelectedValue is long id && id != 0 ? id : null;
        }
    }
}

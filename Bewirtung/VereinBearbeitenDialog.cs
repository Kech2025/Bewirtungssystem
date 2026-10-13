using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten eines Vereins.
    /// </summary>
    public partial class VereinBearbeitenDialog : Form
    {
        private readonly Verein _verein;

        /// <summary>Der bearbeitete bzw. neu erfasste Verein.</summary>
        public Verein Verein => _verein;

        /// <summary>Erstellt den Dialog. Ohne Verein wird ein neuer angelegt.</summary>
        public VereinBearbeitenDialog(IEnumerable<Benutzer> ansprechpartner, Verein? verein = null)
        {
            InitializeComponent();

            _verein = verein ?? new Verein();
            Text = verein == null ? "Neuer Verein" : "Verein ändern";

            var liste = new List<Benutzer> { new() { UserId = 0, Name = "(kein Ansprechpartner)" } };
            liste.AddRange(ansprechpartner);
            cmbAnsprechpartner.DisplayMember = nameof(Benutzer.Name);
            cmbAnsprechpartner.ValueMember = nameof(Benutzer.UserId);
            cmbAnsprechpartner.DataSource = liste;

            txtName.Text = _verein.Name;
            txtStrasse.Text = _verein.Strasse;
            txtPlz.Text = _verein.Plz;
            txtOrt.Text = _verein.Ort;
            cmbAnsprechpartner.SelectedValue = _verein.AnsprechpartnerId ?? 0L;

            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Bitte einen Namen eingeben.", "Vereine",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            _verein.Name = txtName.Text.Trim();
            _verein.Strasse = txtStrasse.Text.Trim();
            _verein.Plz = txtPlz.Text.Trim();
            _verein.Ort = txtOrt.Text.Trim();
            _verein.AnsprechpartnerId =
                cmbAnsprechpartner.SelectedValue is long id && id != 0 ? id : null;
        }
    }
}

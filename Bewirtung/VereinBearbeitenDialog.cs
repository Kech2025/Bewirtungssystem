using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten eines Vereins.
    /// </summary>
    public partial class VereinBearbeitenDialog : Form
    {
        private readonly Verein _verein;
        private readonly VereinRepository _vereinRepository = new();
        private readonly BenutzerRepository _benutzerRepository = new();

        /// <summary>Der bearbeitete bzw. neu erfasste Verein.</summary>
        public Verein Verein => _verein;

        /// <summary>Erstellt den Dialog. Ohne Verein wird ein neuer angelegt.</summary>
        public VereinBearbeitenDialog(IEnumerable<Benutzer> ansprechpartner, Verein? verein = null)
        {
            InitializeComponent();

            _verein = verein ?? new Verein();
            Text = verein == null ? "Neuer Verein" : "Verein ändern";

            LadeAnsprechpartner(ansprechpartner, _verein.AnsprechpartnerId ?? 0L);

            txtName.Text = _verein.Name;
            txtStrasse.Text = _verein.Strasse;
            txtPlz.Text = _verein.Plz;
            txtOrt.Text = _verein.Ort;

            btnSpeichern.Click += BtnSpeichern_Click;
            cmdNeuerAnsprechpartner.Click += CmdNeuerAnsprechpartner_Click;
        }

        /// <summary>Deaktiviert den Button zum Anlegen eines neuen Ansprechpartners.</summary>
        public void DeaktiviereNeuerAnsprechpartner()
        {
            cmdNeuerAnsprechpartner.Enabled = false;
        }

        private void LadeAnsprechpartner(IEnumerable<Benutzer> ansprechpartner, long? selectedId)
        {
            var liste = new List<Benutzer> { new() { UserId = 0, Name = "(kein Ansprechpartner)" } };
            liste.AddRange(ansprechpartner);
            cmbAnsprechpartner.DisplayMember = nameof(Benutzer.Name);
            cmbAnsprechpartner.ValueMember = nameof(Benutzer.UserId);
            cmbAnsprechpartner.DataSource = liste;
            cmbAnsprechpartner.SelectedValue = selectedId ?? 0L;
        }

        private void CmdNeuerAnsprechpartner_Click(object? sender, EventArgs e)
        {
            using var dialog = new BenutzerBearbeitenDialog();
            dialog.DeaktiviereNeuerVerein();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var id = _benutzerRepository.Insert(dialog.Benutzer);
            LadeAnsprechpartner(_vereinRepository.GetBenutzer(), id);
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

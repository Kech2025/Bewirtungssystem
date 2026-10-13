using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Popup-Dialog zum Anlegen bzw. Bearbeiten eines Benutzers.
    /// </summary>
    public partial class BenutzerBearbeitenDialog : Form
    {
        private readonly Benutzer _benutzer;
        private readonly VereinRepository _vereinRepository = new();

        /// <summary>Der bearbeitete bzw. neu erfasste Benutzer.</summary>
        public Benutzer Benutzer => _benutzer;

        /// <summary>Erstellt den Dialog. Ohne Benutzer wird ein neuer angelegt.</summary>
        public BenutzerBearbeitenDialog(Benutzer? benutzer = null)
        {
            InitializeComponent();

            _benutzer = benutzer ?? new Benutzer();
            Text = benutzer == null ? "Neuer Benutzer" : "Benutzer ändern";

            LadeVereine();

            txtName.Text = _benutzer.Name;
            txtEmail.Text = _benutzer.Email;
            txtPassword.Text = _benutzer.Password;
            txtTelefon.Text = _benutzer.Telefon;
            chkAdministrator.Checked = _benutzer.Administrator;
            cmbVerein.SelectedValue = _benutzer.VereinId ?? 0L;

            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void LadeVereine()
        {
            var vereine = new List<Verein> { new() { Id = 0, Name = "(kein Verein)" } };
            vereine.AddRange(_vereinRepository.GetAll());
            cmbVerein.DisplayMember = nameof(Verein.Name);
            cmbVerein.ValueMember = nameof(Verein.Id);
            cmbVerein.DataSource = vereine;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Bitte einen Namen eingeben.", "Benutzer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            _benutzer.Name = txtName.Text.Trim();
            _benutzer.Email = txtEmail.Text.Trim();
            _benutzer.Password = txtPassword.Text;
            _benutzer.Telefon = txtTelefon.Text.Trim();
            _benutzer.Administrator = chkAdministrator.Checked;
            var vereinId = cmbVerein.SelectedValue as long?;
            _benutzer.VereinId = vereinId is > 0 ? vereinId : null;
        }
    }
}

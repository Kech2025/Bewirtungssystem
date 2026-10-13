using Bewirtung.Data;

namespace Bewirtung
{
    public partial class ZusatzBearbeitenDialog : Form
    {
        public GlobalePreislisteZusatz Zusatz { get; private set; }

        /// <summary>Der im Dialog erfasste Preis der Zusatzposition.</summary>
        public decimal Preis { get; private set; }

        public ZusatzBearbeitenDialog(long speisekarteId, GlobalePreislisteZusatz? zusatz = null)
        {
            InitializeComponent();

            Zusatz = zusatz ?? new GlobalePreislisteZusatz { SpeisekarteId = speisekarteId };
            Text = zusatz == null ? "Neue Zusatzposition" : "Zusatzposition ändern";

            txtBezeichnung.Text = Zusatz.Bezeichnung;
            txtBildPfad.Text = Zusatz.BildPfad;
            chkDruck.Checked = Zusatz.Druck;
            numPreis.Value = Zusatz.Preis.HasValue
                ? Math.Clamp(Zusatz.Preis.Value, numPreis.Minimum, numPreis.Maximum)
                : 0m;

            btnSpeichern.Click += BtnSpeichern_Click;
            btnBildDurchsuchen.Click += BtnBildDurchsuchen_Click;
            txtBildPfad.TextChanged += TxtBildPfad_TextChanged;

            SetPictureBoxImage(txtBildPfad.Text);
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBezeichnung.Text))
            {
                MessageBox.Show("Bitte eine Bezeichnung eingeben.", "Zusatzpositionen",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBezeichnung.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            Zusatz.Bezeichnung = txtBezeichnung.Text.Trim();
            Zusatz.BildPfad = txtBildPfad.Text.Trim();
            Zusatz.Druck = chkDruck.Checked;
            Preis = numPreis.Value;
            Zusatz.Preis = numPreis.Value;
        }

        private void BtnBildDurchsuchen_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Bilddateien (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Alle Dateien (*.*)|*.*",
                Title = "Bild auswählen"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtBildPfad.Text = dialog.FileName;
            }
        }

        private void TxtBildPfad_TextChanged(object? sender, EventArgs e)
        {
            SetPictureBoxImage(txtBildPfad.Text);
        }

        private void SetPictureBoxImage(string pfad)
        {
            pictureBoxZusatz.Image?.Dispose();
            pictureBoxZusatz.Image = null;

            if (string.IsNullOrWhiteSpace(pfad) || !File.Exists(pfad))
            {
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(pfad);
                using var stream = new MemoryStream(bytes);
                pictureBoxZusatz.Image = Image.FromStream(stream);
            }
            catch
            {
                pictureBoxZusatz.Image = null;
            }
        }
    }
}

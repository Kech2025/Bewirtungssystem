using Bewirtung.Data;

namespace Bewirtung
{
    public partial class GlobalePreislisteBearbeitenDialog : Form
    {
        private sealed record KategorieItem(string Text, Preiskategorie Wert);

        public GlobalePreisliste Eintrag { get; private set; }

        /// <summary>Der im Dialog erfasste Preis der Position.</summary>
        public decimal Preis { get; private set; }

        /// <summary>Gültig-Ab (yyyyMMdd) des Preises aus GlobalerPreis.</summary>
        public int GueltigAb { get; private set; }

        /// <summary>Gültig-Bis (yyyyMMdd) des Preises; 0 = offen (unbegrenzt gültig).</summary>
        public int GueltigBis { get; private set; }

        public GlobalePreislisteBearbeitenDialog(GlobalePreisliste? eintrag = null)
        {
            InitializeComponent();

            Eintrag = eintrag ?? new GlobalePreisliste();
            Text = eintrag == null ? "Neue Position" : "Position ändern";

            cmbPreiskategorie.DisplayMember = nameof(KategorieItem.Text);
            cmbPreiskategorie.ValueMember = nameof(KategorieItem.Wert);
            cmbPreiskategorie.DataSource = new List<KategorieItem>
            {
                new("Speisen", Preiskategorie.Speisen),
                new("Getränke", Preiskategorie.Getraenke)
            };

            txtName.Text = Eintrag.Name;
            cmbPreiskategorie.SelectedValue = Eintrag.Preiskategorie;
            txtZusatztext.Text = Eintrag.Zusatztext;
            txtBildPfad.Text = Eintrag.BildPfad;
            chkDruck.Checked = Eintrag.Druck;
            numPreis.Value = Eintrag.Preis.HasValue
                ? Math.Clamp(Eintrag.Preis.Value, numPreis.Minimum, numPreis.Maximum)
                : 0m;

            // Gültigkeitszeitraum vorbelegen: Start aus bestehendem Preis, bei einer
            // neuen Position der 1.1. des aktuellen Jahres, sonst heute.
            dtpGueltigAb.Value = Eintrag.StartDatum > 0
                ? Termin.FromYyyyMmDd(Eintrag.StartDatum).Date
                : eintrag == null
                    ? new DateTime(DateTime.Today.Year, 1, 1)
                    : DateTime.Today;
            if (Eintrag.GueltigBis > 0)
            {
                dtpGueltigBis.Value = Termin.FromYyyyMmDd(Eintrag.GueltigBis).Date;
                dtpGueltigBis.Checked = true;
            }
            else
            {
                dtpGueltigBis.Value = DateTime.Today;
                dtpGueltigBis.Checked = false;
            }

            btnSpeichern.Click += BtnSpeichern_Click;
            btnBildDurchsuchen.Click += BtnBildDurchsuchen_Click;
            txtBildPfad.TextChanged += TxtBildPfad_TextChanged;

            SetPictureBoxImage(txtBildPfad.Text);
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Bitte einen Namen eingeben.", "Globale Preisliste",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            var gueltigAb = Termin.ToYyyyMmDd(dtpGueltigAb.Value.Date);
            var gueltigBis = dtpGueltigBis.Checked ? Termin.ToYyyyMmDd(dtpGueltigBis.Value.Date) : 0;

            if (gueltigBis != 0 && gueltigBis < gueltigAb)
            {
                MessageBox.Show("Das Gültig-Bis-Datum darf nicht vor dem Gültig-Ab-Datum liegen.",
                    "Globale Preisliste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Eintrag.Name = txtName.Text.Trim();
            Eintrag.Preiskategorie = cmbPreiskategorie.SelectedValue is Preiskategorie k
                ? k : Preiskategorie.Speisen;
            Eintrag.Zusatztext = txtZusatztext.Text.Trim();
            Eintrag.BildPfad = txtBildPfad.Text.Trim();
            Eintrag.Druck = chkDruck.Checked;
            Preis = numPreis.Value;
            Eintrag.Preis = numPreis.Value;
            GueltigAb = gueltigAb;
            GueltigBis = gueltigBis;
            Eintrag.StartDatum = gueltigAb;
            Eintrag.GueltigBis = gueltigBis;
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
            pictureBoxHaupt.Image?.Dispose();
            pictureBoxHaupt.Image = null;

            if (string.IsNullOrWhiteSpace(pfad) || !File.Exists(pfad))
            {
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(pfad);
                using var stream = new MemoryStream(bytes);
                pictureBoxHaupt.Image = Image.FromStream(stream);
            }
            catch
            {
                pictureBoxHaupt.Image = null;
            }
        }
    }
}

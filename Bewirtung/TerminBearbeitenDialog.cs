using Bewirtung.Data;

namespace Bewirtung
{
    public partial class TerminBearbeitenDialog : Form
    {
        public DateTime Datum => dtpDatum.Value.Date;

        public long? VereinId => cmbVerein.SelectedValue as long?;

        public TerminBearbeitenDialog(IEnumerable<Verein> vereine, Termin? termin = null)
        {
            InitializeComponent();

            Text = termin == null ? "Neuer Termin" : "Termin ändern";

            cmbVerein.DisplayMember = nameof(Verein.Name);
            cmbVerein.ValueMember = nameof(Verein.Id);
            cmbVerein.DataSource = vereine.ToList();

            if (termin != null)
            {
                dtpDatum.Value = termin.Datum;
                cmbVerein.SelectedValue = termin.VereinId;
            }

            btnSpeichern.Click += BtnSpeichern_Click;
        }

        private void BtnSpeichern_Click(object? sender, EventArgs e)
        {
            if (cmbVerein.SelectedValue is not long)
            {
                MessageBox.Show("Bitte einen Verein auswählen.", "Termine",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}

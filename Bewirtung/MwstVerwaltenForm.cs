using System.ComponentModel;
using Bewirtung.Data;

namespace Bewirtung
{
    public partial class MwstVerwaltenForm : Form
    {
        private readonly MwstRepository _repository = new();
        private BindingList<MwstSatz> _saetze = new();

        public MwstVerwaltenForm()
        {
            InitializeComponent();
            ConfigureGrid();
            LoadData();
        }

        private void ConfigureGrid()
        {
            dataGridViewMwst.AutoGenerateColumns = false;
            dataGridViewMwst.Columns.Clear();

            dataGridViewMwst.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "KategorieText",
                DataPropertyName = "KategorieText",
                HeaderText = "Kategorie",
                ReadOnly = true,
                Width = 200
            });

            dataGridViewMwst.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Mwst",
                DataPropertyName = "Mwst",
                HeaderText = "Mehrwertsteuer (%)",
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" }
            });
        }

        private void LoadData()
        {
            _saetze = new BindingList<MwstSatz>(_repository.GetAll());
            dataGridViewMwst.DataSource = _saetze;
        }

        private void btnSpeichern_Click(object? sender, EventArgs e)
        {
            dataGridViewMwst.EndEdit();
            try
            {
                foreach (var satz in _saetze)
                {
                    _repository.Update(satz);
                }
                MessageBox.Show("Die Mehrwertsteuersätze wurden gespeichert.", "Mehrwertsteuer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern: " + ex.Message, "Mehrwertsteuer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

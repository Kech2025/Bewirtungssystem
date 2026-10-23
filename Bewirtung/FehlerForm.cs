using System.Runtime.CompilerServices;
using Bewirtung.Data;
using DocumentFormat.OpenXml.Office.Word;

namespace Bewirtung
{
    /// <summary>
    /// Zeigt und druckt die Preisliste eines Termins als flache Liste mit Preis.
    /// Oben links eine Combobox zur Auswahl von Termin/Verein, ein Button zum Drucken.
    /// </summary>
    public partial class FehlerForm : Form
    {
        private readonly TerminRepository _terminRepository = new();
        private readonly AbrechnungRepository _abrechnungRepository = new();

        private List<FehlerAbrechnungZeile> _zeilen = new();

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int TerminPreisliste { get; set; } = 0;

        public FehlerForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            ConfigureGrid();
            cmbTermine.SelectedIndexChanged += CmbTermine_SelectedIndexChanged;
            Load += PreislisteForm_Load;
        }

        private void ConfigureGrid()
        {
            gridPreisliste.AutoGenerateColumns = false;
            gridPreisliste.Columns.Clear();
            gridPreisliste.ReadOnly = false;
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.UserName),
                HeaderText = "Wer",
                Width = 120
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Bewirtungszeitpunkt),
                HeaderText = "Bewirtungszeitpunkt",
                Width = 90
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Tisch),
                HeaderText = "Tisch",
                Width = 120
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Bereich),
                HeaderText = "Bereich",
                Width = 120
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.ArtikelName),
                HeaderText = "Artikel",
                Width = 180
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Besonderheit),
                HeaderText = "Besonderheit",
                Width = 180
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Menge),
                HeaderText = "Menge",
                Width = 80
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Gesamtpreis),
                HeaderText = "Gesamtpreis",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.MengeGeliefert),
                HeaderText = "Menge geliefert",
                Width = 80
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.MengeBezahlt),
                HeaderText = "menge bezahlt",
                Width = 80
            });
            var deleteColumn = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.chkLöschen),
                HeaderText = "löschen",
                Width = 80,
                ReadOnly = false,
                ValueType = typeof(bool)
            };
            deleteColumn.CellTemplate.Value = false;
            gridPreisliste.Columns.Add(deleteColumn);

            var correctColumn = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.chkFehlerKorrigieren),
                HeaderText = "Fehler korrigieren",
                Width = 80,
                ReadOnly = false,
                ValueType = typeof(bool)
            };
            correctColumn.CellTemplate.Value = false;
            gridPreisliste.Columns.Add(correctColumn);
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.BewirtungsauftragId),
                HeaderText = "BewirtungsauftragId",
                Width = 80,
                Visible = false
            });
            gridPreisliste.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(FehlerAbrechnungZeile.Bewirtungsposition),
                HeaderText = "Bewirtungsposition",
                Width = 80,
                Visible = false
            });
            gridPreisliste.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void PreislisteForm_Load(object? sender, EventArgs e)
        {
            LoadTermine();
            if (TerminPreisliste != 0)
            {
                var termine = _terminRepository.GetAll();
                var termin = termine.FirstOrDefault(t => t.BewirtungsDatum == TerminPreisliste);

                if (termin != null)
                {
                    foreach (var item in cmbTermine.Items)
                    {
                        if (item is Termin t && t.BewirtungsDatum == TerminPreisliste)
                        {
                            cmbTermine.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        private void LoadTermine()
        {
            var termine = _terminRepository.GetAll();
            _terminRepository.LoadTerminCombobox(cmbTermine, termine);            
        }

        private void CmbTermine_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadFehlerListe();
        }

        private void LoadFehlerListe()
        {
            if (cmbTermine.SelectedItem is not Termin termin)
            {
                _zeilen = new List<FehlerAbrechnungZeile>();
                gridPreisliste.DataSource = null;
                return;
            }

            _zeilen = _abrechnungRepository
                .GetFehlerListe(termin.BewirtungsDatum)
                .ToList();
            gridPreisliste.DataSource = new BindingSource { DataSource = _zeilen };
            btnFix.Enabled = _zeilen.Count > 0;
        }

        private void btnFix_Click(object sender, EventArgs e)
        {
            bool fehler = false;
            foreach (DataGridViewRow row in gridPreisliste.Rows)
            {
                if (row.DataBoundItem is FehlerAbrechnungZeile zeile)
                {
                    if (zeile.chkLöschen)
                    {
                        _abrechnungRepository.DeleteFehlerAbrechnungZeile(zeile);
                    }
                    else if (zeile.chkFehlerKorrigieren)
                    {
                        if(zeile.Bewirtungsposition != 0)
                        { 
                            _abrechnungRepository.UpdateFehlerAbrechnungZeile(zeile);
                        }
                        else
                        {
                            fehler = true;
                        }
                    }
                }
            }
            if (fehler)
            {
                MessageBox.Show("Fehlerhafte Zeilen konnten nicht korrigiert werden, da die Bewirtungsposition 0 ist.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadFehlerListe();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (TerminPreisliste != 0)
            {
                this.Close();
            }
        }
    }
}

using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Tagesabrechnung: zeigt gleichartig aggregierte Bewirtungspositionen eines Termins.
    /// Filter: Termin, Benutzer (inkl. "Alle"), Tisch (inkl. "Alle") und Art
    /// (Speisen / Getränke / Beides).
    /// </summary>
    public partial class AbrechnungTagForm : Form
    {
        private readonly TerminRepository _terminRepository = new();
        private readonly AbrechnungRepository _abrechnungRepository = new();
        private readonly BindingSource _bindingSource = new();

        private List<AbrechnungZeile> _zeilen = new();

        private sealed class BenutzerFilter
        {
            public long UserId { get; init; }
            public string Anzeige { get; init; } = string.Empty;
            public override string ToString() => Anzeige;
        }

        private sealed class TischFilter
        {
            public int? Tischnummer { get; init; }
            public string Anzeige { get; init; } = string.Empty;
            public override string ToString() => Anzeige;
        }

        private sealed class BereichFilter
        {
            public long? BereichId { get; init; }
            public string Anzeige { get; init; } = string.Empty;
            public override string ToString() => Anzeige;
        }

        public AbrechnungTagForm()
        {
            InitializeComponent();
            Load += AbrechnungTagForm_Load;
        }

        private void AbrechnungTagForm_Load(object? sender, EventArgs e)
        {
            dataGridViewAbrechnung.AutoGenerateColumns = false;
            dataGridViewAbrechnung.Columns.Clear();
            dataGridViewAbrechnung.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AbrechnungZeile.Artikel),
                HeaderText = "Artikel",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewAbrechnung.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AbrechnungZeile.Menge),
                HeaderText = "Menge",
                Width = 90
            });
            dataGridViewAbrechnung.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AbrechnungZeile.Besonderheit),
                HeaderText = "Besonderheit",
                Width = 180
            });
            dataGridViewAbrechnung.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AbrechnungZeile.Gesamtpreis),
                HeaderText = "Gesamtpreis",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dataGridViewAbrechnung.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AbrechnungZeile.Mwst),
                HeaderText = "Mwst",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dataGridViewAbrechnung.DataSource = _bindingSource;
            GridSortHelper.EnableHeaderSorting(dataGridViewAbrechnung);

            LoadFilterTermine();
        }

        private void LoadFilterTermine()
        {
            var termine = _terminRepository.GetAll();

            cmbFilterTermin.SelectedIndexChanged -= cmbFilterTermin_SelectedIndexChanged;
            cmbFilterTermin.DisplayMember = nameof(Termin.Anzeige);
            cmbFilterTermin.Items.Clear();
            foreach (var termin in termine)
            {
                cmbFilterTermin.Items.Add(termin);
            }
            if (cmbFilterTermin.Items.Count > 0)
            {
                cmbFilterTermin.SelectedIndex = 0;
            }
            cmbFilterTermin.SelectedIndexChanged += cmbFilterTermin_SelectedIndexChanged;

            LoadZeilen();
        }

        private void cmbFilterTermin_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadZeilen();
        }

        private void LoadZeilen()
        {
            if (cmbFilterTermin.SelectedItem is not Termin termin)
            {
                _zeilen = new List<AbrechnungZeile>();
            }
            else
            {
                _zeilen = _abrechnungRepository.GetForTermin(termin.BewirtungsDatum);

            }

            LoadFilterBenutzer();
            LoadFilterBereiche();
            LoadFilterTische();
            ApplyFilter();
        }

        private void LoadFilterBenutzer()
        {
            cmbFilterBenutzer.SelectedIndexChanged -= cmbFilter_Changed;
            cmbFilterBenutzer.Items.Clear();
            cmbFilterBenutzer.Items.Add(new BenutzerFilter { UserId = -1, Anzeige = "Alle" });
            foreach (var gruppe in _zeilen
                .GroupBy(z => z.UserId)
                .Select(g => new BenutzerFilter { UserId = g.Key, Anzeige = g.First().Benutzer })
                .OrderBy(b => b.Anzeige))
            {
                cmbFilterBenutzer.Items.Add(gruppe);
            }
            cmbFilterBenutzer.SelectedIndex = 0;
            cmbFilterBenutzer.SelectedIndexChanged += cmbFilter_Changed;
        }

        private void LoadFilterTische()
        {
            long? bereichId = cmbFilterBereich.SelectedItem is BereichFilter brf ? brf.BereichId : null;

            cmbFilterTisch.SelectedIndexChanged -= cmbFilter_Changed;
            cmbFilterTisch.Items.Clear();
            cmbFilterTisch.Items.Add(new TischFilter { Tischnummer = null, Anzeige = "Alle" });
            foreach (var tisch in _zeilen
                .Where(z => !bereichId.HasValue || z.BereichId == bereichId.Value)
                .Select(z => z.Tischnummer)
                .Distinct()
                .OrderBy(n => n))
            {
                cmbFilterTisch.Items.Add(new TischFilter { Tischnummer = tisch, Anzeige = $"Tisch {tisch}" });
            }
            cmbFilterTisch.SelectedIndex = 0;
            cmbFilterTisch.SelectedIndexChanged += cmbFilter_Changed;
        }

        private void LoadFilterBereiche()
        {
            cmbFilterBereich.SelectedIndexChanged -= cmbFilter_Changed;
            cmbFilterBereich.Items.Clear();
            cmbFilterBereich.Items.Add(new BereichFilter { BereichId = null, Anzeige = "Alle" });
            foreach (var bereich in _zeilen
                .Where(z => z.BereichId > 0)
                .GroupBy(z => z.BereichId)
                .Select(g => new BereichFilter { BereichId = g.Key, Anzeige = g.First().BereichName })
                .OrderBy(b => b.Anzeige))
            {
                cmbFilterBereich.Items.Add(bereich);
            }
            cmbFilterBereich.SelectedIndex = 0;
            cmbFilterBereich.SelectedIndexChanged += cmbFilterBereich_Changed;
        }

        private void cmbFilterBereich_Changed(object? sender, EventArgs e)
        {
            LoadFilterTische();
            ApplyFilter();
        }

        private void cmbFilter_Changed(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<AbrechnungZeile> gefiltert = _zeilen;

            if (cmbFilterBenutzer.SelectedItem is BenutzerFilter bf && bf.UserId >= 0)
            {
                gefiltert = gefiltert.Where(z => z.UserId == bf.UserId);
            }

            if (cmbFilterTisch.SelectedItem is TischFilter tf && tf.Tischnummer.HasValue)
            {
                gefiltert = gefiltert.Where(z => z.Tischnummer == tf.Tischnummer.Value);
            }

            if (cmbFilterBereich.SelectedItem is BereichFilter brf && brf.BereichId.HasValue)
            {
                gefiltert = gefiltert.Where(z => z.BereichId == brf.BereichId.Value);
            }

            if (rbSpeisen.Checked)
            {
                gefiltert = gefiltert.Where(z => z.Preiskategorie == (int)Preiskategorie.Speisen);
            }
            else if (rbGetraenke.Checked)
            {
                gefiltert = gefiltert.Where(z => z.Preiskategorie == (int)Preiskategorie.Getraenke);
            }

            
            if (dataGridViewAbrechnung.Rows.Count > 0)
            {
                dataGridViewAbrechnung.ClearSelection();
            }
            IEnumerable<AbrechnungZeile> groupiert = gefiltert
                .GroupBy(z => new { z.PreislisteId, z.PreislisteZusatzId, z.Besonderheit })
                .Select(g => new AbrechnungZeile
                {
                    PreislisteId = g.Key.PreislisteId,
                    PreislisteZusatzId = g.Key.PreislisteZusatzId,
                    Artikel = g.First().Artikel,
                    Menge = g.Sum(z => z.Menge),
                    Besonderheit = g.Key.Besonderheit,
                    Gesamtpreis = g.Sum(z => z.Gesamtpreis),
                    Mwst = g.Sum(z => z.Mwst)
                });
            _bindingSource.DataSource = new SortableBindingList<AbrechnungZeile>(groupiert.ToList());
            UpdateSummen(groupiert.ToList());
        }

        private void UpdateSummen(List<AbrechnungZeile> gefiltert)
        {
            var umsatzVerkauf = gefiltert.Sum(z => z.Gesamtpreis);
            var mwstSumme = gefiltert.Sum(z => z.Mwst);

            decimal umsatzAuftrag = 0m;
            decimal trinkgeld = 0m;
            if (cmbFilterTermin.SelectedItem is Termin termin)
            {
                long? userId = cmbFilterBenutzer.SelectedItem is BenutzerFilter bf && bf.UserId >= 0
                    ? bf.UserId
                    : null;
                int? tischnummer = cmbFilterTisch.SelectedItem is TischFilter tf && tf.Tischnummer.HasValue
                    ? tf.Tischnummer.Value
                    : null;
                trinkgeld = _abrechnungRepository.GetTrinkgeldSumme(termin.BewirtungsDatum, userId, tischnummer);
            }

            lblUmsatzVerkauf.Text = $"Umsatz aus Verkauf: {umsatzVerkauf:C}";
            lblMwstSumme.Text = $"Mehrwertsteuer: {mwstSumme:C}";
            lblUmsatzAuftrag.Text = $"Umsatz aus Auftrag: {umsatzVerkauf + trinkgeld:C}";
            lblTrinkgeld.Text = $"Trinkgeld: {trinkgeld:C}";
        }
    }
}

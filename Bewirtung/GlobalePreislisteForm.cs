using Bewirtung.Data;
using System.ComponentModel;

namespace Bewirtung
{
    public partial class GlobalePreislisteForm : Form
    {
        private readonly GlobalePreislisteRepository _repository = new();
        private readonly PreiseRepository _preiseRepository = new();
        private readonly BindingSource _mainBinding = new();
        private readonly BindingSource _zusatzBinding = new();
        private List<GlobalePreisliste> _eintraege = new();
        private List<GlobalePreislisteZusatz> _zusaetze = new();
        private List<DateTime> _datumsListe = new();
        private DateTime _lastDate = DateTime.Today;
        private readonly bool _sofortNeu;

        public GlobalePreislisteForm()
            : this(false)
        {
        }

        /// <summary>
        /// Erstellt die Form. Mit <paramref name="sofortNeu"/> = true wird direkt nach
        /// dem Laden der Dialog zum Anlegen einer neuen Position geöffnet.
        /// </summary>
        public GlobalePreislisteForm(bool sofortNeu)
        {
            InitializeComponent();
            _sofortNeu = sofortNeu;
            // Im Normalfall maximiert; im "sofort neu"-Modus wird die Form über dem
            // Owner-Fenster positioniert und rundum 20 px kleiner dargestellt.
            if (!sofortNeu)
            {
                WindowState = FormWindowState.Maximized;
            }
            Load += GlobalePreislisteForm_Load;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_sofortNeu && Owner != null)
            {
                StartPosition = FormStartPosition.Manual;
                WindowState = FormWindowState.Normal;
                var bounds = Owner.Bounds;
                Bounds = new Rectangle(
                    bounds.X + 20,
                    bounds.Y + 20,
                    Math.Max(100, bounds.Width - 40),
                    Math.Max(100, bounds.Height - 40));
            }
        }

        private void GlobalePreislisteForm_Load(object? sender, EventArgs e)
        {
            var termine = PreiseRepository.GetAllDates();
            _datumsListe = termine.Select(d => Termin.FromYyyyMmDd(d).Date).ToList();
            if (_datumsListe.Count == 0)
            {
                _datumsListe.Add(DateTime.Today);
            }

            
            SetupMainGrid();
            SetupZusatzGrid();
            cboDatumAb.DataSource = new List<DateTime>(_datumsListe);

            cboDatumAb.SelectedIndex = 0;
            //LoadData();

            if (_sofortNeu)
            {
                // Nach dem ersten Anzeigen sofort den Neu-Dialog öffnen.
                BeginInvoke(new Action(() => btnNeu_Click(this, EventArgs.Empty)));
            }
        }

        private void SetupMainGrid()
        {
            dataGridViewMain.AutoGenerateColumns = false;
            dataGridViewMain.Columns.Clear();
            dataGridViewMain.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.Name),
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewMain.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.KategorieText),
                HeaderText = "Kategorie",
                Width = 130
            });
            dataGridViewMain.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.Druck),
                HeaderText = "Druck",
                Width = 80
            });
            dataGridViewMain.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.StartDatumWert),
                HeaderText = "Start-Datum",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
            });
            dataGridViewMain.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.GueltigBisWert),
                HeaderText = "Gültig-Bis",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
            });
            dataGridViewMain.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreisliste.Preis),
                HeaderText = "Preis",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dataGridViewMain.DataSource = _mainBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewMain);
        }

        private void SetupZusatzGrid()
        {
            dataGridViewZusatz.AutoGenerateColumns = false;
            dataGridViewZusatz.Columns.Clear();
            dataGridViewZusatz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreislisteZusatz.Bezeichnung),
                HeaderText = "Bezeichnung",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewZusatz.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(GlobalePreislisteZusatz.Druck),
                HeaderText = "Druck",
                Width = 80
            });
            dataGridViewZusatz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreislisteZusatz.StartDatumWert),
                HeaderText = "Start-Datum",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
            });
            dataGridViewZusatz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(GlobalePreislisteZusatz.Preis),
                HeaderText = "Preis",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dataGridViewZusatz.DataSource = _zusatzBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewZusatz);
        }

        private void LoadData()
        {
            _eintraege = _repository.GetAll(Termin.ToYyyyMmDd(_lastDate));
            var preise = PreiseRepository.GetHauptPreiseMitStart(Termin.ToYyyyMmDd(_lastDate));
            foreach (var eintrag in _eintraege)
            {
                if (preise.TryGetValue(eintrag.Id, out var info))
                {
                    eintrag.Preis = info.Preis;
                    eintrag.StartDatum = info.GueltigAb;
                    eintrag.GueltigBis = info.GueltigBis;
                }
                else
                {
                    eintrag.Preis = null;
                    eintrag.StartDatum = 0;
                    eintrag.GueltigBis = 0;
                }
            }
            _mainBinding.DataSource = new SortableBindingList<GlobalePreisliste>(_eintraege);
            if (_eintraege.Count > 0)
            {
                dataGridViewMain.ClearSelection();
                dataGridViewMain.Rows[0].Selected = true;
            }
            else
            {
                LoadZusatz();
            }
        }

        private GlobalePreisliste? SelectedEintrag =>
            dataGridViewMain.SelectedRows.Count > 0
                ? dataGridViewMain.SelectedRows[0].DataBoundItem as GlobalePreisliste
                : null;

        private void dataGridViewMain_SelectionChanged(object? sender, EventArgs e)
        {
            LoadZusatz();
        }

        private void dataGridViewMain_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnSpeichern_Click(sender, EventArgs.Empty);
            }
        }

        private void btnNeu_Click(object? sender, EventArgs e)
        {
            using var dialog = new GlobalePreislisteBearbeitenDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Insert(dialog.Eintrag);
            var id = dialog.Eintrag.Id;
            SavePreis(id, 0, dialog.Preis, dialog.Eintrag.Name, 0, dialog.GueltigAb, dialog.GueltigBis);
            LoadData();
            SelectMainById(id);
        }

        private void btnSpeichern_Click(object? sender, EventArgs e)
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null || eintrag.Id == 0)
            {
                MessageBox.Show("Bitte zuerst eine Zeile zum Ändern auswählen.", "Globale Preisliste",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ursprüngliches Gültig-Ab merken, da der Dialog Eintrag.StartDatum überschreibt.
            var altGueltigAb = eintrag.StartDatum;

            using var dialog = new GlobalePreislisteBearbeitenDialog(eintrag);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.Update(dialog.Eintrag);
            var id = dialog.Eintrag.Id;
            SavePreis(id, 0, dialog.Preis, dialog.Eintrag.Name, altGueltigAb, dialog.GueltigAb, dialog.GueltigBis);
            LoadData();
            SelectMainById(id);
        }

        private void btnLoeschen_Click(object? sender, EventArgs e)
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null || eintrag.Id == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Position \"{eintrag.Name}\" inklusive aller Zusatzpositionen wirklich löschen?",
                "Globale Preisliste", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.Delete(eintrag.Id);
            LoadData();
        }

        

        // ----- Zusatzpositionen -----

        private void LoadZusatz()
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null || eintrag.Id == 0)
            {
                _zusaetze = new List<GlobalePreislisteZusatz>();
                _zusatzBinding.DataSource = new SortableBindingList<GlobalePreislisteZusatz>(_zusaetze);
                SetZusatzEnabled(false);
                return;
            }

            SetZusatzEnabled(true);
            _zusaetze = _repository.GetZusatz(eintrag.Id);
            var preise = PreiseRepository.GetZusatzPreiseMitStart(eintrag.Id, Termin.ToYyyyMmDd(_lastDate));
            foreach (var zusatz in _zusaetze)
            {
                if (preise.TryGetValue(zusatz.Id, out var info))
                {
                    zusatz.Preis = info.Preis;
                    zusatz.StartDatum = info.GueltigAb;
                }
                else
                {
                    zusatz.Preis = null;
                    zusatz.StartDatum = 0;
                }
            }
            _zusatzBinding.DataSource = new SortableBindingList<GlobalePreislisteZusatz>(_zusaetze);
            if (_zusaetze.Count > 0)
            {
                dataGridViewZusatz.ClearSelection();
                dataGridViewZusatz.Rows[0].Selected = true;
            }
        }

        private GlobalePreislisteZusatz? SelectedZusatz =>
            dataGridViewZusatz.SelectedRows.Count > 0
                ? dataGridViewZusatz.SelectedRows[0].DataBoundItem as GlobalePreislisteZusatz
                : null;

        private void dataGridViewZusatz_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnZusatzSpeichern_Click(sender, EventArgs.Empty);
            }
        }



        private void btnZusatzNeu_Click(object? sender, EventArgs e)
        {
            var eintrag = SelectedEintrag;
            if (eintrag == null || eintrag.Id == 0)
            {
                MessageBox.Show("Bitte zuerst die Position speichern.", "Zusatzpositionen",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ZusatzBearbeitenDialog(eintrag.Id);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.InsertZusatz(dialog.Zusatz);
            var id = dialog.Zusatz.Id;
            SavePreis(dialog.Zusatz.SpeisekarteId, id, dialog.Preis,dialog.Zusatz.Bezeichnung);
            LoadZusatz();
            SelectZusatzById(id);
        }

        private void btnZusatzSpeichern_Click(object? sender, EventArgs e)
        {
            var zusatz = SelectedZusatz;
            if (zusatz == null || zusatz.Id == 0)
            {
                MessageBox.Show("Bitte zuerst eine Zusatzposition zum Ändern auswählen.", "Zusatzpositionen",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ZusatzBearbeitenDialog(zusatz.SpeisekarteId, zusatz);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _repository.UpdateZusatz(dialog.Zusatz);
            var id = dialog.Zusatz.Id;
            SavePreis(dialog.Zusatz.SpeisekarteId, id, dialog.Preis,dialog.Zusatz.Bezeichnung);
            LoadZusatz();
            SelectZusatzById(id);
        }

        private void btnZusatzLoeschen_Click(object? sender, EventArgs e)
        {
            var zusatz = SelectedZusatz;
            if (zusatz == null || zusatz.Id == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Zusatzposition \"{zusatz.Bezeichnung}\" wirklich löschen?", "Zusatzpositionen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _repository.DeleteZusatz(zusatz.Id);
            LoadZusatz();
        }

        // ----- Hilfsmethoden -----

        private void SavePreis(long preislisteId, long zusatzId, decimal preis, string name)
        {
            var zeile = new PreisZeile
            {
                PreislisteId = preislisteId,
                ZusatzId = zusatzId,
                Preis = preis,
                Name = name
            };
            _preiseRepository.Speichern(Termin.ToYyyyMmDd(_lastDate), new[] { zeile });
        }

        /// <summary>
        /// Speichert den Preis mit explizitem Gültigkeitszeitraum (GueltigAb/GueltigBis)
        /// aus dem Bearbeiten-Dialog. Bei einem neuen Datensatz (altGueltigAb = 0) wird
        /// in GlobalerPreis automatisch ein Datensatz angelegt.
        /// </summary>
        private void SavePreis(long preislisteId, long zusatzId, decimal preis, string name,
            int altGueltigAb, int neuGueltigAb, int gueltigBis)
        {
            if (neuGueltigAb <= 0)
            {
                neuGueltigAb = Termin.ToYyyyMmDd(_lastDate);
            }
            _preiseRepository.SpeichereGueltigkeit(
                preislisteId, zusatzId, altGueltigAb, neuGueltigAb, gueltigBis, preis);
        }

        private void SetZusatzEnabled(bool enabled)
        {
            btnZusatzNeu.Enabled = enabled;
            btnZusatzSpeichern.Enabled = enabled;
            btnZusatzLoeschen.Enabled = enabled;
        }

        private void SelectMainById(long id)
        {
            for (int i = 0; i < _eintraege.Count; i++)
            {
                if (_eintraege[i].Id == id)
                {
                    _mainBinding.Position = i;
                    break;
                }
            }
        }

        private void SelectZusatzById(long id)
        {
            for (int i = 0; i < _zusaetze.Count; i++)
            {
                if (_zusaetze[i].Id == id)
                {
                    _zusatzBinding.Position = i;
                    break;
                }
            }
        }

        private void btnNeuerTermin_Click(object sender, EventArgs e)
        {
            var vorauswahl = cboDatumAb.SelectedItem is DateTime dt ? dt : DateTime.Today;
            using var dialog = new NeuerTerminDialog(vorauswahl);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            AddDatum(dialog.AusgewaehltesDatum);
        }

        private void AddDatum(DateTime datum)
        {
            datum = datum.Date;
            if (!_datumsListe.Contains(datum))
            {
                _datumsListe.Add(datum);
                _datumsListe.Sort();
            }
            int datumInt = Termin.ToYyyyMmDd(_lastDate);
            int newDatumInt = Termin.ToYyyyMmDd(datum);
            var preise = PreiseRepository.GetAllAtDate(datumInt);
            foreach (var zeile in preise)
            {
                var existing = PreiseRepository.GetAllAtDate(newDatumInt)
                    .FirstOrDefault(p => p.GlobalePreislisteId == zeile.GlobalePreislisteId && p.ZusatzId == zeile.ZusatzId);
                if (existing != null)
                {
                    // Bereits ein Preis für diesen Termin vorhanden, überspringen.
                    continue;
                }
                PreiseRepository.Insert(new GlobalerPreis
                {
                    GlobalePreislisteId = zeile.GlobalePreislisteId,
                    ZusatzId = zeile.ZusatzId,
                    GueltigAb = newDatumInt,
                    GueltigBis = 0,
                    Preis = zeile.Preis
                });
            }
            cboDatumAb.DataSource = new List<DateTime>(_datumsListe);
            cboDatumAb.SelectedItem = datum;

        }

        private void cboDatumAb_SelectedIndexChanged(object sender, EventArgs e)
        {

            // Unteres Feld der oberen Auswahl angleichen.
            if (cboDatumAb.SelectedItem is DateTime dt)
            {
                _lastDate = dt;
                // Sicherstellen, dass für dieses Datum alle Preise vorhanden sind.
                // Fehlende Preise werden aus der Vorperiode übernommen (sofern vorhanden).
                PreiseRepository.UebernehmeFehlendeVorperiodenPreise(Termin.ToYyyyMmDd(_lastDate));
                LoadData();
            }
        }
    }
}

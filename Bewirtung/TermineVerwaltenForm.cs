using Bewirtung.Data;

namespace Bewirtung
{
    /// <summary>
    /// Verwaltung der Termine (Tabelle Termin): Verein je Kalendertag über eine ComboBox
    /// wählen und per Speichern-Button anlegen, ändern oder (bei "Keine Bewirtung") löschen.
    /// </summary>
    public partial class TermineVerwaltenForm : Form
    {
        private readonly TerminRepository _repository = new();
        private readonly VereinRepository _vereinRepository = new();
        private List<Termin> _termine = new();

        // Unterdrückt das Aktivieren des Speichern-Buttons bei programmatischen Änderungen.
        private bool _aktualisiereAuswahlIntern;

        // --- Speisekarte-Tab ---
        private readonly SpeisekarteRepository _speisekarteRepository = new();
        private List<SpeisekartenZeile> _zeilen = new();
        private bool _ladenSpeisekarte;

        // --- Tische-Tab ---
        private readonly TischTerminRepository _tischTerminRepository = new();
        private readonly TischRepository _tischRepository = new();
        private readonly BindingSource _tischBinding = new();
        private List<TischTermin> _tischEintraege = new();

        // --- Helfer-Tab ---
        private readonly BenutzerRepository _benutzerRepository = new();
        private readonly BindingSource _helferBinding = new();
        private List<Benutzer> _helfer = new();

        public TermineVerwaltenForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            ConfigureSpeisekarteGrids();
            ConfigureTischeGrid();
            ConfigureHelferGrid();
            VerdrahteTabEvents();
            Load += TermineVerwaltenForm_Load;
        }

        /// <summary>Verdrahtet die Controls innerhalb der Tabs mit ihren Handlern.</summary>
        private void VerdrahteTabEvents()
        {
            // Speisekarte
            button1.Click += BtnSpeisekarteSpeichern_Click;
            gridGlobal.CellValueChanged += GridGlobal_CellValueChanged;
            gridGlobal.CurrentCellDirtyStateChanged += GridGlobal_CurrentCellDirtyStateChanged;

            // Tische
            btnNeu.Click += btnTischNeu_Click;
            btnAendern.Click += btnTischAendern_Click;
            btnLoeschen.Click += btnTischLoeschen_Click;
            dataGridViewTische.CellDoubleClick += dataGridViewTische_CellDoubleClick;

            // Helfer
            btnHelferHinzufuegen.Click += btnHelferHinzufuegen_Click;
            btnHelferEntfernen.Click += btnHelferEntfernen_Click;
        }

        private void TermineVerwaltenForm_Load(object? sender, EventArgs e)
        {
            LadeVereine();
            LoadData();
            AktualisiereVereinAuswahl();
        }
        private void LadeVereine()
        {
            var vereine = new List<Verein>
            {
                new() { Id = 0, Name = "Keine Bewirtung" }
            };
            vereine.AddRange(_vereinRepository.GetAll());

            cmbVereine.DisplayMember = nameof(Verein.Name);
            cmbVereine.ValueMember = nameof(Verein.Id);
            cmbVereine.DataSource = vereine;
            cmbVereine.SelectedIndex = 0;
        }

        private void LoadData()
        {
            _termine = _repository.GetAll();
            MarkiereTermine();
        }

        /// <summary>Markiert alle Termin-Tage fett im Kalender.</summary>
        private void MarkiereTermine()
        {
            monthCalendar1.RemoveAllBoldedDates();
            foreach (var termin in _termine)
            {
                monthCalendar1.AddBoldedDate(termin.Datum.Date);
            }
            monthCalendar1.UpdateBoldedDates();
        }

        /// <summary>
        /// Der am aktuell gewählten Kalendertag hinterlegte Termin.
        /// Gibt es mehrere Termine (Vereine) an diesem Tag, wird der erste verwendet.
        /// </summary>
        private Termin? SelectedTermin
        {
            get
            {
                var tag = monthCalendar1.SelectionStart.Date;
                var datum = Termin.ToYyyyMmDd(tag);
                return _termine.FirstOrDefault(t => t.BewirtungsDatum == datum);
            }
        }

        private void monthCalendar1_DateChanged(object? sender, DateRangeEventArgs e)
        {
            AktualisiereVereinAuswahl();
        }

        /// <summary>
        /// Stellt die ComboBox auf den Verein des am gewählten Tag hinterlegten Termins ein.
        /// Gibt es keinen Termin, wird "Keine Bewirtung" ausgewählt. Der Speichern-Button
        /// wird dabei deaktiviert, da noch keine Benutzeränderung vorliegt.
        /// </summary>
        private void AktualisiereVereinAuswahl()
        {
            _aktualisiereAuswahlIntern = true;
            var termin = SelectedTermin;
            cmbVereine.SelectedValue = termin?.VereinId ?? 0L;
            _aktualisiereAuswahlIntern = false;
            btnSpeichern.Enabled = false;
            AktualisiereTabs();
        }

        private void cmbVereine_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Nur echte Benutzeränderungen aktivieren den Speichern-Button.
            if (!_aktualisiereAuswahlIntern)
            {
                btnSpeichern.Enabled = true;
            }
            AktualisiereTabs();
        }

        /// <summary>
        /// Legt über den VereinBearbeitenDialog einen neuen Verein an, fügt ihn der
        /// ComboBox hinzu und wählt ihn aus.
        /// </summary>
        private void btnNeuerVerein_Click(object? sender, EventArgs e)
        {
            using var dialog = new VereinBearbeitenDialog(_vereinRepository.GetBenutzer());
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var id = _vereinRepository.Insert(dialog.Verein);

            LadeVereine();
            cmbVereine.SelectedValue = id;
        }

        /// <summary>
        /// Speichert die Vereinszuordnung für den gewählten Kalendertag: legt einen neuen
        /// Termin an, ändert einen vorhandenen oder löscht ihn bei "Keine Bewirtung".
        /// </summary>
        private void btnSpeichern_Click(object? sender, EventArgs e)
        {
            var tag = monthCalendar1.SelectionStart.Date;
            var datum = Termin.ToYyyyMmDd(tag);
            var vorhanden = SelectedTermin;
            var neueVereinId = cmbVereine.SelectedValue is long id ? id : 0L;

            try
            {
                if (neueVereinId == 0)
                {
                    // "Keine Bewirtung": vorhandenen Termin löschen.
                    if (vorhanden != null)
                    {
                        _repository.Delete(vorhanden.BewirtungsDatum, vorhanden.VereinId);
                    }
                }
                else if (vorhanden == null)
                {
                    // Noch kein Termin an diesem Tag: neuen anlegen.
                    _repository.Insert(new Termin { BewirtungsDatum = datum, VereinId = neueVereinId });
                }
                else if (vorhanden.VereinId != neueVereinId)
                {
                    // Vorhandenen Termin auf den neuen Verein ändern.
                    _repository.Update(vorhanden.BewirtungsDatum, vorhanden.VereinId,
                        new Termin { BewirtungsDatum = datum, VereinId = neueVereinId });
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                MessageBox.Show("Für diesen Verein existiert bereits ein Termin an diesem Datum.",
                    "Termine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadData();
            SelectTermin(datum, neueVereinId);
        }

        private void SelectTermin(int datum, long vereinId)
        {
            var tag = Termin.FromYyyyMmDd(datum).Date;
            monthCalendar1.SelectionStart = tag;
            monthCalendar1.SelectionEnd = tag;
            AktualisiereVereinAuswahl();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Aktuellen Tab und dessen Text abrufen
            TabPage aktuelleSeite = tabControl1.TabPages[e.Index];
            string tabText = aktuelleSeite.Text;

            // Standard-Schriftart für nicht-selektierte Tabs definieren
            Font tabFont = e.Font;

            // Prüfen, ob der aktuell zu zeichnende Tab der ausgewählte ist
            if (e.Index == tabControl1.SelectedIndex)
            {
                // Wenn ja, erstelle die Schriftart in FETT
                tabFont = new Font(e.Font, FontStyle.Bold);
            }

            // Hintergrund des Tabs zeichnen (Standard-Grau/Weiß)
            e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            // Text-Formatierung: Zentriert im Tab ausrichten
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            // Den Text mit der passenden Schriftart auf den Tab zeichnen
            TextRenderer.DrawText(e.Graphics, tabText, tabFont, e.Bounds, Color.Black, flags);

            // Ressourcen aufräumen, falls eine neue Fett-Schriftart erstellt wurde
            if (e.Index == tabControl1.SelectedIndex)
            {
                tabFont.Dispose();
            }
        }

        // ===================== Tab-Integration =====================

        /// <summary>
        /// Der aktuell im Kalender gewählte, in der Datenbank vorhandene Termin
        /// (Datum + in der ComboBox gewählter Verein). Null, wenn kein gültiger Termin.
        /// </summary>
        private Termin? AktuellerTermin
        {
            get
            {
                var tag = monthCalendar1.SelectionStart.Date;
                var datum = Termin.ToYyyyMmDd(tag);
                var vereinId = cmbVereine.SelectedValue is long id ? id : 0L;
                if (vereinId == 0)
                {
                    return null;
                }
                return _termine.FirstOrDefault(
                    t => t.BewirtungsDatum == datum && t.VereinId == vereinId);
            }
        }

        /// <summary>
        /// Lädt die drei Tabs für den aktuell gewählten Termin. Ist kein gültiger
        /// Termin ausgewählt, wird das TabControl ausgeblendet.
        /// </summary>
        private void AktualisiereTabs()
        {
            var termin = AktuellerTermin;
            tabControl1.Visible = termin != null;
            if (termin == null)
            {
                return;
            }

            LoadSpeisekarte(termin);
            LoadTische(termin);
            LoadHelfer(termin);
        }

        // ----- Speisekarte -----

        private void ConfigureSpeisekarteGrids()
        {
            gridGlobal.AutoGenerateColumns = false;
            gridGlobal.Columns.Clear();
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Produkt),
                HeaderText = "Produkt",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.ZusatzText),
                HeaderText = "Zusatz",
                ReadOnly = true,
                Width = 160
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.KategorieText),
                HeaderText = "Kategorie",
                ReadOnly = true,
                Width = 120
            });
            gridGlobal.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.GlobalPreis),
                HeaderText = "Globaler Preis",
                ReadOnly = true,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 130
            });
            gridGlobal.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Ausgewaehlt),
                HeaderText = "Auswahl",
                Name = "Ausgewaehlt",
                Width = 90
            });

            gridAuswahl.AutoGenerateColumns = false;
            gridAuswahl.Columns.Clear();
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.KategorieText),
                HeaderText = "Kategorie",
                ReadOnly = true,
                Width = 120
            });
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.Produkt),
                HeaderText = "Position",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            gridAuswahl.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SpeisekartenZeile.TerminPreis),
                HeaderText = "Preis",
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120
            });

            GridSortHelper.EnableHeaderSorting(gridGlobal);
            GridSortHelper.EnableHeaderSorting(gridAuswahl);
        }

        private void LoadSpeisekarte(Termin termin)
        {
            _ladenSpeisekarte = true;
            _zeilen = _speisekarteRepository.GetSpeisekarte(termin.BewirtungsDatum, termin.VereinId);
            gridGlobal.DataSource = new BindingSource { DataSource = new SortableBindingList<SpeisekartenZeile>(_zeilen) };
            _ladenSpeisekarte = false;
            RefreshAuswahl();
        }

        /// <summary>
        /// Öffnet die globale Preislisten-Verwaltung modal und sofort im Neu-Modus,
        /// um eine neue Speisekartenposition zu erfassen. Nach dem Schließen wird
        /// die Speisekarte für den aktuellen Termin neu geladen.
        /// </summary>
        private void cmdNeuePositionSpeiseplan_Click(object? sender, EventArgs e)
        {
            using (var form = new GlobalePreislisteForm(sofortNeu: true))
            {
                form.ShowDialog(this);
            }

            var termin = AktuellerTermin;
            if (termin != null)
            {
                LoadSpeisekarte(termin);
            }
        }

        private void GridGlobal_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (gridGlobal.IsCurrentCellDirty && gridGlobal.CurrentCell is DataGridViewCheckBoxCell)
            {
                gridGlobal.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void GridGlobal_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_ladenSpeisekarte || e.RowIndex < 0)
            {
                return;
            }

            if (gridGlobal.Columns[e.ColumnIndex].Name == "Ausgewaehlt")
            {
                if (e.RowIndex < _zeilen.Count)
                {
                    var zeile = gridGlobal.Rows[e.RowIndex].DataBoundItem as SpeisekartenZeile;
                    if (zeile is { Ausgewaehlt: true } && zeile.TerminPreis <= 0m)
                    {
                        zeile.TerminPreis = zeile.GlobalPreis ?? 0m;
                    }
                }

                RefreshAuswahl();
            }
        }

        private void RefreshAuswahl()
        {
            var ausgewaehlt = _zeilen.Where(z => z.Ausgewaehlt).ToList();
            gridAuswahl.DataSource = new BindingSource { DataSource = new SortableBindingList<SpeisekartenZeile>(ausgewaehlt) };
        }

        private void BtnSpeisekarteSpeichern_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            if (termin == null)
            {
                return;
            }

            gridAuswahl.EndEdit();
            gridGlobal.EndEdit();

            _speisekarteRepository.Speichern(termin.BewirtungsDatum, termin.VereinId, _zeilen);
            MessageBox.Show("Die Speisekarte für den Termin wurde gespeichert.", "Gespeichert",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ----- Tische zuordnen -----

        private void ConfigureTischeGrid()
        {
            dataGridViewTische.AutoGenerateColumns = false;
            dataGridViewTische.Columns.Clear();
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.DatumText),
                HeaderText = "Datum",
                Width = 140
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.Tischnummer),
                HeaderText = "Tischnummer",
                Width = 160
            });
            dataGridViewTische.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(TischTermin.BereichName),
                HeaderText = "Bereich",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTische.DataSource = _tischBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewTische);
        }

        private void LoadTische(Termin termin)
        {
            _tischEintraege = _tischTerminRepository.GetAll();
            var gefiltert = _tischEintraege.Where(
                x => x.Bewirtungsdatum == termin.BewirtungsDatum && x.VereinId == termin.VereinId).ToList();
            _tischBinding.DataSource = new SortableBindingList<TischTermin>(gefiltert);
            if (dataGridViewTische.Rows.Count > 0)
            {
                dataGridViewTische.ClearSelection();
                dataGridViewTische.Rows[0].Selected = true;
            }
        }

        private TischTermin? SelectedTischEintrag =>
            dataGridViewTische.SelectedRows.Count > 0
                ? dataGridViewTische.SelectedRows[0].DataBoundItem as TischTermin
                : null;

        private void btnTischNeu_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            if (termin == null)
            {
                return;
            }

            var vorhandene = _tischEintraege
                .Where(x => x.Bewirtungsdatum == termin.BewirtungsDatum && x.VereinId == termin.VereinId)
                .Select(x => x.Tischnummer)
                .ToHashSet();

            using var dialog = new TischTerminBearbeitenDialog(
                _repository.GetAll(), _tischRepository.GetAll(),
                new TischTermin { Bewirtungsdatum = termin.BewirtungsDatum, VereinId = termin.VereinId },
                vorhandene);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            int uebersprungen = 0;
            foreach (var tischnummer in dialog.AusgewaehlteTischnummern)
            {
                var neu = new TischTermin
                {
                    Bewirtungsdatum = dialog.Eintrag.Bewirtungsdatum,
                    VereinId = dialog.Eintrag.VereinId,
                    Tischnummer = tischnummer
                };
                try
                {
                    _tischTerminRepository.Insert(neu);
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    uebersprungen++;
                }
            }

            if (uebersprungen > 0)
            {
                MessageBox.Show($"{uebersprungen} Tisch(e) waren bereits vorhanden.",
                    "Tische pro Termin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LoadTische(termin);
        }

        private void btnTischAendern_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            if (termin == null)
            {
                return;
            }

            var altDatum = termin.BewirtungsDatum;
            var altVereinId = termin.VereinId;

            var vorhandene = _tischEintraege
                .Where(x => x.Bewirtungsdatum == altDatum && x.VereinId == altVereinId)
                .Select(x => x.Tischnummer)
                .ToHashSet();

            using var dialog = new TischTerminBearbeitenDialog(
                _repository.GetAll(), _tischRepository.GetAll(),
                new TischTermin { Bewirtungsdatum = altDatum, VereinId = altVereinId },
                vorhandene);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var neuDatum = dialog.Eintrag.Bewirtungsdatum;
            var neuVereinId = dialog.Eintrag.VereinId;
            var neueAuswahl = dialog.AusgewaehlteTischnummern.ToHashSet();

            try
            {
                if (neuDatum == altDatum && neuVereinId == altVereinId)
                {
                    foreach (var nummer in vorhandene.Except(neueAuswahl))
                    {
                        _tischTerminRepository.Delete(altDatum, altVereinId, nummer);
                    }
                    foreach (var nummer in neueAuswahl.Except(vorhandene))
                    {
                        _tischTerminRepository.Insert(new TischTermin
                        {
                            Bewirtungsdatum = neuDatum,
                            VereinId = neuVereinId,
                            Tischnummer = nummer
                        });
                    }
                }
                else
                {
                    foreach (var nummer in vorhandene)
                    {
                        _tischTerminRepository.Delete(altDatum, altVereinId, nummer);
                    }
                    foreach (var nummer in neueAuswahl)
                    {
                        _tischTerminRepository.Insert(new TischTermin
                        {
                            Bewirtungsdatum = neuDatum,
                            VereinId = neuVereinId,
                            Tischnummer = nummer
                        });
                    }
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                MessageBox.Show("Dieser Tisch ist diesem Termin bereits zugeordnet.",
                    "Tische pro Termin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadTische(termin);
        }

        private void btnTischLoeschen_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            var eintrag = SelectedTischEintrag;
            if (termin == null || eintrag == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Tisch {eintrag.Tischnummer} von Termin {eintrag.TerminAnzeige} wirklich entfernen?",
                "Tische pro Termin", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _tischTerminRepository.Delete(eintrag.Bewirtungsdatum, eintrag.VereinId, eintrag.Tischnummer);
            LoadTische(termin);
        }

        private void dataGridViewTische_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnTischAendern_Click(sender, EventArgs.Empty);
            }
        }

        // ----- Helfer zuordnen -----

        private void ConfigureHelferGrid()
        {
            dataGridViewHelfer.AutoGenerateColumns = false;
            dataGridViewHelfer.Columns.Clear();
            dataGridViewHelfer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.Name),
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewHelfer.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Benutzer.VereinName),
                HeaderText = "Verein",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewHelfer.DataSource = _helferBinding;
            GridSortHelper.EnableHeaderSorting(dataGridViewHelfer);
        }

        private void LoadHelfer(Termin termin)
        {
            _helfer = _repository.GetHelfer(termin.BewirtungsDatum, termin.VereinId);
            _helferBinding.DataSource = new SortableBindingList<Benutzer>(_helfer);
        }

        private Benutzer? SelectedHelfer =>
            dataGridViewHelfer.SelectedRows.Count > 0
                ? dataGridViewHelfer.SelectedRows[0].DataBoundItem as Benutzer
                : null;

        private void btnHelferHinzufuegen_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            if (termin == null)
            {
                return;
            }

            var vorhandeneIds = _helfer.Select(h => h.UserId).ToHashSet();
            var verfuegbar = _benutzerRepository.GetAll()
                .Where(b => !vorhandeneIds.Contains(b.UserId))
                .ToList();

            if (verfuegbar.Count == 0)
            {
                MessageBox.Show("Alle Benutzer sind diesem Termin bereits als Helfer zugeordnet.", "Helfer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new HelferAuswahlDialog(verfuegbar);
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedUserIds.Count == 0)
            {
                return;
            }

            foreach (var userId in dialog.SelectedUserIds)
            {
                _repository.AddHelfer(termin.BewirtungsDatum, termin.VereinId, userId);
            }
            LoadHelfer(termin);
        }

        private void btnHelferEntfernen_Click(object? sender, EventArgs e)
        {
            var termin = AktuellerTermin;
            var helfer = SelectedHelfer;
            if (termin == null || helfer == null)
            {
                return;
            }

            _repository.RemoveHelfer(termin.BewirtungsDatum, termin.VereinId, helfer.UserId);
            LoadHelfer(termin);
        }
    }
}

namespace Bewirtung
{
    partial class TermineVerwaltenForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnSpeichern = new Button();
            btnNeuerVerein = new Button();
            monthCalendar1 = new MonthCalendar();
            cmbVereine = new ComboBox();
            tabControl1 = new TabControl();
            tabSpeisekarte = new TabPage();
            button1 = new Button();
            split = new SplitContainer();
            gridGlobal = new DataGridView();
            gridAuswahl = new DataGridView();
            tabTische = new TabPage();
            btnNeu = new Button();
            btnAendern = new Button();
            btnLoeschen = new Button();
            dataGridViewTische = new DataGridView();
            tabHelfer = new TabPage();
            dataGridViewHelfer = new DataGridView();
            btnHelferEntfernen = new Button();
            btnHelferHinzufuegen = new Button();
            tabControl1.SuspendLayout();
            tabSpeisekarte.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)split).BeginInit();
            split.Panel1.SuspendLayout();
            split.Panel2.SuspendLayout();
            split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridGlobal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridAuswahl).BeginInit();
            tabTische.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).BeginInit();
            tabHelfer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewHelfer).BeginInit();
            SuspendLayout();
            // 
            // btnSpeichern
            // 
            btnSpeichern.Enabled = false;
            btnSpeichern.Location = new Point(585, 17);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 5;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            btnSpeichern.Click += btnSpeichern_Click;
            // 
            // btnNeuerVerein
            // 
            btnNeuerVerein.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNeuerVerein.Location = new Point(1371, 17);
            btnNeuerVerein.Name = "btnNeuerVerein";
            btnNeuerVerein.Size = new Size(320, 40);
            btnNeuerVerein.TabIndex = 6;
            btnNeuerVerein.Text = "Neuer Verein hinzufügen";
            btnNeuerVerein.UseVisualStyleBackColor = true;
            btnNeuerVerein.Click += btnNeuerVerein_Click;
            // 
            // monthCalendar1
            // 
            monthCalendar1.CalendarDimensions = new Size(1, 3);
            monthCalendar1.Location = new Point(5, 6);
            monthCalendar1.MaxSelectionCount = 1;
            monthCalendar1.Name = "monthCalendar1";
            monthCalendar1.TabIndex = 3;
            monthCalendar1.DateChanged += monthCalendar1_DateChanged;
            // 
            // cmbVereine
            // 
            cmbVereine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVereine.Location = new Point(257, 22);
            cmbVereine.Name = "cmbVereine";
            cmbVereine.Size = new Size(320, 33);
            cmbVereine.TabIndex = 4;
            cmbVereine.SelectedIndexChanged += cmbVereine_SelectedIndexChanged;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.Controls.Add(tabSpeisekarte);
            tabControl1.Controls.Add(tabTische);
            tabControl1.Controls.Add(tabHelfer);
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.ItemSize = new Size(160, 33);
            tabControl1.Location = new Point(257, 63);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1443, 797);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 7;
            tabControl1.DrawItem += tabControl1_DrawItem;
            // 
            // tabSpeisekarte
            // 
            tabSpeisekarte.BackColor = SystemColors.Control;
            tabSpeisekarte.BorderStyle = BorderStyle.FixedSingle;
            tabSpeisekarte.Controls.Add(button1);
            tabSpeisekarte.Controls.Add(split);
            tabSpeisekarte.Location = new Point(4, 37);
            tabSpeisekarte.Name = "tabSpeisekarte";
            tabSpeisekarte.Padding = new Padding(3, 50, 3, 3);
            tabSpeisekarte.Size = new Size(1435, 756);
            tabSpeisekarte.TabIndex = 0;
            tabSpeisekarte.Text = "Speisekarte";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(1352, 6);
            button1.Name = "button1";
            button1.Size = new Size(140, 38);
            button1.TabIndex = 3;
            button1.Text = "Speichern";
            // 
            // split
            // 
            split.Dock = DockStyle.Fill;
            split.Location = new Point(3, 50);
            split.Name = "split";
            // 
            // split.Panel1
            // 
            split.Panel1.Controls.Add(gridGlobal);
            // 
            // split.Panel2
            // 
            split.Panel2.Controls.Add(gridAuswahl);
            split.Size = new Size(1427, 701);
            split.SplitterDistance = 903;
            split.SplitterWidth = 6;
            split.TabIndex = 2;
            // 
            // gridGlobal
            // 
            gridGlobal.AllowUserToAddRows = false;
            gridGlobal.AllowUserToDeleteRows = false;
            gridGlobal.ColumnHeadersHeight = 34;
            gridGlobal.Dock = DockStyle.Fill;
            gridGlobal.Location = new Point(0, 0);
            gridGlobal.Name = "gridGlobal";
            gridGlobal.RowHeadersVisible = false;
            gridGlobal.RowHeadersWidth = 62;
            gridGlobal.SelectionMode = DataGridViewSelectionMode.CellSelect;
            gridGlobal.Size = new Size(903, 701);
            gridGlobal.TabIndex = 0;
            // 
            // gridAuswahl
            // 
            gridAuswahl.AllowUserToAddRows = false;
            gridAuswahl.AllowUserToDeleteRows = false;
            gridAuswahl.ColumnHeadersHeight = 34;
            gridAuswahl.Dock = DockStyle.Fill;
            gridAuswahl.Location = new Point(0, 0);
            gridAuswahl.Name = "gridAuswahl";
            gridAuswahl.RowHeadersVisible = false;
            gridAuswahl.RowHeadersWidth = 62;
            gridAuswahl.Size = new Size(518, 701);
            gridAuswahl.TabIndex = 0;
            // 
            // tabTische
            // 
            tabTische.BackColor = SystemColors.Control;
            tabTische.BorderStyle = BorderStyle.FixedSingle;
            tabTische.Controls.Add(btnNeu);
            tabTische.Controls.Add(btnAendern);
            tabTische.Controls.Add(btnLoeschen);
            tabTische.Controls.Add(dataGridViewTische);
            tabTische.Location = new Point(4, 37);
            tabTische.Name = "tabTische";
            tabTische.Padding = new Padding(3);
            tabTische.Size = new Size(1435, 756);
            tabTische.TabIndex = 1;
            tabTische.Text = "Tische zuordnen";
            // 
            // btnNeu
            // 
            btnNeu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNeu.Location = new Point(1075, 6);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(120, 40);
            btnNeu.TabIndex = 3;
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            // 
            // btnAendern
            // 
            btnAendern.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAendern.Location = new Point(1191, 6);
            btnAendern.Name = "btnAendern";
            btnAendern.Size = new Size(120, 40);
            btnAendern.TabIndex = 5;
            btnAendern.Text = "Ändern";
            btnAendern.UseVisualStyleBackColor = true;
            // 
            // btnLoeschen
            // 
            btnLoeschen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoeschen.Location = new Point(1307, 6);
            btnLoeschen.Name = "btnLoeschen";
            btnLoeschen.Size = new Size(120, 40);
            btnLoeschen.TabIndex = 6;
            btnLoeschen.Text = "Löschen";
            btnLoeschen.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTische
            // 
            dataGridViewTische.AllowUserToAddRows = false;
            dataGridViewTische.AllowUserToDeleteRows = false;
            dataGridViewTische.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTische.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTische.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewTische.Location = new Point(6, 45);
            dataGridViewTische.MultiSelect = false;
            dataGridViewTische.Name = "dataGridViewTische";
            dataGridViewTische.ReadOnly = true;
            dataGridViewTische.RowHeadersVisible = false;
            dataGridViewTische.RowHeadersWidth = 62;
            dataGridViewTische.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTische.Size = new Size(1421, 700);
            dataGridViewTische.TabIndex = 4;
            // 
            // tabHelfer
            // 
            tabHelfer.BackColor = SystemColors.Control;
            tabHelfer.BorderStyle = BorderStyle.FixedSingle;
            tabHelfer.Controls.Add(dataGridViewHelfer);
            tabHelfer.Controls.Add(btnHelferEntfernen);
            tabHelfer.Controls.Add(btnHelferHinzufuegen);
            tabHelfer.Location = new Point(4, 37);
            tabHelfer.Name = "tabHelfer";
            tabHelfer.Size = new Size(1435, 756);
            tabHelfer.TabIndex = 2;
            tabHelfer.Text = "Helfer zuordnen";
            // 
            // dataGridViewHelfer
            // 
            dataGridViewHelfer.AllowUserToAddRows = false;
            dataGridViewHelfer.AllowUserToDeleteRows = false;
            dataGridViewHelfer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewHelfer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewHelfer.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewHelfer.Location = new Point(12, 47);
            dataGridViewHelfer.MultiSelect = false;
            dataGridViewHelfer.Name = "dataGridViewHelfer";
            dataGridViewHelfer.ReadOnly = true;
            dataGridViewHelfer.RowHeadersVisible = false;
            dataGridViewHelfer.RowHeadersWidth = 62;
            dataGridViewHelfer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewHelfer.Size = new Size(1415, 700);
            dataGridViewHelfer.TabIndex = 5;
            // 
            // btnHelferEntfernen
            // 
            btnHelferEntfernen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelferEntfernen.Location = new Point(1307, 5);
            btnHelferEntfernen.Name = "btnHelferEntfernen";
            btnHelferEntfernen.Size = new Size(120, 36);
            btnHelferEntfernen.TabIndex = 4;
            btnHelferEntfernen.Text = "Entfernen";
            btnHelferEntfernen.UseVisualStyleBackColor = true;
            // 
            // btnHelferHinzufuegen
            // 
            btnHelferHinzufuegen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelferHinzufuegen.Location = new Point(1170, 5);
            btnHelferHinzufuegen.Name = "btnHelferHinzufuegen";
            btnHelferHinzufuegen.Size = new Size(140, 36);
            btnHelferHinzufuegen.TabIndex = 3;
            btnHelferHinzufuegen.Text = "Hinzufügen";
            btnHelferHinzufuegen.UseVisualStyleBackColor = true;
            // 
            // TermineVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1701, 865);
            Controls.Add(tabControl1);
            Controls.Add(cmbVereine);
            Controls.Add(monthCalendar1);
            Controls.Add(btnSpeichern);
            Controls.Add(btnNeuerVerein);
            Name = "TermineVerwaltenForm";
            Text = "Termine verwalten";
            tabControl1.ResumeLayout(false);
            tabSpeisekarte.ResumeLayout(false);
            split.Panel1.ResumeLayout(false);
            split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)split).EndInit();
            split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridGlobal).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridAuswahl).EndInit();
            tabTische.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).EndInit();
            tabHelfer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewHelfer).EndInit();
            ResumeLayout(false);
        }

        private Button btnSpeichern;
        private Button btnNeuerVerein;
        private MonthCalendar monthCalendar1;
        private ComboBox cmbVereine;
        private TabControl tabControl1;
        private TabPage tabSpeisekarte;
        private TabPage tabTische;
        private TabPage tabHelfer;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
        private DataGridView dataGridViewTische;
        private DataGridView dataGridViewHelfer;
        private Button btnHelferEntfernen;
        private Button btnHelferHinzufuegen;
        private SplitContainer split;
        private DataGridView gridGlobal;
        private DataGridView gridAuswahl;
        private Button button1;
    }
}

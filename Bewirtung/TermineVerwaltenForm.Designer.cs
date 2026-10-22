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
            cmdDrucken = new Button();
            cmdNeuePositionSpeiseplan = new Button();
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
            groupBox1 = new GroupBox();
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
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSpeichern
            // 
            btnSpeichern.Enabled = false;
            btnSpeichern.Location = new Point(417, 21);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 5;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            btnSpeichern.Click += btnSpeichern_Click;
            // 
            // btnNeuerVerein
            // 
            btnNeuerVerein.Location = new Point(555, 21);
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
            cmbVereine.Location = new Point(14, 26);
            cmbVereine.Name = "cmbVereine";
            cmbVereine.Size = new Size(383, 33);
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
            tabControl1.Location = new Point(257, 81);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1443, 779);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 7;
            tabControl1.DrawItem += tabControl1_DrawItem;
            // 
            // tabSpeisekarte
            // 
            tabSpeisekarte.BackColor = SystemColors.Control;
            tabSpeisekarte.BorderStyle = BorderStyle.FixedSingle;
            tabSpeisekarte.Controls.Add(cmdDrucken);
            tabSpeisekarte.Controls.Add(cmdNeuePositionSpeiseplan);
            tabSpeisekarte.Controls.Add(button1);
            tabSpeisekarte.Controls.Add(split);
            tabSpeisekarte.Location = new Point(4, 37);
            tabSpeisekarte.Name = "tabSpeisekarte";
            tabSpeisekarte.Padding = new Padding(3, 50, 3, 3);
            tabSpeisekarte.Size = new Size(1435, 738);
            tabSpeisekarte.TabIndex = 0;
            tabSpeisekarte.Text = "Speisekarte";
            // 
            // cmdDrucken
            // 
            cmdDrucken.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdDrucken.BackColor = SystemColors.ControlLightLight;
            cmdDrucken.Enabled = false;
            cmdDrucken.Location = new Point(1141, 6);
            cmdDrucken.Name = "cmdDrucken";
            cmdDrucken.Size = new Size(140, 38);
            cmdDrucken.TabIndex = 5;
            cmdDrucken.Text = "Drucken";
            cmdDrucken.UseVisualStyleBackColor = false;
            cmdDrucken.Click += cmdDrucken_Click;
            // 
            // cmdNeuePositionSpeiseplan
            // 
            cmdNeuePositionSpeiseplan.Location = new Point(6, 6);
            cmdNeuePositionSpeiseplan.Name = "cmdNeuePositionSpeiseplan";
            cmdNeuePositionSpeiseplan.Size = new Size(502, 38);
            cmdNeuePositionSpeiseplan.TabIndex = 4;
            cmdNeuePositionSpeiseplan.Text = "Neues Produkt zu globalem Speiseplan hinzufügen";
            cmdNeuePositionSpeiseplan.UseVisualStyleBackColor = true;
            cmdNeuePositionSpeiseplan.Click += cmdNeuePositionSpeiseplan_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = SystemColors.ControlLightLight;
            button1.Location = new Point(1287, 6);
            button1.Name = "button1";
            button1.Size = new Size(140, 38);
            button1.TabIndex = 3;
            button1.Text = "Speichern";
            button1.UseVisualStyleBackColor = false;
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
            split.Size = new Size(1427, 683);
            split.SplitterDistance = 762;
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
            gridGlobal.Size = new Size(762, 683);
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
            gridAuswahl.Size = new Size(659, 683);
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
            tabTische.Size = new Size(1435, 738);
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
            dataGridViewTische.Size = new Size(1421, 682);
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
            tabHelfer.Size = new Size(1435, 738);
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
            dataGridViewHelfer.Size = new Size(1415, 682);
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
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(btnNeuerVerein);
            groupBox1.Controls.Add(cmbVereine);
            groupBox1.Controls.Add(btnSpeichern);
            groupBox1.FlatStyle = FlatStyle.Flat;
            groupBox1.Location = new Point(259, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(895, 69);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            // 
            // TermineVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1701, 865);
            Controls.Add(tabControl1);
            Controls.Add(monthCalendar1);
            Controls.Add(groupBox1);
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
            groupBox1.ResumeLayout(false);
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
        private Button cmdNeuePositionSpeiseplan;
        private Button cmdDrucken;
        private GroupBox groupBox1;
    }
}

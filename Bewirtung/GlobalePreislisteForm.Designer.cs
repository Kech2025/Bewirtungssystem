namespace Bewirtung
{
    partial class GlobalePreislisteForm
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
            dataGridViewMain = new DataGridView();
            btnLoeschen = new Button();
            btnSpeichern = new Button();
            btnNeu = new Button();
            btnZusatzLoeschen = new Button();
            btnZusatzSpeichern = new Button();
            btnZusatzNeu = new Button();
            groupBoxZusatz = new GroupBox();
            pictureBoxZusatz = new PictureBox();
            dataGridViewZusatz = new DataGridView();
            cboDatumAb = new ComboBox();
            btnNeuerTermin = new Button();
            groupBox1 = new GroupBox();
            lblStichtag = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMain).BeginInit();
            groupBoxZusatz.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxZusatz).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewZusatz).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridViewMain
            // 
            dataGridViewMain.AllowUserToAddRows = false;
            dataGridViewMain.AllowUserToDeleteRows = false;
            dataGridViewMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMain.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewMain.Location = new Point(12, 120);
            dataGridViewMain.MultiSelect = false;
            dataGridViewMain.Name = "dataGridViewMain";
            dataGridViewMain.ReadOnly = true;
            dataGridViewMain.RowHeadersVisible = false;
            dataGridViewMain.RowHeadersWidth = 62;
            dataGridViewMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMain.Size = new Size(869, 942);
            dataGridViewMain.TabIndex = 0;
            dataGridViewMain.CellDoubleClick += dataGridViewMain_CellDoubleClick;
            dataGridViewMain.SelectionChanged += dataGridViewMain_SelectionChanged;
            // 
            // btnLoeschen
            // 
            btnLoeschen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoeschen.Location = new Point(761, 80);
            btnLoeschen.Name = "btnLoeschen";
            btnLoeschen.Size = new Size(120, 40);
            btnLoeschen.TabIndex = 2;
            btnLoeschen.Text = "Löschen";
            btnLoeschen.UseVisualStyleBackColor = true;
            btnLoeschen.Click += btnLoeschen_Click;
            // 
            // btnSpeichern
            // 
            btnSpeichern.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSpeichern.Location = new Point(649, 80);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 1;
            btnSpeichern.Text = "Ändern";
            btnSpeichern.UseVisualStyleBackColor = true;
            btnSpeichern.Click += btnSpeichern_Click;
            // 
            // btnNeu
            // 
            btnNeu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNeu.Location = new Point(533, 80);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(120, 40);
            btnNeu.TabIndex = 0;
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            btnNeu.Click += btnNeu_Click;
            // 
            // btnZusatzLoeschen
            // 
            btnZusatzLoeschen.Location = new Point(440, 31);
            btnZusatzLoeschen.Name = "btnZusatzLoeschen";
            btnZusatzLoeschen.Size = new Size(120, 40);
            btnZusatzLoeschen.TabIndex = 2;
            btnZusatzLoeschen.Text = "Löschen";
            btnZusatzLoeschen.UseVisualStyleBackColor = true;
            btnZusatzLoeschen.Click += btnZusatzLoeschen_Click;
            // 
            // btnZusatzSpeichern
            // 
            btnZusatzSpeichern.Location = new Point(323, 31);
            btnZusatzSpeichern.Name = "btnZusatzSpeichern";
            btnZusatzSpeichern.Size = new Size(120, 40);
            btnZusatzSpeichern.TabIndex = 1;
            btnZusatzSpeichern.Text = "Ändern";
            btnZusatzSpeichern.UseVisualStyleBackColor = true;
            btnZusatzSpeichern.Click += btnZusatzSpeichern_Click;
            // 
            // btnZusatzNeu
            // 
            btnZusatzNeu.Location = new Point(206, 31);
            btnZusatzNeu.Name = "btnZusatzNeu";
            btnZusatzNeu.Size = new Size(120, 40);
            btnZusatzNeu.TabIndex = 0;
            btnZusatzNeu.Text = "Neu";
            btnZusatzNeu.UseVisualStyleBackColor = true;
            btnZusatzNeu.Click += btnZusatzNeu_Click;
            // 
            // groupBoxZusatz
            // 
            groupBoxZusatz.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupBoxZusatz.Controls.Add(btnZusatzNeu);
            groupBoxZusatz.Controls.Add(btnZusatzSpeichern);
            groupBoxZusatz.Controls.Add(btnZusatzLoeschen);
            groupBoxZusatz.Controls.Add(pictureBoxZusatz);
            groupBoxZusatz.Controls.Add(dataGridViewZusatz);
            groupBoxZusatz.Location = new Point(897, 74);
            groupBoxZusatz.Name = "groupBoxZusatz";
            groupBoxZusatz.Size = new Size(582, 988);
            groupBoxZusatz.TabIndex = 1;
            groupBoxZusatz.TabStop = false;
            groupBoxZusatz.Text = "Zusatzpositionen";
            // 
            // pictureBoxZusatz
            // 
            pictureBoxZusatz.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            pictureBoxZusatz.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxZusatz.Location = new Point(164, 726);
            pictureBoxZusatz.Name = "pictureBoxZusatz";
            pictureBoxZusatz.Size = new Size(256, 256);
            pictureBoxZusatz.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxZusatz.TabIndex = 5;
            pictureBoxZusatz.TabStop = false;
            // 
            // dataGridViewZusatz
            // 
            dataGridViewZusatz.AllowUserToAddRows = false;
            dataGridViewZusatz.AllowUserToDeleteRows = false;
            dataGridViewZusatz.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewZusatz.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewZusatz.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewZusatz.Location = new Point(20, 71);
            dataGridViewZusatz.MultiSelect = false;
            dataGridViewZusatz.Name = "dataGridViewZusatz";
            dataGridViewZusatz.ReadOnly = true;
            dataGridViewZusatz.RowHeadersVisible = false;
            dataGridViewZusatz.RowHeadersWidth = 62;
            dataGridViewZusatz.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewZusatz.Size = new Size(540, 636);
            dataGridViewZusatz.TabIndex = 0;
            dataGridViewZusatz.CellDoubleClick += dataGridViewZusatz_CellDoubleClick;
            // 
            // cboDatumAb
            // 
            cboDatumAb.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDatumAb.FormattingEnabled = true;
            cboDatumAb.Location = new Point(111, 22);
            cboDatumAb.Name = "cboDatumAb";
            cboDatumAb.Size = new Size(200, 33);
            cboDatumAb.TabIndex = 3;
            cboDatumAb.SelectedIndexChanged += cboDatumAb_SelectedIndexChanged;
            // 
            // btnNeuerTermin
            // 
            btnNeuerTermin.Location = new Point(329, 19);
            btnNeuerTermin.Name = "btnNeuerTermin";
            btnNeuerTermin.Size = new Size(140, 36);
            btnNeuerTermin.TabIndex = 4;
            btnNeuerTermin.Text = "Neuer Termin";
            btnNeuerTermin.Click += btnNeuerTermin_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(cboDatumAb);
            groupBox1.Controls.Add(lblStichtag);
            groupBox1.Controls.Add(btnNeuerTermin);
            groupBox1.Location = new Point(12, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1467, 63);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            // 
            // lblStichtag
            // 
            lblStichtag.AutoSize = true;
            lblStichtag.Location = new Point(12, 25);
            lblStichtag.Name = "lblStichtag";
            lblStichtag.Size = new Size(93, 25);
            lblStichtag.TabIndex = 5;
            lblStichtag.Text = "Termin ab:";
            // 
            // GlobalePreislisteForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1504, 1074);
            Controls.Add(groupBox1);
            Controls.Add(btnNeu);
            Controls.Add(btnSpeichern);
            Controls.Add(btnLoeschen);
            Controls.Add(groupBoxZusatz);
            Controls.Add(dataGridViewMain);
            Name = "GlobalePreislisteForm";
            Text = "Globale Preisliste";
            ((System.ComponentModel.ISupportInitialize)dataGridViewMain).EndInit();
            groupBoxZusatz.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxZusatz).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewZusatz).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewMain;
        private Button btnNeu;
        private Button btnSpeichern;
        private Button btnLoeschen;
        private GroupBox groupBoxZusatz;
        private DataGridView dataGridViewZusatz;
        private Button btnZusatzNeu;
        private Button btnZusatzSpeichern;
        private Button btnZusatzLoeschen;
        private PictureBox pictureBoxZusatz;
        private ComboBox cboDatumAb;
        private Button btnNeuerTermin;
        private GroupBox groupBox1;
        private Label lblStichtag;
    }
}

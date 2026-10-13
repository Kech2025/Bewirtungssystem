namespace Bewirtung
{
    partial class GlobalePreislisteBearbeitenDialog
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

        #region Vom Windows Form-Designer generierter Code

        private void InitializeComponent()
        {
            lblName = new Label();
            txtName = new TextBox();
            lblPreiskategorie = new Label();
            cmbPreiskategorie = new ComboBox();
            lblBildPfad = new Label();
            txtBildPfad = new TextBox();
            btnBildDurchsuchen = new Button();
            chkDruck = new CheckBox();
            lblPreis = new Label();
            numPreis = new NumericUpDown();
            lblGueltigAb = new Label();
            dtpGueltigAb = new DateTimePicker();
            lblGueltigBis = new Label();
            dtpGueltigBis = new DateTimePicker();
            lblZusatztext = new Label();
            txtZusatztext = new TextBox();
            pictureBoxHaupt = new PictureBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            ((System.ComponentModel.ISupportInitialize)numPreis).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxHaupt).BeginInit();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(20, 25);
            lblName.Name = "lblName";
            lblName.Size = new Size(59, 25);
            lblName.TabIndex = 10;
            lblName.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(180, 19);
            txtName.Name = "txtName";
            txtName.Size = new Size(556, 31);
            txtName.TabIndex = 0;
            // 
            // lblPreiskategorie
            // 
            lblPreiskategorie.AutoSize = true;
            lblPreiskategorie.Location = new Point(20, 75);
            lblPreiskategorie.Name = "lblPreiskategorie";
            lblPreiskategorie.Size = new Size(123, 25);
            lblPreiskategorie.TabIndex = 9;
            lblPreiskategorie.Text = "Preiskategorie";
            // 
            // cmbPreiskategorie
            // 
            cmbPreiskategorie.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPreiskategorie.Location = new Point(180, 72);
            cmbPreiskategorie.Name = "cmbPreiskategorie";
            cmbPreiskategorie.Size = new Size(320, 33);
            cmbPreiskategorie.TabIndex = 1;
            // 
            // lblBildPfad
            // 
            lblBildPfad.AutoSize = true;
            lblBildPfad.Location = new Point(20, 315);
            lblBildPfad.Name = "lblBildPfad";
            lblBildPfad.Size = new Size(78, 25);
            lblBildPfad.TabIndex = 8;
            lblBildPfad.Text = "Bildpfad";
            // 
            // txtBildPfad
            // 
            txtBildPfad.Location = new Point(180, 312);
            txtBildPfad.Name = "txtBildPfad";
            txtBildPfad.Size = new Size(556, 31);
            txtBildPfad.TabIndex = 3;
            // 
            // btnBildDurchsuchen
            // 
            btnBildDurchsuchen.Location = new Point(751, 315);
            btnBildDurchsuchen.Name = "btnBildDurchsuchen";
            btnBildDurchsuchen.Size = new Size(130, 33);
            btnBildDurchsuchen.TabIndex = 4;
            btnBildDurchsuchen.Text = "Durchsuchen…";
            btnBildDurchsuchen.UseVisualStyleBackColor = true;
            // 
            // chkDruck
            // 
            chkDruck.AutoSize = true;
            chkDruck.Location = new Point(549, 74);
            chkDruck.Name = "chkDruck";
            chkDruck.Size = new Size(84, 29);
            chkDruck.TabIndex = 4;
            chkDruck.Text = "Druck";
            chkDruck.UseVisualStyleBackColor = true;
            // 
            // lblPreis
            // 
            lblPreis.AutoSize = true;
            lblPreis.Location = new Point(20, 129);
            lblPreis.Name = "lblPreis";
            lblPreis.Size = new Size(49, 25);
            lblPreis.TabIndex = 11;
            lblPreis.Text = "Preis";
            // 
            // numPreis
            // 
            numPreis.DecimalPlaces = 2;
            numPreis.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numPreis.Location = new Point(180, 127);
            numPreis.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numPreis.Name = "numPreis";
            numPreis.Size = new Size(200, 31);
            numPreis.TabIndex = 5;
            numPreis.TextAlign = HorizontalAlignment.Right;
            // 
            // lblGueltigAb
            // 
            lblGueltigAb.AutoSize = true;
            lblGueltigAb.Location = new Point(420, 131);
            lblGueltigAb.Name = "lblGueltigAb";
            lblGueltigAb.Size = new Size(90, 25);
            lblGueltigAb.TabIndex = 13;
            lblGueltigAb.Text = "Gültig-Ab";
            // 
            // dtpGueltigAb
            // 
            dtpGueltigAb.Format = DateTimePickerFormat.Short;
            dtpGueltigAb.Location = new Point(540, 127);
            dtpGueltigAb.Name = "dtpGueltigAb";
            dtpGueltigAb.Size = new Size(160, 31);
            dtpGueltigAb.TabIndex = 8;
            // 
            // lblGueltigBis
            // 
            lblGueltigBis.AutoSize = true;
            lblGueltigBis.Location = new Point(420, 163);
            lblGueltigBis.Name = "lblGueltigBis";
            lblGueltigBis.Size = new Size(90, 25);
            lblGueltigBis.TabIndex = 14;
            lblGueltigBis.Text = "Gültig-Bis";
            // 
            // dtpGueltigBis
            // 
            dtpGueltigBis.Format = DateTimePickerFormat.Short;
            dtpGueltigBis.ShowCheckBox = true;
            dtpGueltigBis.Location = new Point(540, 159);
            dtpGueltigBis.Name = "dtpGueltigBis";
            dtpGueltigBis.Size = new Size(160, 31);
            dtpGueltigBis.TabIndex = 9;
            // 
            // lblZusatztext
            // 
            lblZusatztext.AutoSize = true;
            lblZusatztext.Location = new Point(20, 196);
            lblZusatztext.Name = "lblZusatztext";
            lblZusatztext.Size = new Size(92, 25);
            lblZusatztext.TabIndex = 12;
            lblZusatztext.Text = "Zusatztext";
            // 
            // txtZusatztext
            // 
            txtZusatztext.Location = new Point(180, 193);
            txtZusatztext.Multiline = true;
            txtZusatztext.Name = "txtZusatztext";
            txtZusatztext.Size = new Size(556, 102);
            txtZusatztext.TabIndex = 2;
            // 
            // pictureBoxHaupt
            // 
            pictureBoxHaupt.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxHaupt.Location = new Point(180, 367);
            pictureBoxHaupt.Name = "pictureBoxHaupt";
            pictureBoxHaupt.Size = new Size(256, 256);
            pictureBoxHaupt.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxHaupt.TabIndex = 5;
            pictureBoxHaupt.TabStop = false;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(616, 654);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 6;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(751, 654);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 7;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // GlobalePreislisteBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(893, 716);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(pictureBoxHaupt);
            Controls.Add(chkDruck);
            Controls.Add(numPreis);
            Controls.Add(lblPreis);
            Controls.Add(lblGueltigAb);
            Controls.Add(dtpGueltigAb);
            Controls.Add(lblGueltigBis);
            Controls.Add(dtpGueltigBis);
            Controls.Add(lblZusatztext);
            Controls.Add(txtZusatztext);
            Controls.Add(btnBildDurchsuchen);
            Controls.Add(txtBildPfad);
            Controls.Add(lblBildPfad);
            Controls.Add(cmbPreiskategorie);
            Controls.Add(lblPreiskategorie);
            Controls.Add(txtName);
            Controls.Add(lblName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GlobalePreislisteBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Position";
            ((System.ComponentModel.ISupportInitialize)numPreis).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxHaupt).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private Label lblPreiskategorie;
        private ComboBox cmbPreiskategorie;
        private Label lblBildPfad;
        private TextBox txtBildPfad;
        private Button btnBildDurchsuchen;
        private CheckBox chkDruck;
        private Label lblPreis;
        private NumericUpDown numPreis;
        private Label lblGueltigAb;
        private DateTimePicker dtpGueltigAb;
        private Label lblGueltigBis;
        private DateTimePicker dtpGueltigBis;
        private Label lblZusatztext;
        private TextBox txtZusatztext;
        private PictureBox pictureBoxHaupt;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

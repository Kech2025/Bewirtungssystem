namespace Bewirtung
{
    partial class ZusatzBearbeitenDialog
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
            lblBezeichnung = new Label();
            txtBezeichnung = new TextBox();
            lblBildPfad = new Label();
            txtBildPfad = new TextBox();
            btnBildDurchsuchen = new Button();
            chkDruck = new CheckBox();
            lblPreis = new Label();
            numPreis = new NumericUpDown();
            pictureBoxZusatz = new PictureBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            ((System.ComponentModel.ISupportInitialize)numPreis).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxZusatz).BeginInit();
            SuspendLayout();
            // 
            // lblBezeichnung
            // 
            lblBezeichnung.AutoSize = true;
            lblBezeichnung.Location = new Point(20, 25);
            lblBezeichnung.Name = "lblBezeichnung";
            lblBezeichnung.Size = new Size(111, 25);
            lblBezeichnung.TabIndex = 8;
            lblBezeichnung.Text = "Bezeichnung";
            // 
            // txtBezeichnung
            // 
            txtBezeichnung.Location = new Point(180, 22);
            txtBezeichnung.Name = "txtBezeichnung";
            txtBezeichnung.Size = new Size(511, 31);
            txtBezeichnung.TabIndex = 0;
            // 
            // lblBildPfad
            // 
            lblBildPfad.AutoSize = true;
            lblBildPfad.Location = new Point(20, 135);
            lblBildPfad.Name = "lblBildPfad";
            lblBildPfad.Size = new Size(78, 25);
            lblBildPfad.TabIndex = 7;
            lblBildPfad.Text = "Bildpfad";
            // 
            // txtBildPfad
            // 
            txtBildPfad.Location = new Point(180, 135);
            txtBildPfad.Name = "txtBildPfad";
            txtBildPfad.Size = new Size(511, 31);
            txtBildPfad.TabIndex = 1;
            // 
            // btnBildDurchsuchen
            // 
            btnBildDurchsuchen.Location = new Point(720, 131);
            btnBildDurchsuchen.Name = "btnBildDurchsuchen";
            btnBildDurchsuchen.Size = new Size(130, 33);
            btnBildDurchsuchen.TabIndex = 2;
            btnBildDurchsuchen.Text = "Durchsuchen…";
            btnBildDurchsuchen.UseVisualStyleBackColor = true;
            // 
            // chkDruck
            // 
            chkDruck.AutoSize = true;
            chkDruck.Location = new Point(445, 77);
            chkDruck.Name = "chkDruck";
            chkDruck.Size = new Size(84, 29);
            chkDruck.TabIndex = 3;
            chkDruck.Text = "Druck";
            chkDruck.UseVisualStyleBackColor = true;
            // 
            // lblPreis
            // 
            lblPreis.AutoSize = true;
            lblPreis.Location = new Point(20, 77);
            lblPreis.Name = "lblPreis";
            lblPreis.Size = new Size(49, 25);
            lblPreis.TabIndex = 9;
            lblPreis.Text = "Preis";
            // 
            // numPreis
            // 
            numPreis.DecimalPlaces = 2;
            numPreis.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numPreis.Location = new Point(180, 75);
            numPreis.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numPreis.Name = "numPreis";
            numPreis.Size = new Size(200, 31);
            numPreis.TabIndex = 4;
            numPreis.TextAlign = HorizontalAlignment.Right;
            // 
            // pictureBoxZusatz
            // 
            pictureBoxZusatz.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxZusatz.Location = new Point(180, 191);
            pictureBoxZusatz.Name = "pictureBoxZusatz";
            pictureBoxZusatz.Size = new Size(256, 256);
            pictureBoxZusatz.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxZusatz.TabIndex = 4;
            pictureBoxZusatz.TabStop = false;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(571, 407);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 5;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(730, 407);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 6;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // ZusatzBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(885, 477);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(pictureBoxZusatz);
            Controls.Add(chkDruck);
            Controls.Add(numPreis);
            Controls.Add(lblPreis);
            Controls.Add(btnBildDurchsuchen);
            Controls.Add(txtBildPfad);
            Controls.Add(lblBildPfad);
            Controls.Add(txtBezeichnung);
            Controls.Add(lblBezeichnung);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ZusatzBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Zusatzposition";
            ((System.ComponentModel.ISupportInitialize)numPreis).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxZusatz).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBezeichnung;
        private TextBox txtBezeichnung;
        private Label lblBildPfad;
        private TextBox txtBildPfad;
        private Button btnBildDurchsuchen;
        private CheckBox chkDruck;
        private Label lblPreis;
        private NumericUpDown numPreis;
        private PictureBox pictureBoxZusatz;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

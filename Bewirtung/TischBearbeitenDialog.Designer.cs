namespace Bewirtung
{
    partial class TischBearbeitenDialog
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
            lblTischnummer = new Label();
            numTischnummer = new NumericUpDown();
            lblBereich = new Label();
            cmbBereich = new ComboBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            ((System.ComponentModel.ISupportInitialize)numTischnummer).BeginInit();
            SuspendLayout();
            // 
            // lblTischnummer
            // 
            lblTischnummer.AutoSize = true;
            lblTischnummer.Location = new Point(20, 25);
            lblTischnummer.Name = "lblTischnummer";
            lblTischnummer.Size = new Size(110, 25);
            lblTischnummer.Text = "Tischnummer";
            // 
            // numTischnummer
            // 
            numTischnummer.Location = new Point(200, 22);
            numTischnummer.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numTischnummer.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTischnummer.Name = "numTischnummer";
            numTischnummer.Size = new Size(150, 31);
            numTischnummer.TabIndex = 0;
            numTischnummer.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblBereich
            // 
            lblBereich.AutoSize = true;
            lblBereich.Location = new Point(20, 75);
            lblBereich.Name = "lblBereich";
            lblBereich.Size = new Size(70, 25);
            lblBereich.Text = "Bereich";
            // 
            // cmbBereich
            // 
            cmbBereich.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBereich.Location = new Point(200, 72);
            cmbBereich.Name = "cmbBereich";
            cmbBereich.Size = new Size(350, 33);
            cmbBereich.TabIndex = 1;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(310, 135);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 2;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(440, 135);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 3;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // TischBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(580, 200);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(cmbBereich);
            Controls.Add(lblBereich);
            Controls.Add(numTischnummer);
            Controls.Add(lblTischnummer);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TischBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tisch";
            ((System.ComponentModel.ISupportInitialize)numTischnummer).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTischnummer;
        private NumericUpDown numTischnummer;
        private Label lblBereich;
        private ComboBox cmbBereich;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

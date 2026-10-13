namespace Bewirtung
{
    partial class BereichTischeBearbeitenDialog
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
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            SuspendLayout();
            // 
            // lblBezeichnung
            // 
            lblBezeichnung.AutoSize = true;
            lblBezeichnung.Location = new Point(20, 25);
            lblBezeichnung.Name = "lblBezeichnung";
            lblBezeichnung.Size = new Size(110, 25);
            lblBezeichnung.Text = "Bezeichnung";
            // 
            // txtBezeichnung
            // 
            txtBezeichnung.Location = new Point(200, 22);
            txtBezeichnung.MaxLength = 100;
            txtBezeichnung.Name = "txtBezeichnung";
            txtBezeichnung.Size = new Size(350, 31);
            txtBezeichnung.TabIndex = 0;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(310, 85);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 1;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(440, 85);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 2;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // BereichTischeBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(580, 150);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(txtBezeichnung);
            Controls.Add(lblBezeichnung);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BereichTischeBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Bereich Tische";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBezeichnung;
        private TextBox txtBezeichnung;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

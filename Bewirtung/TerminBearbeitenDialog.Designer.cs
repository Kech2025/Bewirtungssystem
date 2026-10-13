namespace Bewirtung
{
    partial class TerminBearbeitenDialog
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
            lblDatum = new Label();
            dtpDatum = new DateTimePicker();
            lblVerein = new Label();
            cmbVerein = new ComboBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            SuspendLayout();
            // 
            // lblDatum
            // 
            lblDatum.AutoSize = true;
            lblDatum.Location = new Point(20, 28);
            lblDatum.Name = "lblDatum";
            lblDatum.Size = new Size(147, 25);
            lblDatum.Text = "Bewirtungsdatum";
            // 
            // dtpDatum
            // 
            dtpDatum.Format = DateTimePickerFormat.Short;
            dtpDatum.Location = new Point(210, 25);
            dtpDatum.Name = "dtpDatum";
            dtpDatum.Size = new Size(250, 31);
            dtpDatum.TabIndex = 0;
            // 
            // lblVerein
            // 
            lblVerein.AutoSize = true;
            lblVerein.Location = new Point(20, 78);
            lblVerein.Name = "lblVerein";
            lblVerein.Size = new Size(63, 25);
            lblVerein.Text = "Verein";
            // 
            // cmbVerein
            // 
            cmbVerein.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVerein.Location = new Point(210, 75);
            cmbVerein.Name = "cmbVerein";
            cmbVerein.Size = new Size(320, 33);
            cmbVerein.TabIndex = 1;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(280, 140);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 2;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(410, 140);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 3;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // TerminBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(560, 210);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(cmbVerein);
            Controls.Add(lblVerein);
            Controls.Add(dtpDatum);
            Controls.Add(lblDatum);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TerminBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Termin";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDatum;
        private DateTimePicker dtpDatum;
        private Label lblVerein;
        private ComboBox cmbVerein;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

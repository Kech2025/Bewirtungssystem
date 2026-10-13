namespace Bewirtung
{
    partial class NeuerTerminDialog
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
            monthCalendar = new MonthCalendar();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            SuspendLayout();
            // 
            // monthCalendar
            // 
            monthCalendar.Location = new Point(20, 18);
            monthCalendar.MaxSelectionCount = 1;
            monthCalendar.Name = "monthCalendar";
            monthCalendar.TabIndex = 0;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(12, 290);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 34);
            btnSpeichern.TabIndex = 1;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(156, 290);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 34);
            btnAbbrechen.TabIndex = 2;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // NeuerTerminDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(288, 336);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(monthCalendar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NeuerTerminDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Neuer Termin";
            ResumeLayout(false);
        }

        #endregion

        private MonthCalendar monthCalendar;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

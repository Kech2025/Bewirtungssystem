namespace Bewirtung
{
    partial class HelferAuswahlDialog
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
            gridBenutzer = new DataGridView();
            colAuswahl = new DataGridViewCheckBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colEmail = new DataGridViewTextBoxColumn();
            colTelefon = new DataGridViewTextBoxColumn();
            colVerein = new DataGridViewTextBoxColumn();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            btnNeuenBenutzerHinzufügen = new Button();
            ((System.ComponentModel.ISupportInitialize)gridBenutzer).BeginInit();
            SuspendLayout();
            // 
            // gridBenutzer
            // 
            gridBenutzer.AllowUserToAddRows = false;
            gridBenutzer.AllowUserToDeleteRows = false;
            gridBenutzer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridBenutzer.Columns.AddRange(new DataGridViewColumn[] { colAuswahl, colName, colEmail, colTelefon, colVerein });
            gridBenutzer.Location = new Point(20, 50);
            gridBenutzer.Name = "gridBenutzer";
            gridBenutzer.RowHeadersVisible = false;
            gridBenutzer.RowHeadersWidth = 62;
            gridBenutzer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridBenutzer.Size = new Size(1193, 717);
            gridBenutzer.TabIndex = 0;
            // 
            // colAuswahl
            // 
            colAuswahl.HeaderText = "Auswahl";
            colAuswahl.MinimumWidth = 8;
            colAuswahl.Name = "colAuswahl";
            colAuswahl.SortMode = DataGridViewColumnSortMode.Automatic;
            colAuswahl.Width = 90;
            // 
            // colName
            // 
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colName.HeaderText = "Name";
            colName.MinimumWidth = 8;
            colName.Name = "colName";
            colName.ReadOnly = true;
            // 
            // colEmail
            // 
            colEmail.HeaderText = "E-Mail";
            colEmail.MinimumWidth = 8;
            colEmail.Name = "colEmail";
            colEmail.ReadOnly = true;
            colEmail.Width = 200;
            // 
            // colTelefon
            // 
            colTelefon.HeaderText = "Telefon";
            colTelefon.MinimumWidth = 8;
            colTelefon.Name = "colTelefon";
            colTelefon.ReadOnly = true;
            colTelefon.Width = 150;
            // 
            // colVerein
            // 
            colVerein.HeaderText = "Verein";
            colVerein.MinimumWidth = 8;
            colVerein.Name = "colVerein";
            colVerein.ReadOnly = true;
            colVerein.Width = 150;
            // 
            // btnSpeichern
            // 
            btnSpeichern.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(969, 776);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 1;
            btnSpeichern.Text = "Ok";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(1095, 776);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 2;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // btnNeuenBenutzerHinzufügen
            // 
            btnNeuenBenutzerHinzufügen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNeuenBenutzerHinzufügen.Location = new Point(814, 10);
            btnNeuenBenutzerHinzufügen.Name = "btnNeuenBenutzerHinzufügen";
            btnNeuenBenutzerHinzufügen.Size = new Size(399, 40);
            btnNeuenBenutzerHinzufügen.TabIndex = 3;
            btnNeuenBenutzerHinzufügen.Text = "Neuen Benutzer hinzufügen";
            btnNeuenBenutzerHinzufügen.UseVisualStyleBackColor = true;
            btnNeuenBenutzerHinzufügen.Click += btnNeuenBenutzerHinzufügen_Click;
            // 
            // HelferAuswahlDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(1225, 828);
            Controls.Add(btnNeuenBenutzerHinzufügen);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(gridBenutzer);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HelferAuswahlDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Helfer hinzufügen";
            ((System.ComponentModel.ISupportInitialize)gridBenutzer).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView gridBenutzer;
        private DataGridViewCheckBoxColumn colAuswahl;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colEmail;
        private DataGridViewTextBoxColumn colTelefon;
        private DataGridViewTextBoxColumn colVerein;
        private Button btnSpeichern;
        private Button btnAbbrechen;
        private Button btnNeuenBenutzerHinzufügen;
    }
}

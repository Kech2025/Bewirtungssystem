namespace Bewirtung
{
    partial class TischTerminBearbeitenDialog
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
            lblTermin = new Label();
            cmbTermin = new ComboBox();
            lblTisch = new Label();
            dataGridViewTische = new DataGridView();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).BeginInit();
            SuspendLayout();
            // 
            // lblTermin
            // 
            lblTermin.AutoSize = true;
            lblTermin.Location = new Point(20, 25);
            lblTermin.Name = "lblTermin";
            lblTermin.Size = new Size(64, 25);
            lblTermin.TabIndex = 5;
            lblTermin.Text = "Termin";
            // 
            // cmbTermin
            // 
            cmbTermin.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTermin.Location = new Point(200, 22);
            cmbTermin.Name = "cmbTermin";
            cmbTermin.Size = new Size(473, 33);
            cmbTermin.TabIndex = 0;
            // 
            // lblTisch
            // 
            lblTisch.AutoSize = true;
            lblTisch.Location = new Point(20, 75);
            lblTisch.Name = "lblTisch";
            lblTisch.Size = new Size(51, 25);
            lblTisch.TabIndex = 4;
            lblTisch.Text = "Tisch";
            // 
            // dataGridViewTische
            // 
            dataGridViewTische.AllowUserToAddRows = false;
            dataGridViewTische.AllowUserToDeleteRows = false;
            dataGridViewTische.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTische.Location = new Point(200, 72);
            dataGridViewTische.MultiSelect = false;
            dataGridViewTische.Name = "dataGridViewTische";
            dataGridViewTische.RowHeadersVisible = false;
            dataGridViewTische.RowHeadersWidth = 62;
            dataGridViewTische.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridViewTische.Size = new Size(473, 300);
            dataGridViewTische.TabIndex = 1;
            dataGridViewTische.CellDoubleClick += dataGridViewTische_CellDoubleClick;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(402, 390);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 2;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(553, 390);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 3;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // TischTerminBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(685, 457);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(dataGridViewTische);
            Controls.Add(lblTisch);
            Controls.Add(cmbTermin);
            Controls.Add(lblTermin);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TischTerminBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tische pro Termin";
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTermin;
        private ComboBox cmbTermin;
        private Label lblTisch;
        private DataGridView dataGridViewTische;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

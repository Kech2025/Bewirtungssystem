namespace Bewirtung
{
    partial class TischeProTerminVerwaltenForm
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
            dataGridViewTischTermin = new DataGridView();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            lblFilter = new Label();
            cmbFilterTermin = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTischTermin).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewTischTermin
            // 
            dataGridViewTischTermin.AllowUserToAddRows = false;
            dataGridViewTischTermin.AllowUserToDeleteRows = false;
            dataGridViewTischTermin.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTischTermin.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTischTermin.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewTischTermin.Location = new Point(12, 92);
            dataGridViewTischTermin.MultiSelect = false;
            dataGridViewTischTermin.Name = "dataGridViewTischTermin";
            dataGridViewTischTermin.ReadOnly = true;
            dataGridViewTischTermin.RowHeadersVisible = false;
            dataGridViewTischTermin.RowHeadersWidth = 62;
            dataGridViewTischTermin.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTischTermin.Size = new Size(1076, 596);
            dataGridViewTischTermin.TabIndex = 0;
            dataGridViewTischTermin.CellDoubleClick += dataGridViewTischTermin_CellDoubleClick;
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Location = new Point(12, 56);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(100, 25);
            lblFilter.TabIndex = 3;
            lblFilter.Text = "Termin-Filter:";
            // 
            // cmbFilterTermin
            // 
            cmbFilterTermin.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterTermin.Location = new Point(140, 53);
            cmbFilterTermin.Name = "cmbFilterTermin";
            cmbFilterTermin.Size = new Size(400, 33);
            cmbFilterTermin.TabIndex = 4;
            cmbFilterTermin.SelectedIndexChanged += cmbFilterTermin_SelectedIndexChanged;
            // 
            // btnLoeschen
            // 
            btnLoeschen.Location = new Point(968, 6);
            btnLoeschen.Name = "btnLoeschen";
            btnLoeschen.Size = new Size(120, 40);
            btnLoeschen.TabIndex = 2;
            btnLoeschen.Text = "Löschen";
            btnLoeschen.UseVisualStyleBackColor = true;
            btnLoeschen.Click += btnLoeschen_Click;
            // 
            // btnAendern
            // 
            btnAendern.Location = new Point(852, 6);
            btnAendern.Name = "btnAendern";
            btnAendern.Size = new Size(120, 40);
            btnAendern.TabIndex = 1;
            btnAendern.Text = "Ändern";
            btnAendern.UseVisualStyleBackColor = true;
            btnAendern.Click += btnAendern_Click;
            // 
            // btnNeu
            // 
            btnNeu.Location = new Point(736, 6);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(120, 40);
            btnNeu.TabIndex = 0;
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            btnNeu.Click += btnNeu_Click;
            // 
            // TischeProTerminVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(lblFilter);
            Controls.Add(cmbFilterTermin);
            Controls.Add(dataGridViewTischTermin);
            Name = "TischeProTerminVerwaltenForm";
            Text = "Tische pro Termin verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewTischTermin).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewTischTermin;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
        private Label lblFilter;
        private ComboBox cmbFilterTermin;
    }
}

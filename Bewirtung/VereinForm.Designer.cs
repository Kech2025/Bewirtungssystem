namespace Bewirtung
{
    partial class VereinForm
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
            dataGridViewVereine = new DataGridView();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewVereine).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewVereine
            // 
            dataGridViewVereine.AllowUserToAddRows = false;
            dataGridViewVereine.AllowUserToDeleteRows = false;
            dataGridViewVereine.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewVereine.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewVereine.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewVereine.Location = new Point(12, 46);
            dataGridViewVereine.MultiSelect = false;
            dataGridViewVereine.Name = "dataGridViewVereine";
            dataGridViewVereine.ReadOnly = true;
            dataGridViewVereine.RowHeadersVisible = false;
            dataGridViewVereine.RowHeadersWidth = 62;
            dataGridViewVereine.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewVereine.Size = new Size(1076, 642);
            dataGridViewVereine.TabIndex = 0;
            dataGridViewVereine.CellDoubleClick += dataGridViewVereine_CellDoubleClick;
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
            // VereinForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(dataGridViewVereine);
            Name = "VereinForm";
            Text = "Vereine verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewVereine).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewVereine;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
    }
}

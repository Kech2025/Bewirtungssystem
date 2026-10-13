namespace Bewirtung
{
    partial class TischeVerwaltenForm
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
            dataGridViewTische = new DataGridView();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewTische
            // 
            dataGridViewTische.AllowUserToAddRows = false;
            dataGridViewTische.AllowUserToDeleteRows = false;
            dataGridViewTische.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTische.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTische.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewTische.Location = new Point(12, 46);
            dataGridViewTische.MultiSelect = false;
            dataGridViewTische.Name = "dataGridViewTische";
            dataGridViewTische.ReadOnly = true;
            dataGridViewTische.RowHeadersVisible = false;
            dataGridViewTische.RowHeadersWidth = 62;
            dataGridViewTische.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTische.Size = new Size(1076, 642);
            dataGridViewTische.TabIndex = 0;
            dataGridViewTische.CellDoubleClick += dataGridViewTische_CellDoubleClick;
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
            // TischeVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(dataGridViewTische);
            Name = "TischeVerwaltenForm";
            Text = "Tische verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewTische).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewTische;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
    }
}

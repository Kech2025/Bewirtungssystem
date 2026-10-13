namespace Bewirtung
{
    partial class BereichTischeVerwaltenForm
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
            dataGridViewBereiche = new DataGridView();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewBereiche).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewBereiche
            // 
            dataGridViewBereiche.AllowUserToAddRows = false;
            dataGridViewBereiche.AllowUserToDeleteRows = false;
            dataGridViewBereiche.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewBereiche.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBereiche.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewBereiche.Location = new Point(12, 46);
            dataGridViewBereiche.MultiSelect = false;
            dataGridViewBereiche.Name = "dataGridViewBereiche";
            dataGridViewBereiche.ReadOnly = true;
            dataGridViewBereiche.RowHeadersVisible = false;
            dataGridViewBereiche.RowHeadersWidth = 62;
            dataGridViewBereiche.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBereiche.Size = new Size(1076, 642);
            dataGridViewBereiche.TabIndex = 0;
            dataGridViewBereiche.CellDoubleClick += dataGridViewBereiche_CellDoubleClick;
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
            // BereichTischeVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(dataGridViewBereiche);
            Name = "BereichTischeVerwaltenForm";
            Text = "Bewirtungsbereiche verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewBereiche).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewBereiche;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
    }
}

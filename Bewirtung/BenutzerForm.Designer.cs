namespace Bewirtung
{
    partial class BenutzerForm
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
            dataGridViewBenutzer = new DataGridView();
            panelButtons = new Panel();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewBenutzer).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewBenutzer
            // 
            dataGridViewBenutzer.AllowUserToAddRows = false;
            dataGridViewBenutzer.AllowUserToDeleteRows = false;
            dataGridViewBenutzer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewBenutzer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewBenutzer.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewBenutzer.Location = new Point(12, 69);
            dataGridViewBenutzer.MultiSelect = false;
            dataGridViewBenutzer.Name = "dataGridViewBenutzer";
            dataGridViewBenutzer.ReadOnly = true;
            dataGridViewBenutzer.RowHeadersVisible = false;
            dataGridViewBenutzer.RowHeadersWidth = 62;
            dataGridViewBenutzer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBenutzer.Size = new Size(1376, 768);
            dataGridViewBenutzer.TabIndex = 0;
            dataGridViewBenutzer.CellDoubleClick += dataGridViewBenutzer_CellDoubleClick;
            // 
            // panelButtons
            // 
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 790);
            panelButtons.Name = "panelButtons";
            panelButtons.Padding = new Padding(20, 10, 20, 10);
            panelButtons.Size = new Size(1400, 60);
            panelButtons.TabIndex = 1;
            // 
            // btnLoeschen
            // 
            btnLoeschen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoeschen.Location = new Point(1268, 28);
            btnLoeschen.Name = "btnLoeschen";
            btnLoeschen.Size = new Size(120, 40);
            btnLoeschen.TabIndex = 2;
            btnLoeschen.Text = "Löschen";
            btnLoeschen.UseVisualStyleBackColor = true;
            btnLoeschen.Click += btnLoeschen_Click;
            // 
            // btnAendern
            // 
            btnAendern.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAendern.Location = new Point(1152, 28);
            btnAendern.Name = "btnAendern";
            btnAendern.Size = new Size(120, 40);
            btnAendern.TabIndex = 1;
            btnAendern.Text = "Ändern";
            btnAendern.UseVisualStyleBackColor = true;
            btnAendern.Click += btnAendern_Click;
            // 
            // btnNeu
            // 
            btnNeu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNeu.Location = new Point(1037, 28);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(120, 40);
            btnNeu.TabIndex = 0;
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            btnNeu.Click += btnNeu_Click;
            // 
            // BenutzerForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1400, 850);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(dataGridViewBenutzer);
            Controls.Add(panelButtons);
            Name = "BenutzerForm";
            Text = "Benutzer verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewBenutzer).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewBenutzer;
        private Panel panelButtons;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
    }
}

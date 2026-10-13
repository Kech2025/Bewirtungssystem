namespace Bewirtung
{
    partial class TerminForm
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
            dataGridViewTermine = new DataGridView();
            btnLoeschen = new Button();
            btnAendern = new Button();
            btnNeu = new Button();
            groupBoxHelfer = new GroupBox();
            dataGridViewHelfer = new DataGridView();
            btnHelferEntfernen = new Button();
            btnHelferHinzufuegen = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTermine).BeginInit();
            groupBoxHelfer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewHelfer).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewTermine
            // 
            dataGridViewTermine.AllowUserToAddRows = false;
            dataGridViewTermine.AllowUserToDeleteRows = false;
            dataGridViewTermine.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTermine.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTermine.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewTermine.Location = new Point(13, 76);
            dataGridViewTermine.MultiSelect = false;
            dataGridViewTermine.Name = "dataGridViewTermine";
            dataGridViewTermine.ReadOnly = true;
            dataGridViewTermine.RowHeadersVisible = false;
            dataGridViewTermine.RowHeadersWidth = 62;
            dataGridViewTermine.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTermine.Size = new Size(699, 838);
            dataGridViewTermine.TabIndex = 0;
            dataGridViewTermine.CellDoubleClick += dataGridViewTermine_CellDoubleClick;
            dataGridViewTermine.SelectionChanged += dataGridViewTermine_SelectionChanged;
            // 
            // btnLoeschen
            // 
            btnLoeschen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoeschen.Location = new Point(592, 38);
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
            btnAendern.Location = new Point(475, 38);
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
            btnNeu.Location = new Point(359, 38);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(120, 40);
            btnNeu.TabIndex = 0;
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            btnNeu.Click += btnNeu_Click;
            // 
            // groupBoxHelfer
            // 
            groupBoxHelfer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupBoxHelfer.Controls.Add(dataGridViewHelfer);
            groupBoxHelfer.Controls.Add(btnHelferEntfernen);
            groupBoxHelfer.Controls.Add(btnHelferHinzufuegen);
            groupBoxHelfer.Location = new Point(718, 12);
            groupBoxHelfer.Name = "groupBoxHelfer";
            groupBoxHelfer.Size = new Size(750, 912);
            groupBoxHelfer.TabIndex = 0;
            groupBoxHelfer.TabStop = false;
            groupBoxHelfer.Text = "Helfer zum Termin";
            // 
            // dataGridViewHelfer
            // 
            dataGridViewHelfer.AllowUserToAddRows = false;
            dataGridViewHelfer.AllowUserToDeleteRows = false;
            dataGridViewHelfer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dataGridViewHelfer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewHelfer.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewHelfer.Location = new Point(6, 64);
            dataGridViewHelfer.MultiSelect = false;
            dataGridViewHelfer.Name = "dataGridViewHelfer";
            dataGridViewHelfer.ReadOnly = true;
            dataGridViewHelfer.RowHeadersVisible = false;
            dataGridViewHelfer.RowHeadersWidth = 62;
            dataGridViewHelfer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewHelfer.Size = new Size(746, 838);
            dataGridViewHelfer.TabIndex = 2;
            // 
            // btnHelferEntfernen
            // 
            btnHelferEntfernen.Location = new Point(630, 29);
            btnHelferEntfernen.Name = "btnHelferEntfernen";
            btnHelferEntfernen.Size = new Size(120, 36);
            btnHelferEntfernen.TabIndex = 1;
            btnHelferEntfernen.Text = "Entfernen";
            btnHelferEntfernen.UseVisualStyleBackColor = true;
            btnHelferEntfernen.Click += btnHelferEntfernen_Click;
            // 
            // btnHelferHinzufuegen
            // 
            btnHelferHinzufuegen.Location = new Point(493, 29);
            btnHelferHinzufuegen.Name = "btnHelferHinzufuegen";
            btnHelferHinzufuegen.Size = new Size(140, 36);
            btnHelferHinzufuegen.TabIndex = 0;
            btnHelferHinzufuegen.Text = "Hinzufügen";
            btnHelferHinzufuegen.UseVisualStyleBackColor = true;
            btnHelferHinzufuegen.Click += btnHelferHinzufuegen_Click;
            // 
            // TerminForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1480, 936);
            Controls.Add(btnNeu);
            Controls.Add(btnAendern);
            Controls.Add(btnLoeschen);
            Controls.Add(groupBoxHelfer);
            Controls.Add(dataGridViewTermine);
            Name = "TerminForm";
            Text = "Termine";
            ((System.ComponentModel.ISupportInitialize)dataGridViewTermine).EndInit();
            groupBoxHelfer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewHelfer).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewTermine;
        private Button btnNeu;
        private Button btnAendern;
        private Button btnLoeschen;
        private GroupBox groupBoxHelfer;
        private Button btnHelferHinzufuegen;
        private Button btnHelferEntfernen;
        private DataGridView dataGridViewHelfer;
    }
}

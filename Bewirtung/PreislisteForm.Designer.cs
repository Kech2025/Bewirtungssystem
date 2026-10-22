namespace Bewirtung
{
    partial class PreislisteForm
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
            components = new System.ComponentModel.Container();
            lblTermin = new Label();
            cmbTermine = new ComboBox();
            btnDrucken = new Button();
            gridPreisliste = new DataGridView();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)gridPreisliste).BeginInit();
            SuspendLayout();
            // 
            // lblTermin
            // 
            lblTermin.AutoSize = true;
            lblTermin.Location = new Point(12, 16);
            lblTermin.Name = "lblTermin";
            lblTermin.Size = new Size(129, 25);
            lblTermin.TabIndex = 0;
            lblTermin.Text = "Termin / Verein";
            // 
            // cmbTermine
            // 
            cmbTermine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTermine.Location = new Point(138, 12);
            cmbTermine.Name = "cmbTermine";
            cmbTermine.Size = new Size(360, 33);
            cmbTermine.TabIndex = 1;
            // 
            // btnDrucken
            // 
            btnDrucken.Location = new Point(520, 10);
            btnDrucken.Name = "btnDrucken";
            btnDrucken.Size = new Size(120, 38);
            btnDrucken.TabIndex = 2;
            btnDrucken.Text = "Drucken";
            btnDrucken.UseVisualStyleBackColor = true;
            btnDrucken.Click += btnDrucken_Click;
            // 
            // gridPreisliste
            // 
            gridPreisliste.AllowUserToAddRows = false;
            gridPreisliste.AllowUserToDeleteRows = false;
            gridPreisliste.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridPreisliste.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridPreisliste.Location = new Point(12, 60);
            gridPreisliste.Name = "gridPreisliste";
            gridPreisliste.ReadOnly = true;
            gridPreisliste.RowHeadersVisible = false;
            gridPreisliste.RowHeadersWidth = 62;
            gridPreisliste.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridPreisliste.Size = new Size(776, 490);
            gridPreisliste.TabIndex = 3;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // PreislisteForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 562);
            Controls.Add(gridPreisliste);
            Controls.Add(btnDrucken);
            Controls.Add(cmbTermine);
            Controls.Add(lblTermin);
            Name = "PreislisteForm";
            Text = "Preisliste";
            ((System.ComponentModel.ISupportInitialize)gridPreisliste).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblTermin;
        private ComboBox cmbTermine;
        private Button btnDrucken;
        private DataGridView gridPreisliste;
        private System.Windows.Forms.Timer timer1;
    }
}

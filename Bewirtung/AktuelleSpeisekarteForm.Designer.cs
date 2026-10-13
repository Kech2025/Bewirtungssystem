namespace Bewirtung
{
    partial class AktuelleSpeisekarteForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// </summary>
        private void InitializeComponent()
        {
            topPanel = new Panel();
            cmbTermine = new ComboBox();
            lblTermin = new Label();
            bottomPanel = new Panel();
            btnSpeichern = new Button();
            split = new SplitContainer();
            gridGlobal = new DataGridView();
            gridAuswahl = new DataGridView();
            topPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)split).BeginInit();
            split.Panel1.SuspendLayout();
            split.Panel2.SuspendLayout();
            split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridGlobal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridAuswahl).BeginInit();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(cmbTermine);
            topPanel.Controls.Add(lblTermin);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8);
            topPanel.Size = new Size(1100, 48);
            topPanel.TabIndex = 0;
            // 
            // cmbTermine
            // 
            cmbTermine.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTermine.Location = new Point(70, 10);
            cmbTermine.Name = "cmbTermine";
            cmbTermine.Size = new Size(400, 33);
            cmbTermine.TabIndex = 0;
            cmbTermine.SelectedIndexChanged += CmbTermine_SelectedIndexChanged;
            // 
            // lblTermin
            // 
            lblTermin.AutoSize = true;
            lblTermin.Location = new Point(8, 14);
            lblTermin.Name = "lblTermin";
            lblTermin.Size = new Size(68, 25);
            lblTermin.TabIndex = 1;
            lblTermin.Text = "Termin:";
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(btnSpeichern);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 642);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(8);
            bottomPanel.Size = new Size(1100, 58);
            bottomPanel.TabIndex = 2;
            // 
            // btnSpeichern
            // 
            btnSpeichern.Location = new Point(8, 10);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(140, 38);
            btnSpeichern.TabIndex = 0;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.Click += BtnSpeichern_Click;
            // 
            // split
            // 
            split.Dock = DockStyle.Fill;
            split.Location = new Point(0, 48);
            split.Name = "split";
            // 
            // split.Panel1
            // 
            split.Panel1.Controls.Add(gridGlobal);
            // 
            // split.Panel2
            // 
            split.Panel2.Controls.Add(gridAuswahl);
            split.Size = new Size(1100, 594);
            split.SplitterDistance = 541;
            split.TabIndex = 1;
            // 
            // gridGlobal
            // 
            gridGlobal.AllowUserToAddRows = false;
            gridGlobal.AllowUserToDeleteRows = false;
            gridGlobal.ColumnHeadersHeight = 34;
            gridGlobal.Dock = DockStyle.Fill;
            gridGlobal.Location = new Point(0, 0);
            gridGlobal.Name = "gridGlobal";
            gridGlobal.RowHeadersVisible = false;
            gridGlobal.RowHeadersWidth = 62;
            gridGlobal.SelectionMode = DataGridViewSelectionMode.CellSelect;
            gridGlobal.Size = new Size(541, 594);
            gridGlobal.TabIndex = 0;
            gridGlobal.CellValueChanged += GridGlobal_CellValueChanged;
            gridGlobal.CurrentCellDirtyStateChanged += GridGlobal_CurrentCellDirtyStateChanged;
            // 
            // gridAuswahl
            // 
            gridAuswahl.AllowUserToAddRows = false;
            gridAuswahl.AllowUserToDeleteRows = false;
            gridAuswahl.ColumnHeadersHeight = 34;
            gridAuswahl.Dock = DockStyle.Fill;
            gridAuswahl.Location = new Point(0, 0);
            gridAuswahl.Name = "gridAuswahl";
            gridAuswahl.RowHeadersVisible = false;
            gridAuswahl.RowHeadersWidth = 62;
            gridAuswahl.Size = new Size(555, 594);
            gridAuswahl.TabIndex = 0;
            // 
            // AktuelleSpeisekarteForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 700);
            Controls.Add(split);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
            Name = "AktuelleSpeisekarteForm";
            Text = "Aktuelle Speisekarte";
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            bottomPanel.ResumeLayout(false);
            split.Panel1.ResumeLayout(false);
            split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)split).EndInit();
            split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridGlobal).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridAuswahl).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel topPanel;
        private Label lblTermin;
        private ComboBox cmbTermine;
        private Panel bottomPanel;
        private Button btnSpeichern;
        private SplitContainer split;
        private DataGridView gridGlobal;
        private DataGridView gridAuswahl;
    }
}

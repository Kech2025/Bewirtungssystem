namespace Bewirtung
{
    partial class AbrechnungTagForm
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
            lblTermin = new Label();
            cmbFilterTermin = new ComboBox();
            lblBenutzer = new Label();
            cmbFilterBenutzer = new ComboBox();
            lblTisch = new Label();
            cmbFilterTisch = new ComboBox();
            grpArt = new GroupBox();
            rbBeides = new RadioButton();
            rbGetraenke = new RadioButton();
            rbSpeisen = new RadioButton();
            dataGridViewAbrechnung = new DataGridView();
            lblUmsatzVerkauf = new Label();
            lblMwstSumme = new Label();
            lblUmsatzAuftrag = new Label();
            lblTrinkgeld = new Label();
            grpArt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAbrechnung).BeginInit();
            SuspendLayout();
            // 
            // lblTermin
            // 
            lblTermin.AutoSize = true;
            lblTermin.Location = new Point(12, 15);
            lblTermin.Name = "lblTermin";
            lblTermin.Size = new Size(68, 25);
            lblTermin.TabIndex = 0;
            lblTermin.Text = "Termin:";
            // 
            // cmbFilterTermin
            // 
            cmbFilterTermin.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterTermin.Location = new Point(110, 12);
            cmbFilterTermin.Name = "cmbFilterTermin";
            cmbFilterTermin.Size = new Size(360, 33);
            cmbFilterTermin.TabIndex = 1;
            cmbFilterTermin.SelectedIndexChanged += cmbFilterTermin_SelectedIndexChanged;
            // 
            // lblBenutzer
            // 
            lblBenutzer.AutoSize = true;
            lblBenutzer.Location = new Point(12, 55);
            lblBenutzer.Name = "lblBenutzer";
            lblBenutzer.Size = new Size(84, 25);
            lblBenutzer.TabIndex = 2;
            lblBenutzer.Text = "Benutzer:";
            // 
            // cmbFilterBenutzer
            // 
            cmbFilterBenutzer.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterBenutzer.Location = new Point(110, 52);
            cmbFilterBenutzer.Name = "cmbFilterBenutzer";
            cmbFilterBenutzer.Size = new Size(360, 33);
            cmbFilterBenutzer.TabIndex = 3;
            cmbFilterBenutzer.SelectedIndexChanged += cmbFilter_Changed;
            // 
            // lblTisch
            // 
            lblTisch.AutoSize = true;
            lblTisch.Location = new Point(500, 15);
            lblTisch.Name = "lblTisch";
            lblTisch.Size = new Size(55, 25);
            lblTisch.TabIndex = 4;
            lblTisch.Text = "Tisch:";
            // 
            // cmbFilterTisch
            // 
            cmbFilterTisch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterTisch.Location = new Point(590, 12);
            cmbFilterTisch.Name = "cmbFilterTisch";
            cmbFilterTisch.Size = new Size(309, 33);
            cmbFilterTisch.TabIndex = 5;
            cmbFilterTisch.SelectedIndexChanged += cmbFilter_Changed;
            // 
            // grpArt
            // 
            grpArt.Controls.Add(rbBeides);
            grpArt.Controls.Add(rbGetraenke);
            grpArt.Controls.Add(rbSpeisen);
            grpArt.Location = new Point(921, 5);
            grpArt.Name = "grpArt";
            grpArt.Size = new Size(329, 80);
            grpArt.TabIndex = 6;
            grpArt.TabStop = false;
            grpArt.Text = "Art";
            // 
            // rbBeides
            // 
            rbBeides.AutoSize = true;
            rbBeides.Checked = true;
            rbBeides.Location = new Point(227, 35);
            rbBeides.Name = "rbBeides";
            rbBeides.Size = new Size(88, 29);
            rbBeides.TabIndex = 2;
            rbBeides.TabStop = true;
            rbBeides.Text = "Beides";
            rbBeides.CheckedChanged += cmbFilter_Changed;
            // 
            // rbGetraenke
            // 
            rbGetraenke.AutoSize = true;
            rbGetraenke.Location = new Point(114, 35);
            rbGetraenke.Name = "rbGetraenke";
            rbGetraenke.Size = new Size(107, 29);
            rbGetraenke.TabIndex = 1;
            rbGetraenke.Text = "Getränke";
            rbGetraenke.CheckedChanged += cmbFilter_Changed;
            // 
            // rbSpeisen
            // 
            rbSpeisen.AutoSize = true;
            rbSpeisen.Location = new Point(10, 35);
            rbSpeisen.Name = "rbSpeisen";
            rbSpeisen.Size = new Size(98, 29);
            rbSpeisen.TabIndex = 0;
            rbSpeisen.Text = "Speisen";
            rbSpeisen.CheckedChanged += cmbFilter_Changed;
            // 
            // dataGridViewAbrechnung
            // 
            dataGridViewAbrechnung.AllowUserToAddRows = false;
            dataGridViewAbrechnung.AllowUserToDeleteRows = false;
            dataGridViewAbrechnung.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewAbrechnung.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAbrechnung.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridViewAbrechnung.Location = new Point(12, 100);
            dataGridViewAbrechnung.MultiSelect = false;
            dataGridViewAbrechnung.Name = "dataGridViewAbrechnung";
            dataGridViewAbrechnung.ReadOnly = true;
            dataGridViewAbrechnung.RowHeadersVisible = false;
            dataGridViewAbrechnung.RowHeadersWidth = 62;
            dataGridViewAbrechnung.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAbrechnung.Size = new Size(1312, 434);
            dataGridViewAbrechnung.TabIndex = 7;
            // 
            // lblUmsatzVerkauf
            // 
            lblUmsatzVerkauf.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblUmsatzVerkauf.Location = new Point(12, 542);
            lblUmsatzVerkauf.Name = "lblUmsatzVerkauf";
            lblUmsatzVerkauf.Size = new Size(1312, 32);
            lblUmsatzVerkauf.TabIndex = 8;
            lblUmsatzVerkauf.Text = "Umsatz aus Verkauf:";
            lblUmsatzVerkauf.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblMwstSumme
            // 
            lblMwstSumme.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblMwstSumme.Location = new Point(12, 576);
            lblMwstSumme.Name = "lblMwstSumme";
            lblMwstSumme.Size = new Size(1312, 32);
            lblMwstSumme.TabIndex = 9;
            lblMwstSumme.Text = "Mehrwertsteuer:";
            lblMwstSumme.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblUmsatzAuftrag
            // 
            lblUmsatzAuftrag.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblUmsatzAuftrag.Location = new Point(12, 610);
            lblUmsatzAuftrag.Name = "lblUmsatzAuftrag";
            lblUmsatzAuftrag.Size = new Size(1312, 32);
            lblUmsatzAuftrag.TabIndex = 10;
            lblUmsatzAuftrag.Text = "Umsatz aus Auftrag:";
            lblUmsatzAuftrag.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTrinkgeld
            // 
            lblTrinkgeld.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTrinkgeld.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTrinkgeld.Location = new Point(12, 644);
            lblTrinkgeld.Name = "lblTrinkgeld";
            lblTrinkgeld.Size = new Size(1312, 32);
            lblTrinkgeld.TabIndex = 11;
            lblTrinkgeld.Text = "Trinkgeld:";
            lblTrinkgeld.TextAlign = ContentAlignment.MiddleRight;
            // 
            // AbrechnungTagForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1336, 700);
            Controls.Add(lblTermin);
            Controls.Add(cmbFilterTermin);
            Controls.Add(lblBenutzer);
            Controls.Add(cmbFilterBenutzer);
            Controls.Add(lblTisch);
            Controls.Add(cmbFilterTisch);
            Controls.Add(grpArt);
            Controls.Add(dataGridViewAbrechnung);
            Controls.Add(lblUmsatzVerkauf);
            Controls.Add(lblMwstSumme);
            Controls.Add(lblUmsatzAuftrag);
            Controls.Add(lblTrinkgeld);
            Name = "AbrechnungTagForm";
            Text = "Tagesabrechnung";
            grpArt.ResumeLayout(false);
            grpArt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAbrechnung).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblTermin;
        private ComboBox cmbFilterTermin;
        private Label lblBenutzer;
        private ComboBox cmbFilterBenutzer;
        private Label lblTisch;
        private ComboBox cmbFilterTisch;
        private GroupBox grpArt;
        private RadioButton rbBeides;
        private RadioButton rbGetraenke;
        private RadioButton rbSpeisen;
        private DataGridView dataGridViewAbrechnung;
        private Label lblUmsatzVerkauf;
        private Label lblMwstSumme;
        private Label lblUmsatzAuftrag;
        private Label lblTrinkgeld;
    }
}

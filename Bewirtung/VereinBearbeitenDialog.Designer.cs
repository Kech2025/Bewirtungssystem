namespace Bewirtung
{
    partial class VereinBearbeitenDialog
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
            lblName = new Label();
            txtName = new TextBox();
            lblStrasse = new Label();
            txtStrasse = new TextBox();
            lblPlz = new Label();
            txtPlz = new TextBox();
            lblOrt = new Label();
            txtOrt = new TextBox();
            lblAnsprechpartner = new Label();
            cmbAnsprechpartner = new ComboBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(20, 25);
            lblName.Name = "lblName";
            lblName.Size = new Size(64, 25);
            lblName.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(200, 22);
            txtName.Name = "txtName";
            txtName.Size = new Size(350, 31);
            txtName.TabIndex = 0;
            // 
            // lblStrasse
            // 
            lblStrasse.AutoSize = true;
            lblStrasse.Location = new Point(20, 75);
            lblStrasse.Name = "lblStrasse";
            lblStrasse.Size = new Size(72, 25);
            lblStrasse.Text = "Straße";
            // 
            // txtStrasse
            // 
            txtStrasse.Location = new Point(200, 72);
            txtStrasse.MaxLength = 50;
            txtStrasse.Name = "txtStrasse";
            txtStrasse.Size = new Size(350, 31);
            txtStrasse.TabIndex = 1;
            // 
            // lblPlz
            // 
            lblPlz.AutoSize = true;
            lblPlz.Location = new Point(20, 125);
            lblPlz.Name = "lblPlz";
            lblPlz.Size = new Size(43, 25);
            lblPlz.Text = "PLZ";
            // 
            // txtPlz
            // 
            txtPlz.Location = new Point(200, 122);
            txtPlz.MaxLength = 10;
            txtPlz.Name = "txtPlz";
            txtPlz.Size = new Size(150, 31);
            txtPlz.TabIndex = 2;
            // 
            // lblOrt
            // 
            lblOrt.AutoSize = true;
            lblOrt.Location = new Point(20, 175);
            lblOrt.Name = "lblOrt";
            lblOrt.Size = new Size(37, 25);
            lblOrt.Text = "Ort";
            // 
            // txtOrt
            // 
            txtOrt.Location = new Point(200, 172);
            txtOrt.MaxLength = 50;
            txtOrt.Name = "txtOrt";
            txtOrt.Size = new Size(350, 31);
            txtOrt.TabIndex = 3;
            // 
            // lblAnsprechpartner
            // 
            lblAnsprechpartner.AutoSize = true;
            lblAnsprechpartner.Location = new Point(20, 225);
            lblAnsprechpartner.Name = "lblAnsprechpartner";
            lblAnsprechpartner.Size = new Size(151, 25);
            lblAnsprechpartner.Text = "Ansprechpartner";
            // 
            // cmbAnsprechpartner
            // 
            cmbAnsprechpartner.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAnsprechpartner.Location = new Point(200, 222);
            cmbAnsprechpartner.Name = "cmbAnsprechpartner";
            cmbAnsprechpartner.Size = new Size(350, 33);
            cmbAnsprechpartner.TabIndex = 4;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(310, 285);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 5;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(440, 285);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 6;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // VereinBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(580, 350);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(cmbAnsprechpartner);
            Controls.Add(lblAnsprechpartner);
            Controls.Add(txtOrt);
            Controls.Add(lblOrt);
            Controls.Add(txtPlz);
            Controls.Add(lblPlz);
            Controls.Add(txtStrasse);
            Controls.Add(lblStrasse);
            Controls.Add(txtName);
            Controls.Add(lblName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "VereinBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Verein";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private Label lblStrasse;
        private TextBox txtStrasse;
        private Label lblPlz;
        private TextBox txtPlz;
        private Label lblOrt;
        private TextBox txtOrt;
        private Label lblAnsprechpartner;
        private ComboBox cmbAnsprechpartner;
        private Button btnSpeichern;
        private Button btnAbbrechen;
    }
}

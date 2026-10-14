namespace Bewirtung
{
    partial class BenutzerBearbeitenDialog
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
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblTelefon = new Label();
            txtTelefon = new TextBox();
            lblVerein = new Label();
            cmbVerein = new ComboBox();
            chkAdministrator = new CheckBox();
            btnSpeichern = new Button();
            btnAbbrechen = new Button();
            cmdNeuerVerein = new Button();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(20, 25);
            lblName.Name = "lblName";
            lblName.Size = new Size(59, 25);
            lblName.TabIndex = 12;
            lblName.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(180, 22);
            txtName.Name = "txtName";
            txtName.Size = new Size(350, 31);
            txtName.TabIndex = 0;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(20, 75);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(61, 25);
            lblEmail.TabIndex = 11;
            lblEmail.Text = "E-Mail";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(180, 72);
            txtEmail.MaxLength = 100;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(350, 31);
            txtEmail.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 125);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(82, 25);
            lblPassword.TabIndex = 10;
            lblPassword.Text = "Passwort";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(180, 122);
            txtPassword.MaxLength = 100;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(350, 31);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblTelefon
            // 
            lblTelefon.AutoSize = true;
            lblTelefon.Location = new Point(20, 175);
            lblTelefon.Name = "lblTelefon";
            lblTelefon.Size = new Size(68, 25);
            lblTelefon.TabIndex = 9;
            lblTelefon.Text = "Telefon";
            // 
            // txtTelefon
            // 
            txtTelefon.Location = new Point(180, 172);
            txtTelefon.Name = "txtTelefon";
            txtTelefon.Size = new Size(350, 31);
            txtTelefon.TabIndex = 3;
            // 
            // lblVerein
            // 
            lblVerein.AutoSize = true;
            lblVerein.Location = new Point(20, 284);
            lblVerein.Name = "lblVerein";
            lblVerein.Size = new Size(60, 25);
            lblVerein.TabIndex = 8;
            lblVerein.Text = "Verein";
            // 
            // cmbVerein
            // 
            cmbVerein.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVerein.Location = new Point(180, 281);
            cmbVerein.Name = "cmbVerein";
            cmbVerein.Size = new Size(350, 33);
            cmbVerein.TabIndex = 4;
            // 
            // chkAdministrator
            // 
            chkAdministrator.AutoSize = true;
            chkAdministrator.Location = new Point(180, 333);
            chkAdministrator.Name = "chkAdministrator";
            chkAdministrator.Size = new Size(147, 29);
            chkAdministrator.TabIndex = 5;
            chkAdministrator.Text = "Administrator";
            chkAdministrator.UseVisualStyleBackColor = true;
            // 
            // btnSpeichern
            // 
            btnSpeichern.DialogResult = DialogResult.OK;
            btnSpeichern.Location = new Point(271, 388);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 6;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.DialogResult = DialogResult.Cancel;
            btnAbbrechen.Location = new Point(414, 388);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(120, 40);
            btnAbbrechen.TabIndex = 7;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.UseVisualStyleBackColor = true;
            // 
            // cmdNeuerVerein
            // 
            cmdNeuerVerein.Location = new Point(180, 235);
            cmdNeuerVerein.Name = "cmdNeuerVerein";
            cmdNeuerVerein.Size = new Size(285, 40);
            cmdNeuerVerein.TabIndex = 13;
            cmdNeuerVerein.Text = "Neuer Verein hinzufügen";
            cmdNeuerVerein.UseVisualStyleBackColor = true;
            // 
            // BenutzerBearbeitenDialog
            // 
            AcceptButton = btnSpeichern;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnAbbrechen;
            ClientSize = new Size(560, 440);
            Controls.Add(cmdNeuerVerein);
            Controls.Add(btnAbbrechen);
            Controls.Add(btnSpeichern);
            Controls.Add(chkAdministrator);
            Controls.Add(cmbVerein);
            Controls.Add(lblVerein);
            Controls.Add(txtTelefon);
            Controls.Add(lblTelefon);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(txtName);
            Controls.Add(lblName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BenutzerBearbeitenDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Benutzer";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblTelefon;
        private TextBox txtTelefon;
        private Label lblVerein;
        private ComboBox cmbVerein;
        private CheckBox chkAdministrator;
        private Button btnSpeichern;
        private Button btnAbbrechen;
        private Button cmdNeuerVerein;
    }
}

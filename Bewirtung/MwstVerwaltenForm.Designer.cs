namespace Bewirtung
{
    partial class MwstVerwaltenForm
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
            dataGridViewMwst = new DataGridView();
            btnSpeichern = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMwst).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewMwst
            // 
            dataGridViewMwst.AllowUserToAddRows = false;
            dataGridViewMwst.AllowUserToDeleteRows = false;
            dataGridViewMwst.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewMwst.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMwst.Location = new Point(12, 60);
            dataGridViewMwst.MultiSelect = false;
            dataGridViewMwst.Name = "dataGridViewMwst";
            dataGridViewMwst.RowHeadersVisible = false;
            dataGridViewMwst.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMwst.Size = new Size(576, 428);
            dataGridViewMwst.TabIndex = 0;
            // 
            // btnSpeichern
            // 
            btnSpeichern.Location = new Point(468, 6);
            btnSpeichern.Name = "btnSpeichern";
            btnSpeichern.Size = new Size(120, 40);
            btnSpeichern.TabIndex = 1;
            btnSpeichern.Text = "Speichern";
            btnSpeichern.UseVisualStyleBackColor = true;
            btnSpeichern.Click += btnSpeichern_Click;
            // 
            // MwstVerwaltenForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 500);
            Controls.Add(btnSpeichern);
            Controls.Add(dataGridViewMwst);
            Name = "MwstVerwaltenForm";
            Text = "Mehrwertsteuer verwalten";
            ((System.ComponentModel.ISupportInitialize)dataGridViewMwst).EndInit();
            ResumeLayout(false);
        }

        private DataGridView dataGridViewMwst;
        private Button btnSpeichern;
    }
}

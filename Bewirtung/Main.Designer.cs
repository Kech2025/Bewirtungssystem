namespace Bewirtung
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem stammdatenToolStripMenuItem;
        private ToolStripMenuItem einstellungenProTerminToolStripMenuItem;
        private ToolStripMenuItem benutzerToolStripMenuItem;
        private ToolStripMenuItem vereinToolStripMenuItem;
        private ToolStripMenuItem globalePreislisteToolStripMenuItem;
        private ToolStripMenuItem helferZuTermineToolStripMenuItem;
        private ToolStripMenuItem termineVerwaltenToolStripMenuItem;
        private ToolStripMenuItem bereichTischeToolStripMenuItem;
        private ToolStripMenuItem tischeVerwaltenToolStripMenuItem;
        private ToolStripMenuItem tischeProTerminToolStripMenuItem;
        private ToolStripMenuItem aktuellePreislisteToolStripMenuItem;
        private ToolStripMenuItem abrechnungToolStripMenuItem;
        private ToolStripMenuItem tagAbrechnenToolStripMenuItem;
        private ToolStripMenuItem listenToolStripMenuItem;
        private ToolStripMenuItem offeneLieferpositionenToolStripMenuItem;
        private ToolStripMenuItem offeneRechnungspositionenToolStripMenuItem;
        private ToolStripMenuItem offeneBestellungenToolStripMenuItem;
        private ToolStripMenuItem uebersichtTagToolStripMenuItem;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStripMain = new MenuStrip();
            stammdatenToolStripMenuItem = new ToolStripMenuItem();
            einstellungenProTerminToolStripMenuItem = new ToolStripMenuItem();
            benutzerToolStripMenuItem = new ToolStripMenuItem();
            vereinToolStripMenuItem = new ToolStripMenuItem();
            globalePreislisteToolStripMenuItem = new ToolStripMenuItem();
            helferZuTermineToolStripMenuItem = new ToolStripMenuItem();
            termineVerwaltenToolStripMenuItem = new ToolStripMenuItem();
            bereichTischeToolStripMenuItem = new ToolStripMenuItem();
            tischeVerwaltenToolStripMenuItem = new ToolStripMenuItem();
            tischeProTerminToolStripMenuItem = new ToolStripMenuItem();
            aktuellePreislisteToolStripMenuItem = new ToolStripMenuItem();
            abrechnungToolStripMenuItem = new ToolStripMenuItem();
            tagAbrechnenToolStripMenuItem = new ToolStripMenuItem();
            listenToolStripMenuItem = new ToolStripMenuItem();
            offeneLieferpositionenToolStripMenuItem = new ToolStripMenuItem();
            offeneRechnungspositionenToolStripMenuItem = new ToolStripMenuItem();
            offeneBestellungenToolStripMenuItem = new ToolStripMenuItem();
            uebersichtTagToolStripMenuItem = new ToolStripMenuItem();
            menuStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripMain
            // 
            menuStripMain.Items.AddRange(new ToolStripItem[] { stammdatenToolStripMenuItem, einstellungenProTerminToolStripMenuItem, abrechnungToolStripMenuItem, listenToolStripMenuItem });
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.Size = new Size(2047, 33);
            menuStripMain.TabIndex = 0;
            menuStripMain.Text = "menuStripMain";
            // 
            // stammdatenToolStripMenuItem
            // 
            stammdatenToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { benutzerToolStripMenuItem, vereinToolStripMenuItem, globalePreislisteToolStripMenuItem, termineVerwaltenToolStripMenuItem, bereichTischeToolStripMenuItem, tischeVerwaltenToolStripMenuItem });
            stammdatenToolStripMenuItem.Name = "stammdatenToolStripMenuItem";
            stammdatenToolStripMenuItem.Size = new Size(133, 29);
            stammdatenToolStripMenuItem.Text = "Stammdaten";
            // 
            // einstellungenProTerminToolStripMenuItem
            // 
            einstellungenProTerminToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tischeProTerminToolStripMenuItem, helferZuTermineToolStripMenuItem, aktuellePreislisteToolStripMenuItem });
            einstellungenProTerminToolStripMenuItem.Name = "einstellungenProTerminToolStripMenuItem";
            einstellungenProTerminToolStripMenuItem.Size = new Size(200, 29);
            einstellungenProTerminToolStripMenuItem.Text = "Einstellungen pro Termin";
            // 
            // benutzerToolStripMenuItem
            // 
            benutzerToolStripMenuItem.Name = "benutzerToolStripMenuItem";
            benutzerToolStripMenuItem.Size = new Size(270, 34);
            benutzerToolStripMenuItem.Text = "Benutzer";
            benutzerToolStripMenuItem.Click += benutzerToolStripMenuItem_Click;
            // 
            // vereinToolStripMenuItem
            // 
            vereinToolStripMenuItem.Name = "vereinToolStripMenuItem";
            vereinToolStripMenuItem.Size = new Size(270, 34);
            vereinToolStripMenuItem.Text = "Verein";
            vereinToolStripMenuItem.Click += vereinToolStripMenuItem_Click;
            // 
            // globalePreislisteToolStripMenuItem
            // 
            globalePreislisteToolStripMenuItem.Name = "globalePreislisteToolStripMenuItem";
            globalePreislisteToolStripMenuItem.Size = new Size(270, 34);
            globalePreislisteToolStripMenuItem.Text = "Globale Preisliste";
            globalePreislisteToolStripMenuItem.Click += globalePreislisteToolStripMenuItem_Click;            
            // 
            // helferZuTermineToolStripMenuItem
            // 
            helferZuTermineToolStripMenuItem.Name = "helferZuTermineToolStripMenuItem";
            helferZuTermineToolStripMenuItem.Size = new Size(270, 34);
            helferZuTermineToolStripMenuItem.Text = "Helfer zu Termine verwalten";
            helferZuTermineToolStripMenuItem.Click += helferZuTermineToolStripMenuItem_Click;
            // 
            // termineVerwaltenToolStripMenuItem
            // 
            termineVerwaltenToolStripMenuItem.Name = "termineVerwaltenToolStripMenuItem";
            termineVerwaltenToolStripMenuItem.Size = new Size(270, 34);
            termineVerwaltenToolStripMenuItem.Text = "Termine verwalten";
            termineVerwaltenToolStripMenuItem.Click += termineVerwaltenToolStripMenuItem_Click;
            // 
            // bereichTischeToolStripMenuItem
            // 
            bereichTischeToolStripMenuItem.Name = "bereichTischeToolStripMenuItem";
            bereichTischeToolStripMenuItem.Size = new Size(270, 34);
            bereichTischeToolStripMenuItem.Text = "Bewirtungsbereiche verwalten";
            bereichTischeToolStripMenuItem.Click += bereichTischeToolStripMenuItem_Click;
            // 
            // tischeVerwaltenToolStripMenuItem
            // 
            tischeVerwaltenToolStripMenuItem.Name = "tischeVerwaltenToolStripMenuItem";
            tischeVerwaltenToolStripMenuItem.Size = new Size(270, 34);
            tischeVerwaltenToolStripMenuItem.Text = "Tische verwalten";
            tischeVerwaltenToolStripMenuItem.Click += tischeVerwaltenToolStripMenuItem_Click;
            // 
            // tischeProTerminToolStripMenuItem
            // 
            tischeProTerminToolStripMenuItem.Name = "tischeProTerminToolStripMenuItem";
            tischeProTerminToolStripMenuItem.Size = new Size(270, 34);
            tischeProTerminToolStripMenuItem.Text = "Tische pro Termin verwalten";
            tischeProTerminToolStripMenuItem.Click += tischeProTerminToolStripMenuItem_Click;
            // 
            // aktuellePreislisteToolStripMenuItem
            // 
            aktuellePreislisteToolStripMenuItem.Name = "aktuellePreislisteToolStripMenuItem";
            aktuellePreislisteToolStripMenuItem.Size = new Size(270, 34);
            aktuellePreislisteToolStripMenuItem.Text = "Preisliste pro Termin verwalten";
            aktuellePreislisteToolStripMenuItem.Click += aktuellePreislisteToolStripMenuItem_Click;            
            // 
            // abrechnungToolStripMenuItem
            // 
            abrechnungToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tagAbrechnenToolStripMenuItem});
            abrechnungToolStripMenuItem.Name = "abrechnungToolStripMenuItem";
            abrechnungToolStripMenuItem.Size = new Size(101, 29);
            abrechnungToolStripMenuItem.Text = "Abrechnung";
            // 
            // tagAbrechnenToolStripMenuItem
            // 
            tagAbrechnenToolStripMenuItem.Name = "tagAbrechnenToolStripMenuItem";
            tagAbrechnenToolStripMenuItem.Size = new Size(250, 34);
            tagAbrechnenToolStripMenuItem.Text = "Tagesabrechnung";
            tagAbrechnenToolStripMenuItem.Click += tagAbrechnenToolStripMenuItem_Click;           
            // 
            // listenToolStripMenuItem
            // 
            listenToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { offeneLieferpositionenToolStripMenuItem, offeneRechnungspositionenToolStripMenuItem, offeneBestellungenToolStripMenuItem, uebersichtTagToolStripMenuItem });
            listenToolStripMenuItem.Name = "listenToolStripMenuItem";
            listenToolStripMenuItem.Size = new Size(69, 29);
            listenToolStripMenuItem.Text = "Listen";
            // 
            // offeneLieferpositionenToolStripMenuItem
            // 
            offeneLieferpositionenToolStripMenuItem.Name = "offeneLieferpositionenToolStripMenuItem";
            offeneLieferpositionenToolStripMenuItem.Size = new Size(300, 34);
            offeneLieferpositionenToolStripMenuItem.Text = "Offene Lieferpositionen";
            offeneLieferpositionenToolStripMenuItem.Click += offeneLieferpositionenToolStripMenuItem_Click;
            // 
            // offeneRechnungspositionenToolStripMenuItem
            // 
            offeneRechnungspositionenToolStripMenuItem.Name = "offeneRechnungspositionenToolStripMenuItem";
            offeneRechnungspositionenToolStripMenuItem.Size = new Size(300, 34);
            offeneRechnungspositionenToolStripMenuItem.Text = "Offene Rechnungspositionen";
            offeneRechnungspositionenToolStripMenuItem.Click += offeneRechnungspositionenToolStripMenuItem_Click;
            // 
            // offeneBestellungenToolStripMenuItem
            // 
            offeneBestellungenToolStripMenuItem.Name = "offeneBestellungenToolStripMenuItem";
            offeneBestellungenToolStripMenuItem.Size = new Size(300, 34);
            offeneBestellungenToolStripMenuItem.Text = "Offene Bestellungen";
            offeneBestellungenToolStripMenuItem.Click += offeneBestellungenToolStripMenuItem_Click;
            // 
            // uebersichtTagToolStripMenuItem
            // 
            uebersichtTagToolStripMenuItem.Name = "uebersichtTagToolStripMenuItem";
            uebersichtTagToolStripMenuItem.Size = new Size(300, 34);
            uebersichtTagToolStripMenuItem.Text = "Übersicht Tag";
            uebersichtTagToolStripMenuItem.Click += uebersichtTagToolStripMenuItem_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2047, 1045);
            Controls.Add(menuStripMain);
            IsMdiContainer = true;
            MainMenuStrip = menuStripMain;
            Name = "Main";
            Text = "Bewirtung";
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}

namespace Bewirtung
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void benutzerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<BenutzerForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new BenutzerForm { MdiParent = this };
            form.Show();
        }

        private void vereinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<VereinForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new VereinForm { MdiParent = this };
            form.Show();
        }

        private void globalePreislisteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<GlobalePreislisteForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new GlobalePreislisteForm { MdiParent = this };
            form.Show();
        }

        private void helferZuTermineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<TerminForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new TerminForm { MdiParent = this };
            form.Show();
        }

        private void termineVerwaltenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<TermineVerwaltenForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new TermineVerwaltenForm { MdiParent = this };
            form.Show();
        }

        private void bereichTischeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<BereichTischeVerwaltenForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new BereichTischeVerwaltenForm { MdiParent = this };
            form.Show();
        }

        private void tischeVerwaltenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<TischeVerwaltenForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new TischeVerwaltenForm { MdiParent = this };
            form.Show();
        }

        private void tischeProTerminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<TischeProTerminVerwaltenForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new TischeProTerminVerwaltenForm { MdiParent = this };
            form.Show();
        }

        private void aktuellePreislisteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var existing = MdiChildren.OfType<AktuelleSpeisekarteForm>().FirstOrDefault();
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new AktuelleSpeisekarteForm { MdiParent = this };
            form.Show();
        }

        private void auftragErfassenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void auftragAbrechnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void offeneLieferpositionenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void offeneRechnungspositionenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void offeneBestellungenToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void uebersichtTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}

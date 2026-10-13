using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bewirtung
{
    /// <summary>
    /// Dialog zur Auswahl eines neuen Termindatums über einen Kalender.
    /// </summary>
    public partial class NeuerTerminDialog : Form
    {
        /// <summary>Das ausgewählte Datum (ohne Uhrzeit).</summary>
        public DateTime AusgewaehltesDatum => monthCalendar.SelectionStart.Date;

        public NeuerTerminDialog()
        {
            InitializeComponent();
        }

        public NeuerTerminDialog(DateTime vorauswahl) : this()
        {
            monthCalendar.SelectionStart = vorauswahl.Date;
            monthCalendar.SelectionEnd = vorauswahl.Date;
        }
    }
}

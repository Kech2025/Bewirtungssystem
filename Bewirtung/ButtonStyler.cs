using System.Drawing;
using System.Windows.Forms;

namespace Bewirtung
{
    /// <summary>
    /// Setzt global für alle Buttons der Anwendung die Hintergrundfarbe auf
    /// <see cref="SystemColors.ActiveCaption"/>. Wird einmalig in Program.Main
    /// aktiviert und färbt über das Application.Idle-Ereignis automatisch die
    /// Buttons aller offenen Forms und Dialoge ein – auch neu geöffnete.
    /// </summary>
    internal static class ButtonStyler
    {
        public static void Enable()
        {
            Application.Idle += (_, _) => ApplyToOpenForms();
        }

        private static void ApplyToOpenForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                ApplyToControl(form);
            }
        }

        private static void ApplyToControl(Control control)
        {
            foreach (Control child in control.Controls)
            {
                if (child is Button button)
                {
                    Color target = button.Enabled
                        ? SystemColors.ActiveCaption
                        : SystemColors.ControlLightLight;

                    if (button.BackColor != target)
                    {
                        button.UseVisualStyleBackColor = false;
                        button.BackColor = target;
                    }
                }

                if (child.HasChildren)
                {
                    ApplyToControl(child);
                }
            }
        }
    }
}

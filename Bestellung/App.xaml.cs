using Microsoft.Extensions.DependencyInjection;
using Bestellung.Data;

namespace Bestellung
{
    public partial class App : Application
    {
        private readonly JournalApiClient _journalApiClient = new();

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Beim App-Start eine evtl. aus einer früheren Sitzung noch offene Anmeldung
            // nachträglich abmelden (App wurde geschlossen, ohne abzumelden).
            window.Created += async (_, _) =>
            {
                try
                {
                    await _journalApiClient.BeendeOffeneSitzungAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Journal-Nachhol-Logout fehlgeschlagen: {ex.Message}");
                }
            };

            // Beim Schließen/Stoppen der App die offene Sitzung best-effort abmelden.
            window.Stopped += async (_, _) =>
            {
                try
                {
                    await _journalApiClient.BeendeOffeneSitzungAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Journal-Logout beim Schließen fehlgeschlagen: {ex.Message}");
                }
            };

            return window;
        }
    }
}
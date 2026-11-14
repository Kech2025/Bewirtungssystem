namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Hält den aktuell angemeldeten Benutzer für die Dauer eines Blazor-Circuits.
    /// </summary>
    public class SessionState
    {
        private readonly JournalRepository _journalRepository;

        public SessionState(JournalRepository journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public Benutzer? AngemeldeterBenutzer { get; private set; }

        public bool IstAngemeldet => AngemeldeterBenutzer is not null;

        public bool IstAdmin => AngemeldeterBenutzer?.Administrator ?? false;

        public event Action? OnChange;

        public void Anmelden(Benutzer benutzer)
        {
            AngemeldeterBenutzer = benutzer;

            // Website-Anmeldung im Journal protokollieren (Modus = Webseite).
            try
            {
                _journalRepository.Login(benutzer.UserId, JournalRepository.ModusWebseite);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Journal-Login (Webseite) fehlgeschlagen: {ex.Message}");
            }

            OnChange?.Invoke();
        }

        public void Abmelden()
        {
            // Website-Abmeldung im Journal protokollieren.
            if (AngemeldeterBenutzer is { } benutzer)
            {
                try
                {
                    _journalRepository.Logout(benutzer.UserId, JournalRepository.ModusWebseite);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Journal-Logout (Webseite) fehlgeschlagen: {ex.Message}");
                }
            }

            AngemeldeterBenutzer = null;
            OnChange?.Invoke();
        }

        /// <summary>
        /// Wird beim endgültigen Schließen des Blazor-Circuits aufgerufen (Browser/Tab zu).
        /// Protokolliert den Website-Logout, ohne UI-Benachrichtigungen auszulösen.
        /// </summary>
        public void AbmeldenBeiCircuitEnde()
        {
            if (AngemeldeterBenutzer is { } benutzer)
            {
                try
                {
                    _journalRepository.Logout(benutzer.UserId, JournalRepository.ModusWebseite);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Journal-Logout (Circuit-Ende) fehlgeschlagen: {ex.Message}");
                }
            }

            AngemeldeterBenutzer = null;
        }
    }
}

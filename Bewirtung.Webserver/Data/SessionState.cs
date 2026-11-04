namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Hält den aktuell angemeldeten Benutzer für die Dauer eines Blazor-Circuits.
    /// </summary>
    public class SessionState
    {
        public Benutzer? AngemeldeterBenutzer { get; private set; }

        public bool IstAngemeldet => AngemeldeterBenutzer is not null;

        public bool IstAdmin => AngemeldeterBenutzer?.Administrator ?? false;

        public event Action? OnChange;

        public void Anmelden(Benutzer benutzer)
        {
            AngemeldeterBenutzer = benutzer;
            OnChange?.Invoke();
        }

        public void Abmelden()
        {
            AngemeldeterBenutzer = null;
            OnChange?.Invoke();
        }
    }
}

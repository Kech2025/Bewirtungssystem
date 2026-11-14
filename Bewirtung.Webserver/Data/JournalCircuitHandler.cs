using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Beendet beim Schließen des Blazor-Circuits (Browser/Tab geschlossen oder
    /// Verbindungsabbruch) eine noch offene Website-Anmeldung im Journal. Ist an
    /// denselben Scope wie <see cref="SessionState"/> gebunden.
    /// </summary>
    public class JournalCircuitHandler : CircuitHandler
    {
        private readonly SessionState _sessionState;

        public JournalCircuitHandler(SessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            // Circuit endgültig geschlossen (Browser/Tab zu) – offene Website-Anmeldung abmelden.
            _sessionState.AbmeldenBeiCircuitEnde();
            return Task.CompletedTask;
        }
    }
}

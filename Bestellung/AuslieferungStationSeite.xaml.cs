using System.Collections.ObjectModel;
using Bestellung.Data;

namespace Bestellung;

public partial class AuslieferungStationSeite : ContentPage
{
    private readonly AuslieferungApiClient _auslieferungApiClient = new();
    private readonly BenutzerDto _benutzer;
    private readonly TerminDto _termin;
    private readonly KuechenstationDto _station;
    private readonly ObservableCollection<AuslieferungTischDto> _tische = new();

    // Automatisches Aktualisieren, damit neue Bestellungen ohne manuelles Neuladen erscheinen.
    private IDispatcherTimer? _refreshTimer;
    private bool _ladeLaeuft;

    public AuslieferungStationSeite(BenutzerDto benutzer, TerminDto termin, KuechenstationDto station)
    {
        InitializeComponent();
        _benutzer = benutzer;
        _termin = termin;
        _station = station;
        Title = $"Auslieferung {_station.Name}";
        TitelLabel.Text = $"Offene Positionen {_station.Name} – {_termin.Anzeige}";
        TischeView.ItemsSource = _tische;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LadePositionenAsync();
        StarteAutoRefresh();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StoppeAutoRefresh();
    }

    private void StarteAutoRefresh()
    {
        if (_refreshTimer is not null)
        {
            return;
        }

        _refreshTimer = Dispatcher.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(5);
        _refreshTimer.Tick += async (_, _) => await LadePositionenAsync();
        _refreshTimer.Start();
    }

    private void StoppeAutoRefresh()
    {
        if (_refreshTimer is not null)
        {
            _refreshTimer.Stop();
            _refreshTimer = null;
        }
    }

    private async Task LadePositionenAsync()
    {
        if (_ladeLaeuft)
        {
            return;
        }

        _ladeLaeuft = true;
        try
        {
            var tische = await _auslieferungApiClient.GetStationAsync(_termin.BewirtungsDatum, _station.Id);
            MergePositionen(tische);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Positionen konnten nicht geladen werden: {ex.Message}");
        }
        finally
        {
            _ladeLaeuft = false;
        }
    }

    // Aktualisiert die Anzeige, ohne die laufende Benutzereingabe (SeitenMenge) zu verwerfen.
    // Es werden nur neue Tische/Positionen hinzugefügt bzw. nicht mehr offene entfernt und
    // die Serverwerte (Menge/MengeGeliefert) bestehender Positionen aktualisiert.
    private void MergePositionen(List<AuslieferungTischDto> neueTische)
    {
        // Entfernte Tische löschen.
        for (var i = _tische.Count - 1; i >= 0; i--)
        {
            var vorhanden = _tische[i];
            if (!neueTische.Any(t => t.TischNummer == vorhanden.TischNummer))
            {
                _tische.RemoveAt(i);
            }
        }

        foreach (var neuerTisch in neueTische)
        {
            var bestehenderTisch = _tische.FirstOrDefault(t => t.TischNummer == neuerTisch.TischNummer);
            if (bestehenderTisch is null)
            {
                // Neuer Tisch: Seitenmenge auf offene Menge vorbelegen.
                foreach (var position in neuerTisch.Positionen)
                {
                    position.SeitenMenge = position.MaxSeitenMenge;
                }
                _tische.Add(neuerTisch);
                continue;
            }

            // Entfernte Positionen löschen.
            for (var i = bestehenderTisch.Positionen.Count - 1; i >= 0; i--)
            {
                var vorhandenePos = bestehenderTisch.Positionen[i];
                if (!neuerTisch.Positionen.Any(p => p.AuftragId == vorhandenePos.AuftragId && p.PosNr == vorhandenePos.PosNr))
                {
                    bestehenderTisch.Positionen.RemoveAt(i);
                }
            }

            foreach (var neuePos in neuerTisch.Positionen)
            {
                var bestehendePos = bestehenderTisch.Positionen
                    .FirstOrDefault(p => p.AuftragId == neuePos.AuftragId && p.PosNr == neuePos.PosNr);

                if (bestehendePos is null)
                {
                    // Neue Position: Seitenmenge auf offene Menge vorbelegen.
                    neuePos.SeitenMenge = neuePos.MaxSeitenMenge;
                    bestehenderTisch.Positionen.Add(neuePos);
                }
                else
                {
                    // Bestehende Position: nur Serverwerte aktualisieren, Benutzereingabe (SeitenMenge) beibehalten.
                    bestehendePos.Menge = neuePos.Menge;
                    bestehendePos.MengeGeliefert = neuePos.MengeGeliefert;
                    bestehendePos.Besonderheit = neuePos.Besonderheit;
                }
            }
        }
    }

    private void OnPlusClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is AuslieferungPositionDto position)
        {
            position.SeitenMenge++;
        }
    }

    private void OnMinusClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is AuslieferungPositionDto position)
        {
            position.SeitenMenge--;
        }
    }

    private async void OnFertigClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not AuslieferungTischDto tisch)
        {
            return;
        }

        var positionen = tisch.Positionen
            .Select(p => new AuslieferungFertigPosition
            {
                AuftragId = p.AuftragId,
                PosNr = p.PosNr,
                SeitenMenge = p.SeitenMenge
            })
            .ToList();

        try
        {
            await _auslieferungApiClient.FertigAsync(positionen);
            await LadePositionenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auslieferung konnte nicht gemeldet werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Auslieferung konnte nicht gespeichert werden.", "OK");
        }
    }
}

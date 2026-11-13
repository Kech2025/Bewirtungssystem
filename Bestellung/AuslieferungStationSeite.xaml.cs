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
            _tische.Clear();
            foreach (var tisch in tische)
            {
                foreach (var position in tisch.Positionen)
                {
                    var offen = position.Menge - position.MengeGeliefert;
                    position.SeitenMenge = offen > 0m ? (int)offen : 0;
                }
                _tische.Add(tisch);
            }
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

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
    }

    private async Task LadePositionenAsync()
    {
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
            await DisplayAlertAsync("Fehler", "Offene Positionen konnten nicht vom Server geladen werden.", "OK");
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

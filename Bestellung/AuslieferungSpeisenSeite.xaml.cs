using System.Collections.ObjectModel;
using Bestellung.Data;

namespace Bestellung;

public partial class AuslieferungSpeisenSeite : ContentPage
{
    private readonly AuslieferungApiClient _auslieferungApiClient = new();
    private readonly BenutzerDto _benutzer;
    private readonly TerminDto _termin;
    private readonly ObservableCollection<AuslieferungTischDto> _tische = new();

    public AuslieferungSpeisenSeite(BenutzerDto benutzer, TerminDto termin)
    {
        InitializeComponent();
        _benutzer = benutzer;
        _termin = termin;
        TitelLabel.Text = $"Offene Speisen – {_termin.Anzeige}";
        TischeView.ItemsSource = _tische;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LadeSpeisenAsync();
    }

    private async Task LadeSpeisenAsync()
    {
        try
        {
            var tische = await _auslieferungApiClient.GetSpeisenAsync(_termin.BewirtungsDatum);
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
            System.Diagnostics.Debug.WriteLine($"Speisen konnten nicht geladen werden: {ex.Message}");
            await DisplayAlert("Fehler", "Offene Speisen konnten nicht vom Server geladen werden.", "OK");
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
            // Neu laden: Der Tisch verschwindet nur, wenn alle seine Positionen
            // vollständig ausgeliefert sind (Server liefert nur offene Positionen).
            await LadeSpeisenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Auslieferung konnte nicht gemeldet werden: {ex.Message}");
            await DisplayAlert("Fehler", "Die Auslieferung konnte nicht gespeichert werden.", "OK");
        }
    }
}

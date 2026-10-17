namespace Bestellung;

using Bestellung.Data;

public partial class MainPage : ContentPage
{
    private readonly TerminApiClient _terminApiClient = new();
    private readonly BenutzerApiClient _benutzerApiClient = new();

    // Zuletzt verwendete Anmeldung, um sie bei Rückkehr zur Anmeldeseite vorzubelegen.
    private int? _letzterBewirtungsDatum;
    private long? _letzterVereinId;
    private long? _letzterUserId;

    public MainPage()
    {
        InitializeComponent();
        ModusPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LadeTermineAsync();
    }

    private async Task LadeBenutzerFuerTerminAsync(TerminDto termin)
    {
        try
        {
            var benutzer = await _benutzerApiClient.GetBenutzerFuerTerminAsync(termin.BewirtungsDatum, termin.VereinId);
            BenutzerPicker.ItemsSource = benutzer;

            var index = -1;
            if (_letzterUserId is long userId)
            {
                index = benutzer.FindIndex(b => b.UserId == userId);
            }

            BenutzerPicker.SelectedIndex = index >= 0 ? index : (benutzer.Count > 0 ? 0 : -1);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Benutzer konnten nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Benutzer konnten nicht vom Server geladen werden.", "OK");
        }
    }

    private async Task LadeTermineAsync()
    {
        try
        {
            var termine = await _terminApiClient.GetTermineAsync();
            TerminPicker.ItemsSource = termine;

            var index = -1;
            if (_letzterBewirtungsDatum is int datum && _letzterVereinId is long vereinId)
            {
                index = termine.FindIndex(t => t.BewirtungsDatum == datum && t.VereinId == vereinId);
            }

            // Standard: zuletzt verwendeter Termin, sonst aktuellster/erster (API liefert neueste zuerst).
            TerminPicker.SelectedIndex = index >= 0 ? index : (termine.Count > 0 ? 0 : -1);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Termine konnten nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Preislisten (Termine) konnten nicht vom Server geladen werden.", "OK");
        }
    }

    private async void OnTerminSelectionChanged(object? sender, EventArgs e)
    {
        if (TerminPicker.SelectedItem is TerminDto termin)
        {
            await LadeBenutzerFuerTerminAsync(termin);
        }
        else
        {
            BenutzerPicker.ItemsSource = null;
        }
    }

    private async void OnAnmeldenClicked(object? sender, EventArgs e)
    {
        if (BenutzerPicker.SelectedItem is not BenutzerDto benutzer)
        {
            await DisplayAlertAsync("Hinweis", "Bitte einen Benutzer auswählen.", "OK");
            return;
        }

        if (TerminPicker.SelectedItem is not TerminDto termin)
        {
            await DisplayAlertAsync("Hinweis", "Bitte eine Preisliste (Termin) auswählen.", "OK");
            return;
        }

        // Anmeldung merken, um sie bei Rückkehr zur Anmeldeseite vorzubelegen.
        _letzterBewirtungsDatum = termin.BewirtungsDatum;
        _letzterVereinId = termin.VereinId;
        _letzterUserId = benutzer.UserId;

        switch (ModusPicker.SelectedIndex)
        {
            case 1:
                await Navigation.PushAsync(new AuslieferungGetraenkeSeite(benutzer, termin));
                break;
            case 2:
                await Navigation.PushAsync(new AuslieferungSpeisenSeite(benutzer, termin));
                break;
            default:
                await Navigation.PushAsync(new BestellungSeite(benutzer, termin));
                break;
        }
    }
}

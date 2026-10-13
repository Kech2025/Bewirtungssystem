namespace Bestellung;

using Bestellung.Data;

public partial class MainPage : ContentPage
{
    private readonly TerminApiClient _terminApiClient = new();
    private readonly BenutzerApiClient _benutzerApiClient = new();

    public MainPage()
    {
        InitializeComponent();
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
            BenutzerPicker.SelectedIndex = benutzer.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Benutzer konnten nicht geladen werden: {ex.Message}");
            await DisplayAlert("Fehler", "Benutzer konnten nicht vom Server geladen werden.", "OK");
        }
    }

    private async Task LadeTermineAsync()
    {
        try
        {
            var termine = await _terminApiClient.GetTermineAsync();
            TerminPicker.ItemsSource = termine;
            // Standard: aktuellster/erster Termin (API liefert neueste zuerst).
            TerminPicker.SelectedIndex = termine.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Termine konnten nicht geladen werden: {ex.Message}");
            await DisplayAlert("Fehler", "Preislisten (Termine) konnten nicht vom Server geladen werden.", "OK");
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
            await DisplayAlert("Hinweis", "Bitte einen Benutzer auswählen.", "OK");
            return;
        }

        if (TerminPicker.SelectedItem is not TerminDto termin)
        {
            await DisplayAlert("Hinweis", "Bitte eine Preisliste (Termin) auswählen.", "OK");
            return;
        }

        await Navigation.PushAsync(new BestellungSeite(benutzer, termin));
    }
}

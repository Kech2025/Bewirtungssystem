namespace Bestellung;

using Bestellung.Data;

public partial class MainPage : ContentPage
{
    private readonly TerminApiClient _terminApiClient = new();
    private readonly BenutzerApiClient _benutzerApiClient = new();
    private readonly HandyApiClient _handyApiClient = new();
    private readonly KuechenstationApiClient _kuechenstationApiClient = new();
    private readonly JournalApiClient _journalApiClient = new();

    // Im Modus-Picker geführte Küchenstationen (Index im Picker = Index hier + 1, da Index 0 = Bestellung).
    private List<KuechenstationDto> _kuechenstationen = new();

    // Ob dieses Gerät den Bestellmodus anzeigen darf (aus der Tabelle handy, Feld Bestellung).
    private bool _bestellungErlaubt = true;

    // Zuletzt verwendete Anmeldung, um sie bei Rückkehr zur Anmeldeseite vorzubelegen.
    private int? _letzterBewirtungsDatum;
    private long? _letzterVereinId;
    private long? _letzterUserId;

    // Benutzer-ID der aktuell offenen Journal-Sitzung (Login ohne zugehörigen Logout).
    private long? _aktiveJournalUserId;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Kehrt der Benutzer von einer Modus-/Bestellseite zur Anmeldeseite zurück,
        // wird die noch offene Journal-Sitzung abgemeldet.
        await BeendeJournalSitzungAsync();

        await StelleHandyRegistrierungSicherAsync();
        await AktualisiereBestellungFlagAsync();
        await LadeModusAsync();
        await LadeTermineAsync();
    }

    /// <summary>
    /// Schließt eine noch offene Journal-Sitzung (Logout). Idempotent: ohne offene
    /// Sitzung passiert nichts und Fehler werden ignoriert.
    /// </summary>
    private async Task BeendeJournalSitzungAsync()
    {
        if (_aktiveJournalUserId is not long userId)
        {
            return;
        }

        _aktiveJournalUserId = null;
        try
        {
            await _journalApiClient.LogoutAsync(userId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Journal-Logout fehlgeschlagen: {ex.Message}");
        }
    }

    /// <summary>
    /// Lädt das aktuelle Bestellung-Flag des Geräts frisch vom Server (anhand der
    /// gespeicherten Seriennummer) und merkt es für den Modus-Picker. Fällt bei
    /// Fehlern auf den zuletzt gespeicherten Wert zurück.
    /// </summary>
    private async Task AktualisiereBestellungFlagAsync()
    {
        var seriennummer = await HandyIdentitaet.GetSeriennummerAsync();
        if (!string.IsNullOrWhiteSpace(seriennummer))
        {
            try
            {
                var handy = await _handyApiClient.ValidiereSeriennummerAsync(seriennummer);
                if (handy is not null)
                {
                    _bestellungErlaubt = handy.Bestellung;
                    await HandyIdentitaet.SpeichereBestellungAsync(handy.Bestellung);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bestellung-Flag konnte nicht geladen werden: {ex.Message}");
            }
        }

        _bestellungErlaubt = await HandyIdentitaet.GetBestellungAsync();
    }

    /// <summary>
    /// Befüllt den Modus-Picker: Index 0 = "Bestellung", danach je Küchenstation ein Eintrag.
    /// Wird ein Termin übergeben, werden nur die diesem Termin zugeordneten Küchenstationen geladen.
    /// </summary>
    private async Task LadeModusAsync(TerminDto? termin = null)
    {
        try
        {
            _kuechenstationen = termin is null
                ? await _kuechenstationApiClient.GetKuechenstationenAsync()
                : await _kuechenstationApiClient.GetKuechenstationenFuerTerminAsync(termin.BewirtungsDatum, termin.VereinId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Küchenstationen konnten nicht geladen werden: {ex.Message}");
            _kuechenstationen = new List<KuechenstationDto>();
        }

        ModusPicker.Items.Clear();
        if (_bestellungErlaubt)
        {
            ModusPicker.Items.Add("Bestellung");
        }
        foreach (var station in _kuechenstationen)
        {
            ModusPicker.Items.Add(station.Name);
        }
        ModusPicker.SelectedIndex = ModusPicker.Items.Count > 0 ? 0 : -1;
    }

    /// <summary>
    /// Erfasst beim ersten Start einmalig die Seriennummer des Geräts, validiert sie
    /// gegen die Tabelle handy und speichert Seriennummer + Name im SecureStorage.
    /// </summary>
    private async Task StelleHandyRegistrierungSicherAsync()
    {
        if (await HandyIdentitaet.IstRegistriertAsync())
        {
            return;
        }

        while (true)
        {
            var eingabe = await DisplayPromptAsync(
                "Geräte-Registrierung",
                "Bitte die Seriennummer dieses Geräts eingeben:",
                accept: "Speichern",
                cancel: "Abbrechen",
                keyboard: Keyboard.Default);

            if (string.IsNullOrWhiteSpace(eingabe))
            {
                await DisplayAlertAsync(
                    "Hinweis",
                    "Ohne gültige Seriennummer kann das Gerät nicht verwendet werden.",
                    "OK");
                continue;
            }

            try
            {
                var handy = await _handyApiClient.ValidiereSeriennummerAsync(eingabe.Trim());
                if (handy is null)
                {
                    await DisplayAlertAsync(
                        "Ungültig",
                        "Diese Seriennummer wurde nicht gefunden. Bitte erneut versuchen.",
                        "OK");
                    continue;
                }

                await HandyIdentitaet.SpeichereAsync(handy.Seriennummer, handy.Name);
                await HandyIdentitaet.SpeichereBestellungAsync(handy.Bestellung);
                _bestellungErlaubt = handy.Bestellung;
                await DisplayAlertAsync(
                    "Registriert",
                    $"Gerät erfolgreich registriert: {handy.Name}",
                    "OK");
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Seriennummer-Validierung fehlgeschlagen: {ex.Message}");
                await DisplayAlertAsync(
                    "Fehler",
                    "Die Seriennummer konnte nicht geprüft werden (Server nicht erreichbar?).",
                    "OK");
                return;
            }
        }
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
            await LadeModusAsync(termin);
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

        // Modus-Picker-Belegung: Wenn Bestellung erlaubt ist, steht sie an Index 0 und
        // die Küchenstationen beginnen bei Index 1. Ist Bestellung nicht erlaubt, beginnen
        // die Küchenstationen bereits bei Index 0.
        var modusIndex = ModusPicker.SelectedIndex;
        var stationIndex = _bestellungErlaubt ? modusIndex - 1 : modusIndex;

        // Login in der journal-Tabelle protokollieren (modus = gewählter Modus-Index).
        try
        {
            await _journalApiClient.LoginAsync(benutzer.UserId, modusIndex, termin.BewirtungsDatum);
            _aktiveJournalUserId = benutzer.UserId;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Journal-Login fehlgeschlagen: {ex.Message}");
        }

        if (stationIndex >= 0 && stationIndex < _kuechenstationen.Count)
        {
            var station = _kuechenstationen[stationIndex];
            await Navigation.PushAsync(new AuslieferungStationSeite(benutzer, termin, station));
        }
        else
        {
            await Navigation.PushAsync(new BestellungSeite(benutzer, termin));
        }
    }

    private void OnAbmeldenClicked(object? sender, EventArgs e)
    {
        // Gemerkte Anmeldung verwerfen und Auswahl zurücksetzen.
        _letzterBewirtungsDatum = null;
        _letzterVereinId = null;
        _letzterUserId = null;

        _ = BeendeJournalSitzungAsync();

        TerminPicker.SelectedIndex = -1;
        BenutzerPicker.ItemsSource = null;
        BenutzerPicker.SelectedIndex = -1;
        ModusPicker.SelectedIndex = 0;
    }
}

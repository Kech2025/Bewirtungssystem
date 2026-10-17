using System.Collections.ObjectModel;
using System.Globalization;
using Bestellung.Data;

namespace Bestellung;

public partial class BestellungSeite : ContentPage
{
    private readonly TischApiClient _tischApiClient = new();
    private readonly SpeisekarteApiClient _speisekarteApiClient = new();
    private readonly BestellungApiClient _bestellungApiClient = new();
    private readonly string _name;
    private readonly long _userId;
    private readonly TerminDto _termin;
    private bool _preislisteGeladen;
    private List<SpeisekartePositionDto> _positionen = new();
    private ObservableCollection<SpeisekarteAnzeigeZeile> _anzeigeZeilen = new();
    private OffeneBestellungDto? _offeneBestellung;
    private static readonly CultureInfo EuroKultur = CultureInfo.GetCultureInfo("de-DE");

    // True, wenn der Teilbetrag manuell im Entry überschrieben wurde.
    private bool _teilbetragUeberschrieben;
    // Verhindert, dass programmatische Änderungen des Entry als manuelle Eingabe gewertet werden.
    private bool _aktualisiereTeilbetragIntern;

    public BestellungSeite(BenutzerDto benutzer, TerminDto termin)
    {
        InitializeComponent();
        _name = benutzer?.Name ?? string.Empty;
        _userId = benutzer?.UserId ?? 0;
        _termin = termin;
        BenutzerLabel.Text = string.IsNullOrWhiteSpace(_name) ? string.Empty : _name;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LadeTischnummernAsync();
    }

    private async Task LadeTischnummernAsync()
    {
        try
        {
            var tische = await _tischApiClient.GetTischeAsync();
            TischPicker.ItemsSource = tische;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Tischnummern konnten nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Tischnummern konnten nicht vom Server geladen werden.", "OK");
        }
    }

    private async void OnTischChanged(object? sender, EventArgs e)
    {
        // Preisliste erst anzeigen, wenn eine Tischnummer ausgewählt wurde.
        if (TischPicker.SelectedItem is null)
        {
            RechnungButton.IsEnabled = false;
            return;
        }

        // Beim Tischwechsel die Rechnungsansicht zurücksetzen und Bestellansicht zeigen.
        ZeigeBestellAnsicht();

        if (_preislisteGeladen)
        {
            PreislisteTitel.IsVisible = true;
            PreislisteView.IsVisible = true;
        }
        else
        {
            await LadePreislisteAsync();
        }

        // Bei Tischwechsel stets eine leere neue Bestellung zeigen (alle Mengen auf 0).
        SetzeAlleMengenZurueck();

        await PruefeOffeneBestellungAsync();
    }

    /// <summary>Setzt die Menge aller Preislistenpositionen auf 0 (leere neue Bestellung).</summary>
    private void SetzeAlleMengenZurueck()
    {
        // Zuerst die Anzeigezeilen (inkl. zugeklappter Varianten) über ihre Setter
        // zurücksetzen, damit die gebundenen Texte und Mengen in der UI aktualisiert werden.
        foreach (var zeile in AlleAnzeigeZeilen())
        {
            zeile.Besonderheit = string.Empty;
            zeile.ZeigeBesonderheit = false;
            zeile.Menge = 0;
            zeile.AktualisiereMengeAusVarianten();
        }

        // Sicherheitshalber auch das zugrundeliegende Datenmodell leeren.
        foreach (var position in _positionen)
        {
            position.Menge = 0;
            position.Besonderheit = string.Empty;
        }

        AktualisiereGesamtpreis();
    }

    /// <summary>Liefert alle Anzeigezeilen inkl. der (ggf. zugeklappten) Untervarianten.</summary>
    private IEnumerable<SpeisekarteAnzeigeZeile> AlleAnzeigeZeilen()
    {
        foreach (var zeile in _anzeigeZeilen)
        {
            yield return zeile;

            if (zeile.Art == ZeilenArt.Kopf)
            {
                foreach (var variante in zeile.Varianten)
                {
                    yield return variante;
                }
            }
        }
    }

    /// <summary>Prüft, ob für den gewählten Tisch ein offener Auftrag (Status 0) existiert.</summary>
    private async Task PruefeOffeneBestellungAsync()
    {
        RechnungButton.IsEnabled = false;
        _offeneBestellung = null;

        if (TischPicker.SelectedItem is not TischDto tisch)
        {
            return;
        }

        var tischNummer = tisch.Tischnummer;

        try
        {
            _offeneBestellung = await _bestellungApiClient.GetOffeneBestellungAsync(
                _termin.BewirtungsDatum, tischNummer);
            RechnungButton.IsEnabled = _offeneBestellung is { Positionen.Count: > 0 };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Offene Bestellung konnte nicht geprüft werden: {ex.Message}");
        }
    }

    /// <summary>Wechselt zur Bestellansicht (Preisliste) und blendet die Rechnung aus.</summary>
    private void ZeigeBestellAnsicht()
    {
        RechnungBereich.IsVisible = false;
        TeilRechnungBereich.IsVisible = false;
        RechnungAufteilenButton.IsVisible = false;
        BestellenButton.IsVisible = true;
    }

    private async Task LadePreislisteAsync()
    {
        try
        {
            var positionen = await _speisekarteApiClient.GetSpeisekarteAsync(
                _termin.BewirtungsDatum, _termin.VereinId);

            _positionen = positionen;
            _anzeigeZeilen = BaueAnzeigeZeilen(positionen);
            PreislisteView.ItemsSource = _anzeigeZeilen;
            AktualisiereGesamtpreis();
            PreislisteTitel.Text = $"Preisliste: {_termin.Anzeige}";
            PreislisteTitel.IsVisible = true;
            PreislisteView.IsVisible = true;
            _preislisteGeladen = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preisliste konnte nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Preisliste konnte nicht vom Server geladen werden.", "OK");
        }
    }

    private void OnMengePlus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is SpeisekarteAnzeigeZeile zeile
            && zeile.Position is { } position && position.Menge < 99)
        {
            zeile.Menge = position.Menge + 1;
            AktualisiereGesamtpreis();
        }
    }

    private void OnMengeMinus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is SpeisekarteAnzeigeZeile zeile
            && zeile.Position is { } position && position.Menge > 0)
        {
            zeile.Menge = position.Menge - 1;
            AktualisiereGesamtpreis();
        }
    }

    private void OnAktionButtonClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not SpeisekarteAnzeigeZeile zeile)
        {
            return;
        }

        // Einzel/Variante: Besonderheit-Eingabezeile ein-/ausblenden.
        if (zeile.Art != ZeilenArt.Kopf)
        {
            zeile.ZeigeBesonderheit = !zeile.ZeigeBesonderheit;
            return;
        }

        var kopf = zeile;
        var index = _anzeigeZeilen.IndexOf(kopf);
        if (index < 0)
        {
            return;
        }

        if (kopf.IstErweitert)
        {
            // Zuklappen: Untervarianten direkt nach dem Kopf entfernen.
            foreach (var variante in kopf.Varianten)
            {
                _anzeigeZeilen.Remove(variante);
            }
            kopf.IstErweitert = false;
        }
        else
        {
            // Aufklappen: Untervarianten direkt nach dem Kopf einfügen.
            for (int i = 0; i < kopf.Varianten.Count; i++)
            {
                _anzeigeZeilen.Insert(index + 1 + i, kopf.Varianten[i]);
            }
            kopf.IstErweitert = true;
        }
    }

    /// <summary>
    /// Gruppiert Positionen nach PreislisteId: Mehrere Positionen mit gleicher
    /// PreislisteId werden zu einem aufklappbaren Gruppenkopf mit Untervarianten.
    /// </summary>
    private static ObservableCollection<SpeisekarteAnzeigeZeile> BaueAnzeigeZeilen(
        List<SpeisekartePositionDto> positionen)
    {
        var zeilen = new ObservableCollection<SpeisekarteAnzeigeZeile>();

        foreach (var gruppe in positionen.GroupBy(p => p.PreislisteId))
        {
            var items = gruppe.ToList();
            if (items.Count == 1)
            {
                var p = items[0];
                zeilen.Add(new SpeisekarteAnzeigeZeile
                {
                    Art = ZeilenArt.Einzel,
                    Position = p,
                    Produkt = p.Produkt,
                    ProduktFarbe = p.ProduktFarbe
                });
            }
            else
            {
                // Der Basispreis der Hauptposition ist bereits serverseitig in p.Preis
                // enthalten (API addiert GlobalerPreis mit ZusatzId == 0).
                var unterpositionen = items;

                var varianten = unterpositionen.Select(p => new SpeisekarteAnzeigeZeile
                {
                    Art = ZeilenArt.Variante,
                    Position = p,
                    Produkt = string.IsNullOrEmpty(p.ZusatzText) ? p.Produkt : p.ZusatzText,
                    ProduktFarbe = p.ProduktFarbe
                }).ToList();

                var kopf = new SpeisekarteAnzeigeZeile
                {
                    Art = ZeilenArt.Kopf,
                    Produkt = items[0].Name,
                    ProduktFarbe = items[0].ProduktFarbe,
                    Varianten = varianten
                };

                // Varianten mit ihrem Kopf verknüpfen (für Mengen-Aggregation).
                foreach (var variante in varianten)
                {
                    variante.Kopf = kopf;
                }

                zeilen.Add(kopf);
            }
        }

        return zeilen;
    }

    private void AktualisiereGesamtpreis()
    {
        var gesamt = _positionen.Sum(p => p.Menge * p.Preis);
        GesamtpreisLabel.Text = gesamt.ToString("C", EuroKultur);
        BestellenButton.IsEnabled = gesamt > 0m;
    }

    private async void OnBestellenClicked(object? sender, EventArgs e)
    {
        if (await SendeAusgewaehlteBestellungAsync() != true)
        {
            return;
        }

        // Tischnummer unverändert lassen und offene Bestellung neu prüfen,
        // damit der Rechnung-Button aktiviert wird.
        await PruefeOffeneBestellungAsync();
    }

    /// <summary>
    /// Schickt die aktuell ausgewählten Positionen (Menge > 0) als Bestellung ab.
    /// Liefert null, wenn nichts ausgewählt ist, true bei Erfolg, false bei Fehler.
    /// Setzt bei Erfolg die Mengen zurück, ändert aber nicht die Tisch-/Ansichtsauswahl.
    /// </summary>
    private async Task<bool?> SendeAusgewaehlteBestellungAsync()
    {
        var gewaehlte = _positionen.Where(p => p.Menge > 0).ToList();
        if (gewaehlte.Count == 0)
        {
            return null;
        }

        if (TischPicker.SelectedItem is not TischDto tisch)
        {
            await DisplayAlertAsync("Hinweis", "Bitte zuerst eine Tischnummer wählen.", "OK");
            return false;
        }

        var tischNummer = tisch.Tischnummer;
        var gesamt = gewaehlte.Sum(p => p.Menge * p.Preis);
        var heute = _termin.BewirtungsDatum;

        var bestellung = new BestellungRequest
        {
            Bewirtungstermin = heute,
            UserId = _userId,
            Gesamtpreis = gesamt,
            TischNummer = tischNummer,
            Positionen = gewaehlte.Select(p => new BestellungPosition
            {
                PreislisteId = p.PreislisteId,
                ZusatzId = p.ZusatzId,
                Preis = p.Preis,
                Menge = p.Menge,
                Besonderheit = p.Besonderheit
            }).ToList()
        };

        try
        {
            BestellenButton.IsEnabled = false;
            var auftragId = await _bestellungApiClient.BestellungSendenAsync(bestellung);
            await DisplayAlertAsync("Bestellung", $"Bestellung Nr. {auftragId} über {gesamt.ToString("C", EuroKultur)} wurde gespeichert.", "OK");

            // Nach erfolgreicher Bestellung alle Mengen der Speisekarte auf 0 setzen.
            SetzeAlleMengenZurueck();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Bestellung konnte nicht gespeichert werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Bestellung konnte nicht an den Server gesendet werden.", "OK");
            AktualisiereGesamtpreis();
            return false;
        }
    }

    /// <summary>
    /// Button "Rechnung": Falls noch Positionen ausgewählt sind, wird zuerst die Bestellung
    /// abgeschickt, damit sie in der Rechnung erscheint. Danach werden die Positionen aller
    /// offenen Aufträge des Tisches flach aufgelistet.
    /// </summary>
    private async void OnRechnungClicked(object? sender, EventArgs e)
    {
        // Noch ausgewählte Positionen zuerst bestellen, damit sie in der Rechnung erscheinen.
        var bestellergebnis = await SendeAusgewaehlteBestellungAsync();
        if (bestellergebnis == false)
        {
            // Bestellung fehlgeschlagen -> Rechnung nicht öffnen.
            return;
        }

        // Nach einer erfolgreichen Nachbestellung die offenen Aufträge neu laden.
        if (bestellergebnis == true)
        {
            await PruefeOffeneBestellungAsync();
        }

        if (_offeneBestellung is null || _offeneBestellung.Positionen.Count == 0)
        {
            return;
        }

        // Für Tische mit Sofortlieferung je Position eine "geliefert"-Checkbox (vorausgewählt) anzeigen.
        var zeigeGeliefert = (TischPicker.SelectedItem as TischDto)?.SofortLieferung == 1;
        foreach (var p in _offeneBestellung.Positionen)
        {
            p.ZeigeGeliefert = zeigeGeliefert;
            p.Geliefert = true;
        }

        // Preisliste/Bestellen ausblenden, Rechnung einblenden.
        PreislisteTitel.IsVisible = false;
        PreislisteView.IsVisible = false;
        BestellenButton.IsVisible = false;

        RechnungView.ItemsSource = _offeneBestellung.Positionen;
        RechnungSummeLabel.Text = _offeneBestellung.Summe.ToString("C", EuroKultur);
        BetragEntry.Text = _offeneBestellung.Summe.ToString("0.00", EuroKultur);
        RechnungBereich.IsVisible = true;

        // Gesamt-Anzeige oben rechts in der Rechnungsansicht ausblenden.
        GesamtTitelLabel.IsVisible = false;
        GesamtpreisLabel.IsVisible = false;

        // Rechnung-Button deaktivieren, solange die Rechnung angezeigt wird.
        RechnungButton.IsEnabled = false;

        // "Rechnung aufteilen" anzeigen, solange der Auftrag nicht vollständig bezahlt ist.
        var positionen = _offeneBestellung.Positionen;
        RechnungAufteilenButton.IsVisible = positionen.Any(p => p.OffeneMenge > 0m);
    }

    /// <summary>
    /// Button "Rechnung aufteilen": zeigt die Positionen mit +/- je Position zur Auswahl der Teilmenge.
    /// </summary>
    private void OnRechnungAufteilenClicked(object? sender, EventArgs e)
    {
        if (_offeneBestellung is null || _offeneBestellung.Positionen.Count == 0)
        {
            return;
        }

        // Teilmengen zurücksetzen.
        foreach (var p in _offeneBestellung.Positionen)
        {
            p.TeilMenge = 0m;
        }

        _teilbetragUeberschrieben = false;
        RechnungBereich.IsVisible = false;
        RechnungAufteilenButton.IsVisible = false;
        TeilRechnungBereich.IsVisible = true;
        ZeigeTeilrechnungPositionen();
    }

    /// <summary>
    /// Weist der Teilrechnungs-CollectionView die offenen Positionen neu zu. Das Leeren und
    /// verzögerte Neuzuweisen erzwingt einen vollständigen Zellen-Neuaufbau, damit die
    /// "geliefert"-Checkbox (gebunden an ZeigeGeliefert) korrekt angezeigt wird.
    /// </summary>
    private void ZeigeTeilrechnungPositionen()
    {
        var positionen = ErmittleOffeneTeilrechnungPositionen();
        TeilRechnungView.ItemsSource = null;
        Dispatcher.Dispatch(() =>
        {
            TeilRechnungView.ItemsSource = positionen;
            AktualisiereTeilbetrag();
        });
    }

    /// <summary>
    /// Liefert nur die noch offenen (nicht vollständig bezahlten) Positionen für die Teilrechnung
    /// und setzt dabei die "geliefert"-Checkbox (nur für Tische mit Sofortlieferung).
    /// </summary>
    private List<OffeneBestellungPosition> ErmittleOffeneTeilrechnungPositionen()
    {
        var offene = _offeneBestellung?.Positionen.Where(p => p.OffeneMenge > 0m).ToList()
            ?? new List<OffeneBestellungPosition>();

        var zeigeGeliefert = (TischPicker.SelectedItem as TischDto)?.SofortLieferung == 1;
        foreach (var p in offene)
        {
            p.ZeigeGeliefert = zeigeGeliefert;
            p.Geliefert = true;
        }

        return offene;
    }

    private void OnTeilMengePlus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is OffeneBestellungPosition pos
            && pos.TeilMenge < pos.OffeneMenge)
        {
            pos.TeilMenge += 1m;
            AktualisiereTeilbetrag();
        }
    }

    private void OnTeilMengeMinus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is OffeneBestellungPosition pos
            && pos.TeilMenge > 0m)
        {
            pos.TeilMenge -= 1m;
            AktualisiereTeilbetrag();
        }
    }

    /// <summary>Berechnet den Teilbetrag aus den gewählten Teilmengen.</summary>
    private void AktualisiereTeilbetrag()
    {
        // Bei manuell überschriebenem Betrag die berechnete Summe nicht erzwingen.
        if (_teilbetragUeberschrieben)
        {
            return;
        }

        var betrag = _offeneBestellung?.Positionen.Sum(p => p.TeilMenge * p.Preis) ?? 0m;
        _aktualisiereTeilbetragIntern = true;
        TeilRechnungSummeEntry.Text = betrag.ToString("N2", EuroKultur);
        _aktualisiereTeilbetragIntern = false;
    }

    private void OnTeilbetragTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Programmatische Aktualisierungen ignorieren; nur echte Benutzereingaben markieren.
        if (_aktualisiereTeilbetragIntern)
        {
            return;
        }

        _teilbetragUeberschrieben = true;
    }

    /// <summary>Teil bezahlen: bucht die gewählten Teilmengen und schließt voll bezahlte Aufträge ab.</summary>
    private async void OnTeilBezahlenClicked(object? sender, EventArgs e)
    {
        if (_offeneBestellung is null)
        {
            return;
        }

        var zuZahlen = _offeneBestellung.Positionen.Where(p => p.TeilMenge > 0m).ToList();
        if (zuZahlen.Count == 0)
        {
            await DisplayAlertAsync("Hinweis", "Bitte mindestens eine Menge wählen.", "OK");
            return;
        }

        var berechneterBetrag = zuZahlen.Sum(p => p.TeilMenge * p.Preis);

        // Falls der Teilbetrag manuell überschrieben wurde, diesen Wert verwenden.
        var betrag = berechneterBetrag;
        if (_teilbetragUeberschrieben)
        {
            if (!decimal.TryParse(TeilRechnungSummeEntry.Text, NumberStyles.Number, EuroKultur, out betrag)
                || betrag <= 0m)
            {
                await DisplayAlertAsync("Hinweis", "Bitte einen gültigen Teilbetrag eingeben.", "OK");
                return;
            }
        }

        try
        {
            TeilBezahlenButton.IsEnabled = false;

            // Für Tische > 100: unter den bezahlten Positionen die als geliefert markierten
            // sofort als geliefert verbuchen (MengeGeliefert = Menge; StatusLieferung = 1, wenn vollständig).
            var gelieferte = zuZahlen
                .Where(p => p.ZeigeGeliefert && p.Geliefert)
                .ToList();
            if (gelieferte.Count > 0)
            {
                await _bestellungApiClient.GeliefertSetzenAsync(gelieferte);
            }

            await _bestellungApiClient.TeilzahlenAsync(betrag, zuZahlen);
            await DisplayAlertAsync("Teilrechnung",
                $"Teilbetrag {betrag.ToString("C", EuroKultur)} wurde verbucht.", "OK");

            // Offene Bestellung neu laden (aktualisiert offene/bezahlte Mengen).
            await PruefeOffeneBestellungAsync();

            if (_offeneBestellung is { Positionen.Count: > 0 }
                && _offeneBestellung.Positionen.Any(p => p.OffeneMenge > 0m))
            {
                // Im Teilzahlungsmodus bleiben: nur noch offene Positionen anzeigen, Teilmengen auf 0.
                foreach (var p in _offeneBestellung.Positionen)
                {
                    p.TeilMenge = 0m;
                }
                _teilbetragUeberschrieben = false;
                TeilRechnungBereich.IsVisible = true;
                ZeigeTeilrechnungPositionen();
            }
            else
            {
                // Alles bezahlt -> zurück zur Bestellansicht.
                TeilRechnungBereich.IsVisible = false;
                ZeigeBestellAnsicht();
                GesamtTitelLabel.IsVisible = true;
                GesamtpreisLabel.IsVisible = true;
                TischPicker.SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Teilzahlung fehlgeschlagen: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Teilrechnung konnte nicht verbucht werden.", "OK");
        }
        finally
        {
            TeilBezahlenButton.IsEnabled = true;
        }
    }

    /// <summary>Abbrechen der Teilrechnung: zurück zur normalen Rechnungsansicht.</summary>
    private void OnTeilAbbrechenClicked(object? sender, EventArgs e)
    {
        TeilRechnungBereich.IsVisible = false;
        OnRechnungClicked(this, EventArgs.Empty);
    }

    /// <summary>Bezahlen-Button: schließt alle offenen Aufträge des Tisches ab.</summary>
    private async void OnBezahlenClicked(object? sender, EventArgs e)
    {
        await AbschliessenAsync();
    }

    /// <summary>Abbrechen: zurück zur Bestellansicht (Preisliste), ohne zu bezahlen.</summary>
    private void OnAbbrechenClicked(object? sender, EventArgs e)
    {
        ZeigeBestellAnsicht();

        // Gesamt-Anzeige oben rechts wieder einblenden.
        GesamtTitelLabel.IsVisible = true;
        GesamtpreisLabel.IsVisible = true;

        // Rechnung-Button wieder aktivieren, da ein offener Auftrag weiterhin existiert.
        RechnungButton.IsEnabled = _offeneBestellung is { Positionen.Count: > 0 };

        if (_preislisteGeladen)
        {
            PreislisteTitel.IsVisible = true;
            PreislisteView.IsVisible = true;
        }
    }

    /// <summary>Schließt den Auftrag mit Status = 1 und dem eingegebenen Betrag ab.</summary>
    private async Task AbschliessenAsync()
    {
        if (_offeneBestellung is null)
        {
            return;
        }

        if (!decimal.TryParse(BetragEntry.Text, System.Globalization.NumberStyles.Number, EuroKultur, out var betrag)
            || betrag < 0m)
        {
            await DisplayAlertAsync("Hinweis", "Bitte einen gültigen Betrag eingeben.", "OK");
            return;
        }

        try
        {
            RechnungButton.IsEnabled = false;
            BezahlenButton.IsEnabled = false;

            // Für Tische > 99: markierte Positionen sofort als geliefert verbuchen
            // (MengeGeliefert = Menge; StatusLieferung = 1, wenn der Auftrag vollständig geliefert ist).
            var gelieferte = _offeneBestellung.Positionen
                .Where(p => p.ZeigeGeliefert && p.Geliefert)
                .ToList();
            if (gelieferte.Count > 0)
            {
                await _bestellungApiClient.GeliefertSetzenAsync(gelieferte);
            }

            await _bestellungApiClient.BezahlenAsync(_offeneBestellung.AlleAuftragIds, betrag);
            await DisplayAlertAsync("Rechnung",
                $"{_offeneBestellung.AlleAuftragIds.Count} Auftrag/Aufträge wurden mit {betrag.ToString("C", EuroKultur)} abgeschlossen.",
                "OK");

            // Zurück zur Tischauswahl.
            _offeneBestellung = null;
            RechnungBereich.IsVisible = false;
            RechnungAufteilenButton.IsVisible = false;
            BestellenButton.IsVisible = true;
            GesamtTitelLabel.IsVisible = true;
            GesamtpreisLabel.IsVisible = true;
            RechnungButton.IsEnabled = false;
            BezahlenButton.IsEnabled = true;
            TischPicker.SelectedItem = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Bezahlen fehlgeschlagen: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Rechnung konnte nicht abgeschlossen werden.", "OK");
            RechnungButton.IsEnabled = true;
            BezahlenButton.IsEnabled = true;
        }
    }
}

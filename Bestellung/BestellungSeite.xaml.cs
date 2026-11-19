using Bestellung.Data;
using QRCoder;
using System.Collections.ObjectModel;
using System.Globalization;
//using static Android.Icu.Text.Transliterator;

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

    // Aktuell angezeigte (je Speisekartenposition zusammengefasste) Rechnungszeilen.
    private List<OffeneBestellungPosition> _rechnungsZeilen = new();
    private static readonly CultureInfo EuroKultur = CultureInfo.GetCultureInfo("de-DE");

    // Aktuell geladene Tische und der ausgewählte Tisch (ersetzt den früheren Picker).
    private List<TischDto> _tische = new();
    private TischDto? _gewaehlterTisch;

    // True, wenn der Teilbetrag manuell im Entry überschrieben wurde.
    private bool _teilbetragUeberschrieben;
    // Verhindert, dass programmatische Änderungen des Entry als manuelle Eingabe gewertet werden.
    private bool _aktualisiereTeilbetragIntern;

    // Änderungsmodus: Mengenfelder zeigen die noch nicht berechneten Mengen; beim Bestellen
    // wird nur die Differenz zur Ausgangsmenge (Basis) gespeichert.
    private bool _aenderungsModus;
    private readonly Dictionary<SpeisekartePositionDto, (int Menge, string Besonderheit)> _aenderungsBasis = new();

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
            _tische = await _tischApiClient.GetTischeAsync(_termin.BewirtungsDatum);
            TischAuswahlView.ItemsSource = _tische;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Tischnummern konnten nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Tischnummern konnten nicht vom Server geladen werden.", "OK");
        }
    }

    /// <summary>Öffnet das Overlay zur Tischauswahl (Tische mit offener Rechnung erscheinen rot).</summary>
    private async void OnTischAuswahlOeffnen(object? sender, EventArgs e)
    {
        // Beim Öffnen die aktuellen Tische inkl. Offene-Rechnung-Status neu laden.
        await LadeTischnummernAsync();
        TischAuswahlOverlay.IsVisible = true;
    }

    private void OnTischAuswahlAbbrechen(object? sender, EventArgs e)
    {
        TischAuswahlOverlay.IsVisible = false;
    }

    private async void OnTischAusgewaehlt(object? sender, EventArgs e)
    {
        if ((sender as Element)?.BindingContext is not TischDto tisch)
        {
            return;
        }

        TischAuswahlOverlay.IsVisible = false;
        _gewaehlterTisch = tisch;
        TischAuswahlButton.Text = tisch.Anzeige;
        TischAuswahlButton.TextColor = Colors.White;

        await TischGewechseltAsync();
    }

    private async Task TischGewechseltAsync()
    {
        if (_gewaehlterTisch is null)
        {
            RechnungButton.IsEnabled = false;
            return;
        }

        // Beim Tischwechsel die Rechnungsansicht zurücksetzen und Bestellansicht zeigen.
        ZeigeBestellAnsicht();

        if (_preislisteGeladen)
        {
            ZeigePreisliste();
        }
        else
        {
            await LadePreislisteAsync();
        }

        // Bei Tischwechsel stets eine leere neue Bestellung zeigen (alle Mengen auf 0).
        BeendeAenderungsModus();
        SetzeAlleMengenZurueck();

        // Alle aufgeklappten Positionen wieder einklappen (nur oberste Ebene anzeigen).
        KlappeAlleKoepfeEin();

        await PruefeOffeneBestellungAsync();
    }

    /// <summary>Setzt die Tischauswahl zurück (kein Tisch gewählt).</summary>
    private void SetzeTischAuswahlZurueck()
    {
        _gewaehlterTisch = null;
        TischAuswahlButton.Text = "Tischnummer wählen";
        TischAuswahlButton.TextColor = Colors.White;
        ZeigePreisliste();
    }

    /// <summary>Zeigt die Speisekarte nur an, wenn ein Tisch gewählt ist.</summary>
    private void ZeigePreisliste()
    {
        var sichtbar = _gewaehlterTisch is not null;
        PreislisteTitel.IsVisible = sichtbar;
        PreislisteView.IsVisible = sichtbar;
    }
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

    /// <summary>Klappt alle erweiterten Gruppenköpfe wieder ein (nur oberste Ebene sichtbar).</summary>
    private void KlappeAlleKoepfeEin()
    {
        foreach (var kopf in _anzeigeZeilen.Where(z => z.Art == ZeilenArt.Kopf && z.IstErweitert).ToList())
        {
            foreach (var variante in kopf.Varianten)
            {
                _anzeigeZeilen.Remove(variante);
            }
            kopf.IstErweitert = false;
        }
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

        if (_gewaehlterTisch is not TischDto tisch)
        {
            return;
        }

        var tischNummer = tisch.Tischnummer;

        try
        {
            _offeneBestellung = await _bestellungApiClient.GetOffeneBestellungAsync(_termin.BewirtungsDatum, tischNummer);

            bool offeneRechnung = false;
            var tische = await _tischApiClient.GetTischeAsync(_termin.BewirtungsDatum);
            foreach (var t in tische)
            {
                if (t.Tischnummer == tischNummer)
                {
                     offeneRechnung = t.OffeneRechnung;
                    break;
                }
            }
            RechnungButton.IsEnabled = offeneRechnung;
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
        AendernButton.IsVisible = true;
        BestellenButton.IsVisible = true;

        // Tischauswahl wieder aktivieren
        TischAuswahlButton.IsEnabled = true;
        TischAuswahlButton.ClearValue(VisualElement.BackgroundColorProperty);
    }

    /// <summary>Deaktiviert den Tischauswahl-Button (grau), solange die Rechnung angezeigt wird.</summary>
    private void SperreTischAuswahl()
    {
        TischAuswahlButton.IsEnabled = false;
        TischAuswahlButton.BackgroundColor = Colors.LightGray;
    }

    private async Task LadePreislisteAsync()
    {
        try
        {
            var positionen = await _speisekarteApiClient.GetSpeisekarteAsync(_termin.BewirtungsDatum, _termin.VereinId);

            _positionen = positionen;
            _anzeigeZeilen = BaueAnzeigeZeilen(positionen);
            PreislisteView.ItemsSource = _anzeigeZeilen;
            AktualisiereGesamtpreis();
            PreislisteTitel.Text = $"Preisliste: {_termin.Anzeige}";
            ZeigePreisliste();
            _preislisteGeladen = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preisliste konnte nicht geladen werden: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Preisliste konnte nicht vom Server geladen werden.", "OK");
        }
    }

    /// <summary>Basismenge (bereits offen bestellt) einer Position im Änderungsmodus, sonst 0.</summary>
    private int BasisMenge(SpeisekartePositionDto position) =>
        _aenderungsModus && _aenderungsBasis.TryGetValue(position, out var b) ? b.Menge : 0;

    /// <summary>Im Änderungsmodus die eigenen Änderungszeilen, sonst die Speisekarte.</summary>
    private IEnumerable<SpeisekartePositionDto> AktivePositionen() =>
        _aenderungsModus ? _aenderungsBasis.Keys : _positionen;

    /// <summary>
    /// Noch verfügbare Menge einer limitierten Position. Im Änderungsmodus ist die
    /// Basismenge bereits in MengeFrei abgezogen und wird daher wieder hinzugerechnet.
    /// </summary>
    private int Verfuegbar(SpeisekartePositionDto position) =>
        position.MengeFrei + BasisMenge(position) - position.Menge;

    private void AktuelisiereMengeLimit(SpeisekarteAnzeigeZeile zeile)
    {
        if (zeile.Position is { } position)
        {
            if (position.Limit == -1)
            {
                zeile.PreisText = position.PreisText;
                zeile.PlusAktiv = true;
            }
            else
            {
                zeile.PreisText = position.PreisText + Environment.NewLine + " Noch verfügbar:" + Verfuegbar(position).ToString();
                zeile.PlusAktiv = Verfuegbar(position) > 0;
            }
        }
    }
    private void OnMengePlus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is SpeisekarteAnzeigeZeile zeile
            && zeile.Position is { } position && position.Menge < 99
            && (position.Limit == -1 || Verfuegbar(position) > 0))
        {
            zeile.Menge = position.Menge + 1;
            AktualisiereGesamtpreis();
            AktuelisiereMengeLimit(zeile);
        }
    }

    private void OnMengeMinus(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is SpeisekarteAnzeigeZeile zeile
            && zeile.Position is { } position && position.Menge > 0)
        {
            zeile.Menge = position.Menge - 1;
            AktualisiereGesamtpreis();
            AktuelisiereMengeLimit(zeile);
        }
    }

    /// <summary>
    /// Button "Ändern": zeigt in jeder Position die Summe der noch nicht berechneten Mengen
    /// des Tisches. Beim Bestellen wird nur die Differenz (plus oder minus) gespeichert.
    /// </summary>
    private async void OnAendernClicked(object? sender, EventArgs e)
    {
        await PruefeOffeneBestellungAsync();
        if (_offeneBestellung is null || _offeneBestellung.Positionen.Count == 0)
        {
            return;
        }

        if (!_preislisteGeladen)
        {
            await LadePreislisteAsync();
        }

        ZeigeBestellAnsicht();
        ZeigePreisliste();
        GesamtTitelLabel.IsVisible = true;
        GesamtpreisLabel.IsVisible = true;

        SetzeAlleMengenZurueck();

        _aenderungsBasis.Clear();
        var aenderungsZeilen = new ObservableCollection<SpeisekarteAnzeigeZeile>();
        // Je Speisekartenposition UND Besonderheit eine eigene Zeile (z. B. Kuchen "Schwarzwälder" / "Erdbeer").
        foreach (var gruppe in _offeneBestellung.Positionen
                     .Where(p => p.OffeneMenge != 0m)
                     .GroupBy(p => (p.PreislisteId, p.ZusatzId, Besonderheit: (p.Besonderheit ?? string.Empty).Trim())))
        {
            var offen = (int)gruppe.Sum(p => p.OffeneMenge);
            var vorlage = _positionen.FirstOrDefault(p =>
                p.PreislisteId == gruppe.Key.PreislisteId && p.ZusatzId == gruppe.Key.ZusatzId);
            if (offen <= 0 || vorlage is null)
            {
                continue;
            }

            var position = new SpeisekartePositionDto
            {
                PreislisteId = vorlage.PreislisteId,
                ZusatzId = vorlage.ZusatzId,
                Name = vorlage.Name,
                ZusatzText = vorlage.ZusatzText,
                Produkt = vorlage.Produkt,
                KategorieText = vorlage.KategorieText,
                Preiskategorie = vorlage.Preiskategorie,
                Preis = vorlage.Preis,
                Limit = vorlage.Limit,
                MengeFrei = vorlage.MengeFrei,
                Besonderheit = gruppe.Key.Besonderheit,
                Menge = offen
            };
            _aenderungsBasis[position] = (offen, gruppe.Key.Besonderheit);

            var produkt = string.IsNullOrEmpty(vorlage.ZusatzText) || vorlage.Produkt.Contains(vorlage.ZusatzText)
                ? vorlage.Produkt
                : $"{vorlage.Produkt} – {vorlage.ZusatzText}";
            aenderungsZeilen.Add(new SpeisekarteAnzeigeZeile
            {
                Art = ZeilenArt.Einzel,
                Position = position,
                Produkt = produkt,
                ProduktFarbe = vorlage.ProduktFarbe,
                PreisText = vorlage.Limit == -1 ? vorlage.PreisText : vorlage.PreisText + Environment.NewLine + " Noch verfügbar:" + vorlage.MengeFrei.ToString(),
                MengeFarbe = Colors.Red,
                ZeigeBesonderheit = gruppe.Key.Besonderheit.Length > 0
            });
        }
        _aenderungsModus = true;

        // Im Änderungsmodus Rechnung/Ändern sperren (Ändern folgt RechnungButton.IsEnabled)
        // und den Modus am Tisch-Button anzeigen.
        RechnungButton.IsEnabled = false;
        if (_gewaehlterTisch is { } tisch)
        {
            TischAuswahlButton.Text = $"{tisch.Anzeige} (Ändern-Modus)";
        }

        // Im Änderungsmodus nur die noch nicht berechneten Positionen (wie im Lieferbildschirm) anzeigen.
        PreislisteView.ItemsSource = aenderungsZeilen;

        AktualisiereGesamtpreis();
    }

    /// <summary>Verlässt den Änderungsmodus und setzt die Limit-Anzeige zurück.</summary>
    private void BeendeAenderungsModus()
    {
        if (!_aenderungsModus)
        {
            return;
        }
        _aenderungsModus = false;
        _aenderungsBasis.Clear();
        PreislisteView.ItemsSource = _anzeigeZeilen;
        foreach (var zeile in AlleAnzeigeZeilen())
        {
            zeile.MengeFarbe = Colors.Black;
        }
        if (_gewaehlterTisch is { } tisch)
        {
            TischAuswahlButton.Text = tisch.Anzeige;
        }
        foreach (var zeile in AlleAnzeigeZeilen())
        {
            //AktuelisiereMengeLimit(zeile);
        }
    }

    private string BasisBesonderheit(SpeisekartePositionDto position) =>
        _aenderungsBasis.TryGetValue(position, out var b) ? b.Besonderheit : string.Empty;

    /// <summary>Im Änderungsmodus: wurde die Besonderheit gegenüber der Bestellung geändert?</summary>
    private bool BesonderheitGeaendert(SpeisekartePositionDto position) =>
        _aenderungsModus
        && _aenderungsBasis.TryGetValue(position, out var b)
        && (position.Besonderheit ?? string.Empty).Trim() != b.Besonderheit;

    /// <summary>Zu bestellende Menge: im Änderungsmodus die Differenz zur Basis, sonst die Menge.</summary>
    private int BestellMenge(SpeisekartePositionDto position) =>
        position.Menge - BasisMenge(position);

    private async void OnAktionButtonClicked(object? sender, EventArgs e)
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

            // Liste so verschieben, dass die neu eingefügten Untervarianten sichtbar werden
            // (wichtig, wenn der Kopf am unteren Listenende angeklickt wird).
            if (kopf.Varianten.Count > 0)
            {
                var letzteVariante = kopf.Varianten[^1];
                // Kurze Verzögerung, damit die CollectionView die neuen Items gerendert hat.
                await Task.Delay(50);
                try
                {
                    PreislisteView.ScrollTo(letzteVariante, position: ScrollToPosition.MakeVisible, animate: true);
                }
                catch
                {
                    // ScrollTo kann fehlschlagen, wenn das Element (noch) nicht realisiert ist – ignorieren.
                }
            }
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
                    ProduktFarbe = p.ProduktFarbe,
                    PreisText = p.Limit == -1 ? p.PreisText : p.PreisText + Environment.NewLine + " Noch verfügbar:" + p.MengeFrei.ToString() 
                });
                if (p.Limit != -1 && p.MengeFrei - p.Menge <= 0)
                {
                    //MengePlus-Button inaktiv setzen
                    zeilen[zeilen.Count -1].PlusAktiv = false;
                }
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
                    if(variante.Position is { } p)
                    {
                        if(p.Limit == -1)
                        {
                            variante.PreisText = p.PreisText;
                        }
                        else
                        {
                            variante.PreisText = p.PreisText + Environment.NewLine + " Noch verfügbar:" + p.MengeFrei.ToString();
                            if (p.Limit != -1 && p.MengeFrei - p.Menge <= 0)
                            {
                                //MengePlus-Button inaktiv setzen
                                variante.PlusAktiv = false;
                            }
                        }
                    }
                }

                zeilen.Add(kopf);
            }
        }

        return zeilen;
    }

    private void OnBesonderheitTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_aenderungsModus)
        {
            AktualisiereGesamtpreis();
        }
    }

    private void AktualisiereGesamtpreis()
    {
        var gesamt = AktivePositionen().Sum(p => p.Menge * p.Preis);
        GesamtpreisLabel.Text = gesamt.ToString("C", EuroKultur);
        BestellenButton.IsEnabled = _aenderungsModus
            ? _aenderungsBasis.Keys.Any(p => BestellMenge(p) != 0 || BesonderheitGeaendert(p))
            : gesamt > 0m;
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
        var gewaehlte = AktivePositionen()
            .Select(p => (Position: p, Menge: BestellMenge(p)))
            .Where(x => x.Menge != 0 || BesonderheitGeaendert(x.Position))
            .ToList();
        if (gewaehlte.Count == 0)
        {
            return null;
        }

        if (_gewaehlterTisch is not TischDto tisch)
        {
            await DisplayAlertAsync("Hinweis", "Bitte zuerst eine Tischnummer wählen.", "OK");
            return false;
        }

        var tischNummer = tisch.Tischnummer;
        var gesamt = gewaehlte.Sum(x => x.Menge * x.Position.Preis);
        var heute = _termin.BewirtungsDatum;
        var warAenderung = _aenderungsModus;

        var bestellung = new BestellungRequest
        {
            Bewirtungstermin = heute,
            UserId = _userId,
            Gesamtpreis = gesamt,
            TischNummer = tischNummer,
            Aenderung = warAenderung,
            Positionen = gewaehlte.Select(x => new BestellungPosition
            {
                PreislisteId = x.Position.PreislisteId,
                ZusatzId = x.Position.ZusatzId,
                Preis = x.Position.Preis,
                Menge = x.Menge,
                Besonderheit = x.Position.Besonderheit,
                OriginalBesonderheit = _aenderungsModus ? BasisBesonderheit(x.Position) : null
            }).ToList()
        };

        try
        {
            BestellenButton.IsEnabled = false;
            var auftragId = await _bestellungApiClient.BestellungSendenAsync(bestellung);
            await DisplayAlertAsync("Bestellung", $"Bestellung Nr. {auftragId} über {gesamt.ToString("C", EuroKultur)} wurde gespeichert.", "OK");

            // Nach erfolgreicher Bestellung alle Mengen der Speisekarte auf 0 setzen.
            BeendeAenderungsModus();
            SetzeAlleMengenZurueck();

            // Nach einer Änderung die Preisliste neu laden, damit die Limits stimmen.
            if (warAenderung)
            {
                await LadePreislisteAsync();
            }
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

        // Ungespeicherte Änderungsansicht verlassen.
        if (_aenderungsModus)
        {
            BeendeAenderungsModus();
            SetzeAlleMengenZurueck();
        }

        // Minus-Positionen aus Änderungen mit den offenen Positionen desselben Produkts verrechnen.
        VerrechneMinusPositionen(_offeneBestellung.Positionen);

        // Nur noch nicht (vollständig) bezahlte Positionen in der Rechnung anzeigen;
        // gleiche Speisekartenpositionen werden zu einer Zeile zusammengefasst.
        var offenePositionen = FasseZusammen(_offeneBestellung.Positionen);
        _rechnungsZeilen = offenePositionen;
        if (offenePositionen.Count == 0)
        {
            return;
        }

        // Für Tische mit Sofortlieferung je Position eine "geliefert"-Checkbox (vorausgewählt) anzeigen.
        var zeigeGeliefert = _gewaehlterTisch?.SofortLieferung == 1;
        foreach (var p in offenePositionen)
        {
            p.ZeigeGeliefert = zeigeGeliefert;
            p.Geliefert = true;
        }

        // Preisliste/Bestellen ausblenden, Rechnung einblenden.
        PreislisteTitel.IsVisible = false;
        PreislisteView.IsVisible = false;
        BestellenButton.IsVisible = false;
        AendernButton.IsVisible = false;

        var offeneSumme = offenePositionen.Sum(p => p.OffeneMenge * p.Preis);
        RechnungView.ItemsSource = offenePositionen;
        RechnungSummeLabel.Text = offeneSumme.ToString("C", EuroKultur);
        BetragEntry.Text = offeneSumme.ToString("0.00", EuroKultur);
        RechnungBereich.IsVisible = true;

        // Gesamt-Anzeige oben rechts in der Rechnungsansicht ausblenden.
        GesamtTitelLabel.IsVisible = false;
        GesamtpreisLabel.IsVisible = false;

        // Rechnung-Button deaktivieren, solange die Rechnung angezeigt wird.
        RechnungButton.IsEnabled = false;

        // Tischauswahl sperren (grau), solange die Rechnung erstellt wird.
        SperreTischAuswahl();

        // "Rechnung aufteilen" anzeigen, solange der Auftrag nicht vollständig bezahlt ist.
        // "Rechnung aufteilen" nur anzeigen, wenn mehr als eine Einheit offen ist
        // (bei nur noch einer Position mit Menge 1 gibt es nichts aufzuteilen).
        RechnungAufteilenButton.IsVisible = offenePositionen.Sum(p => p.OffeneMenge) > 1m;
    }

    /// <summary>
    /// Verrechnet negative offene Mengen (Minus-Positionen aus "Ändern") lokal mit den
    /// positiven offenen Positionen desselben Produkts, damit die Rechnung nur Nettomengen zeigt.
    /// </summary>
    private static void VerrechneMinusPositionen(List<OffeneBestellungPosition> positionen)
    {
        foreach (var gruppe in positionen.GroupBy(p => (p.PreislisteId, p.ZusatzId)))
        {
            var minus = -gruppe.Where(p => p.OffeneMenge < 0m).Sum(p => p.OffeneMenge);
            if (minus <= 0m)
            {
                continue;
            }

            foreach (var neg in gruppe.Where(p => p.OffeneMenge < 0m))
            {
                neg.MengeBezahlt = neg.Menge;
            }

            foreach (var pos in gruppe.Where(p => p.OffeneMenge > 0m))
            {
                var abzug = Math.Min(minus, pos.OffeneMenge);
                pos.MengeBezahlt += abzug;
                minus -= abzug;
                if (minus <= 0m)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Fasst gleiche Speisekartenpositionen (PreislisteId, ZusatzId, Preis) zu einer Rechnungszeile
    /// zusammen. Die Einzelpositionen bleiben in <see cref="OffeneBestellungPosition.Teile"/> erhalten.
    /// </summary>
    private static List<OffeneBestellungPosition> FasseZusammen(List<OffeneBestellungPosition> positionen)
    {
        VerrechneMinusPositionen(positionen);

        return positionen
            .GroupBy(p => (p.PreislisteId, p.ZusatzId, p.Preis))
            .Select(g => new OffeneBestellungPosition
            {
                PreislisteId = g.Key.PreislisteId,
                ZusatzId = g.Key.ZusatzId,
                Preis = g.Key.Preis,
                Produkt = g.First().Produkt,
                Menge = g.Sum(p => p.Menge),
                MengeBezahlt = g.Sum(p => p.MengeBezahlt),
                AuftragId = g.First().AuftragId,
                PosNr = g.First().PosNr,
                Teile = g.ToList()
            })
            .Where(z => z.OffeneMenge > 0m)
            .ToList();
    }

    /// <summary>
    /// Verteilt die gewählten Teilmengen der zusammengefassten Zeilen auf die offenen
    /// Einzelpositionen (älteste zuerst) und liefert diese für die API zurück.
    /// </summary>
    private static List<OffeneBestellungPosition> VerteileAufTeile(IEnumerable<OffeneBestellungPosition> zeilen)
    {
        var ergebnis = new List<OffeneBestellungPosition>();
        foreach (var zeile in zeilen)
        {
            var rest = zeile.TeilMenge;
            foreach (var teil in zeile.Teile.Where(t => t.OffeneMenge > 0m))
            {
                if (rest <= 0m)
                {
                    break;
                }
                teil.TeilMenge = Math.Min(rest, teil.OffeneMenge);
                teil.ZeigeGeliefert = zeile.ZeigeGeliefert;
                teil.Geliefert = zeile.Geliefert;
                rest -= teil.TeilMenge;
                ergebnis.Add(teil);
            }
        }
        return ergebnis;
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
        var offene = _offeneBestellung is null
            ? new List<OffeneBestellungPosition>()
            : FasseZusammen(_offeneBestellung.Positionen);
        _rechnungsZeilen = offene;

        var zeigeGeliefert = _gewaehlterTisch?.SofortLieferung == 1;
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

        var betrag = _rechnungsZeilen.Sum(p => p.TeilMenge * p.Preis);
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

        var zuZahlenZeilen = _rechnungsZeilen.Where(p => p.TeilMenge > 0m).ToList();
        if (zuZahlenZeilen.Count == 0)
        {
            await DisplayAlertAsync("Hinweis", "Bitte mindestens eine Menge wählen.", "OK");
            return;
        }

        var berechneterBetrag = zuZahlenZeilen.Sum(p => p.TeilMenge * p.Preis);
        var zuZahlen = VerteileAufTeile(zuZahlenZeilen);

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

            var handyName = await HandyIdentitaet.GetNameAsync();
            var ergebnis = await _bestellungApiClient.TeilzahlenAsync(betrag, zuZahlen, handyName);

            // Offene Bestellung neu laden (aktualisiert offene/bezahlte Mengen).
            await PruefeOffeneBestellungAsync();

            if (_offeneBestellung is { Positionen.Count: > 0 }
                && FasseZusammen(_offeneBestellung.Positionen).Count > 0)
            {
                // Im Teilzahlungsmodus bleiben: nur noch offene Positionen anzeigen, Teilmengen auf 0.
                foreach (var p in _offeneBestellung.Positionen)
                {
                    p.TeilMenge = 0m;
                }
                _teilbetragUeberschrieben = false;
                TeilRechnungBereich.IsVisible = true;
                // Im Teilzahlungsmodus bleibt der Rechnung-Button (und damit Ändern) gesperrt.
                RechnungButton.IsEnabled = false;
                ZeigeTeilrechnungPositionen();
            }
            else
            {
                // Alles bezahlt -> zurück zur Tischauswahl.
                _offeneBestellung = null;
                _rechnungsZeilen = new();
                TeilRechnungBereich.IsVisible = false;
                ZeigeBestellAnsicht();
                SetzeAlleMengenZurueck();
                if (_preislisteGeladen)
                {
                    ZeigePreisliste();
                }
                GesamtTitelLabel.IsVisible = true;
                GesamtpreisLabel.IsVisible = true;
                RechnungButton.IsEnabled = false;
                SetzeTischAuswahlZurueck();
            }

            // Fiskal-QR-Code des Teilzahlungsbelegs
            ZeigeFiskalBeleg(ergebnis);
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

    /// <summary>"Später bezahlen": Teilrechnung verlassen und zurück zur Bestellansicht.</summary>
    private void OnTeilAbbrechenClicked(object? sender, EventArgs e)
    {
        TeilRechnungBereich.IsVisible = false;
        ZeigeBestellAnsicht();

        // Gesamt-Anzeige oben rechts wieder einblenden.
        GesamtTitelLabel.IsVisible = true;
        GesamtpreisLabel.IsVisible = true;

        // Rechnung-Button wieder aktivieren, da ggf. noch offene Positionen existieren.
        RechnungButton.IsEnabled = _offeneBestellung is { Positionen.Count: > 0 };

        if (_preislisteGeladen)
        {
            ZeigePreisliste();
        }
    }

    /// <summary>Bezahlen-Button: schließt alle offenen Aufträge des Tisches ab.</summary>
    private async void OnBezahlenClicked(object? sender, EventArgs e)
    {
        await AbschliessenAsync();
    }

    /// <summary>Beim Antippen das Betrag-Feld leeren, damit sofort neu eingegeben werden kann.</summary>
    private void OnBetragEntryFocused(object? sender, FocusEventArgs e)
    {
        // Das Leeren asynchron ausführen, damit Android die Tastatur bereits beim ersten
        // Antippen einblendet (das direkte Setzen im Focused-Event unterdrückt sie sonst).
        Dispatcher.Dispatch(() => BetragEntry.Text = string.Empty);
    }

    /// <summary>Beim Antippen das Teilbetrag-Feld leeren, damit sofort neu eingegeben werden kann.</summary>
    private void OnTeilbetragEntryFocused(object? sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() => TeilRechnungSummeEntry.Text = string.Empty);
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
            ZeigePreisliste();
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
            var gelieferte = _rechnungsZeilen
                .Where(p => p.ZeigeGeliefert && p.Geliefert)
                .SelectMany(p => p.Teile.Where(t => t.OffeneMenge > 0m))
                .ToList();
            if (gelieferte.Count > 0)
            {
                await _bestellungApiClient.GeliefertSetzenAsync(gelieferte);
            }

            var handyName = await HandyIdentitaet.GetNameAsync();
            var ergebnis = await _bestellungApiClient.BezahlenAsync(_offeneBestellung.AlleAuftragIds, betrag, handyName);

            // Zurück zur Tischauswahl.
            _offeneBestellung = null;
            ZeigeBestellAnsicht();
            GesamtTitelLabel.IsVisible = true;
            GesamtpreisLabel.IsVisible = true;
            RechnungButton.IsEnabled = false;
            BezahlenButton.IsEnabled = true;
            SetzeTischAuswahlZurueck();

            // Fiskal-QR-Code des Belegs anzeigen (falls die TSE-Signatur erfolgreich war).
            ZeigeFiskalBeleg(ergebnis);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Bezahlen fehlgeschlagen: {ex.Message}");
            await DisplayAlertAsync("Fehler", "Die Rechnung konnte nicht abgeschlossen werden.", "OK");
            RechnungButton.IsEnabled = true;
            BezahlenButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Zeigt den fiskalischen Beleg mit QR-Code an. Bei einem TSE-Fehler wird
    /// stattdessen ein Hinweis angezeigt, dass keine Signatur erzeugt werden konnte.
    /// </summary>
    private void ZeigeFiskalBeleg(BezahlenErgebnis ergebnis)
    {
        if (ergebnis is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ergebnis.FiskalFehler) || string.IsNullOrEmpty(ergebnis.QrCode))
        {
            QrImage.Source = null;
            QrBelegLabel.Text = $"Beleg-Nr. {ergebnis.BelegNummer}";
            QrHinweisLabel.Text = string.IsNullOrEmpty(ergebnis.FiskalFehler)
                ? "Für diesen Beleg wurde keine TSE-Signatur erzeugt."
                : $"TSE-Fehler: {ergebnis.FiskalFehler}";
            QrBereich.IsVisible = true;
            SperreTischAuswahl();
            return;
        }

        try
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(ergebnis.QrCode, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data).GetGraphic(20);
            QrImage.Source = ImageSource.FromStream(() => new MemoryStream(png));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"QR-Code konnte nicht erzeugt werden: {ex.Message}");
            QrImage.Source = null;
        }

        QrBelegLabel.Text = ergebnis.TransaktionsNummer is { } tx
            ? $"Beleg-Nr. {ergebnis.BelegNummer} · TSE-Transaktion {tx}"
            : $"Beleg-Nr. {ergebnis.BelegNummer}";
        QrHinweisLabel.Text = "Hinweis: Simulierte TSE (nicht fiskalkonform).";
        QrBereich.IsVisible = true;
        SperreTischAuswahl();
    }

    /// <summary>Schließt die QR-/Beleganzeige und kehrt zur Bestellansicht zurück.</summary>
    private void OnQrFertigClicked(object? sender, EventArgs e)
    {
        QrBereich.IsVisible = false;
        QrImage.Source = null;

        // Tischauswahl wieder freigeben, außer die Teilrechnung ist noch offen.
        if (!TeilRechnungBereich.IsVisible && !RechnungBereich.IsVisible)
        {
            TischAuswahlButton.IsEnabled = true;
            TischAuswahlButton.ClearValue(VisualElement.BackgroundColorProperty);
        }
    }
}

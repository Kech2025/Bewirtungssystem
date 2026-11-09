namespace Bewirtung.Fiskal
{
    /// <summary>
    /// Art des fiskalischen Geschäftsvorfalls gemäß DSFinV-K / KassenSichV.
    /// </summary>
    public enum FiskalVorgangsTyp
    {
        /// <summary>Normaler Verkauf (Kassenbeleg).</summary>
        Beleg = 0,

        /// <summary>Storno eines vorangegangenen Belegs (als eigener signierter Vorgang).</summary>
        Storno = 1,

        /// <summary>Korrektur / Rückerstattung.</summary>
        Korrektur = 2
    }

    /// <summary>Zahlungsart eines Belegs (DSFinV-K Zahlart).</summary>
    public enum FiskalZahlart
    {
        Bar = 0,
        Unbar = 1
    }

    /// <summary>Status eines fiskalisierten Belegs.</summary>
    public enum FiskalBelegStatus
    {
        /// <summary>Vorgang gestartet, noch nicht signiert/abgeschlossen.</summary>
        Offen = 0,

        /// <summary>Vorgang signiert und abgeschlossen (finalisiert).</summary>
        Abgeschlossen = 1,

        /// <summary>Vorgang abgebrochen (Signatur fehlgeschlagen).</summary>
        Abgebrochen = 2
    }

    /// <summary>
    /// Ein fiskalisierter Beleg (Bon) als unveränderbarer Geschäftsvorfall.
    /// Entspricht einer Zeile in der DSFinV-K-Datei transactions.csv.
    /// Nach dem Abschluss dürfen diese Daten nicht mehr verändert werden.
    /// </summary>
    public class FiskalBeleg
    {
        public long Id { get; set; }

        /// <summary>Fortlaufende, lückenlose Belegnummer pro Kasse.</summary>
        public long BelegNummer { get; set; }

        /// <summary>Kennung der Kasse/des Clients (z. B. Handy-Seriennummer).</summary>
        public string KasseId { get; set; } = string.Empty;

        /// <summary>Name des Bedieners/Benutzers.</summary>
        public string Bediener { get; set; } = string.Empty;

        public FiskalVorgangsTyp VorgangsTyp { get; set; }
        public FiskalBelegStatus Status { get; set; }
        public FiskalZahlart Zahlart { get; set; }

        /// <summary>Zeitpunkt des Vorgangsbeginns (lokale Serverzeit, unveränderbar).</summary>
        public DateTime StartZeitpunkt { get; set; }

        /// <summary>Zeitpunkt des Vorgangsendes/Abschlusses.</summary>
        public DateTime EndZeitpunkt { get; set; }

        /// <summary>Gesamtbetrag brutto über alle Steuersätze.</summary>
        public decimal GesamtBrutto { get; set; }

        /// <summary>Optionaler Bezug zum Ursprungsbeleg bei Storno/Korrektur.</summary>
        public long? UrsprungsBelegId { get; set; }

        /// <summary>Positionszeilen des Belegs (DSFinV-K lines.csv).</summary>
        public List<FiskalBelegPosition> Positionen { get; set; } = new();

        /// <summary>Steuerliche Aufteilung des Belegs nach Steuersatz (businesscases.csv).</summary>
        public List<FiskalSteuerPosition> SteuerPositionen { get; set; } = new();

        /// <summary>Die von der TSE gelieferten Signaturdaten.</summary>
        public FiskalSignatur? Signatur { get; set; }
    }

    /// <summary>Einzelne Positionszeile eines fiskalisierten Belegs (lines.csv).</summary>
    public class FiskalBelegPosition
    {
        public long Id { get; set; }
        public long BelegId { get; set; }

        /// <summary>Positionsnummer innerhalb des Belegs (1-basiert).</summary>
        public int PositionsNummer { get; set; }

        public string Artikel { get; set; } = string.Empty;
        public decimal Menge { get; set; }

        /// <summary>Einzelpreis brutto.</summary>
        public decimal EinzelpreisBrutto { get; set; }

        /// <summary>Gesamtpreis brutto der Position (Menge × Einzelpreis).</summary>
        public decimal GesamtBrutto { get; set; }

        /// <summary>Angewendeter Steuersatz in Prozent (z. B. 7 oder 19).</summary>
        public decimal MwstSatz { get; set; }
    }

    /// <summary>Steuerliche Aufteilung eines Belegs je Steuersatz (businesscases.csv).</summary>
    public class FiskalSteuerPosition
    {
        public long Id { get; set; }
        public long BelegId { get; set; }

        /// <summary>DSFinV-K-Steuerschlüssel (1 = 19 %, 2 = 7 %, usw.).</summary>
        public int SteuerSchluessel { get; set; }

        /// <summary>Steuersatz in Prozent.</summary>
        public decimal MwstSatz { get; set; }

        public decimal Brutto { get; set; }
        public decimal Netto { get; set; }
        public decimal Steuer { get; set; }
    }

    /// <summary>
    /// Von der TSE gelieferte Signaturdaten eines Vorgangs (DSFinV-K tse.csv / transactions.csv).
    /// Diese Werte stammen vom TSE-Provider und werden unverändert gespeichert.
    /// </summary>
    public class FiskalSignatur
    {
        public long Id { get; set; }
        public long BelegId { get; set; }

        /// <summary>Seriennummer der TSE.</summary>
        public string TseSerienNummer { get; set; } = string.Empty;

        /// <summary>Transaktionsnummer der TSE (fortlaufend je TSE).</summary>
        public long TransaktionsNummer { get; set; }

        /// <summary>Signaturzähler der TSE.</summary>
        public long SignaturZaehler { get; set; }

        /// <summary>Die eigentliche Signatur (Base64).</summary>
        public string Signatur { get; set; } = string.Empty;

        /// <summary>Verwendeter Signaturalgorithmus (z. B. ecdsa-plain-SHA256).</summary>
        public string SignaturAlgorithmus { get; set; } = string.Empty;

        /// <summary>Public-Key-Hinweis / Zertifikatsreferenz der TSE.</summary>
        public string ZertifikatReferenz { get; set; } = string.Empty;

        /// <summary>Startzeitpunkt der Transaktion laut TSE (ISO 8601).</summary>
        public DateTime TseStartZeit { get; set; }

        /// <summary>Endzeitpunkt der Transaktion laut TSE (ISO 8601).</summary>
        public DateTime TseEndZeit { get; set; }

        /// <summary>Prozesstyp laut TSE (z. B. Kassenbeleg-V1).</summary>
        public string ProzessTyp { get; set; } = string.Empty;

        /// <summary>Für den Bon-QR-Code zusammengesetzte Signaturdaten.</summary>
        public string QrCodeDaten { get; set; } = string.Empty;
    }

    /// <summary>
    /// Täglicher Kassenabschluss (Z-Bon, DSFinV-K cashpointclosing.csv).
    /// </summary>
    public class FiskalAbschluss
    {
        public long Id { get; set; }

        /// <summary>Fortlaufende Z-Nummer (Abschlussnummer).</summary>
        public long ZNummer { get; set; }

        public string KasseId { get; set; } = string.Empty;

        public DateTime ErstZeitpunkt { get; set; }
        public DateTime LetztZeitpunkt { get; set; }
        public DateTime AbschlussZeitpunkt { get; set; }

        /// <summary>Erste enthaltene Belegnummer.</summary>
        public long VonBelegNummer { get; set; }

        /// <summary>Letzte enthaltene Belegnummer.</summary>
        public long BisBelegNummer { get; set; }

        public decimal SummeBrutto { get; set; }
        public decimal SummeBar { get; set; }
        public decimal SummeUnbar { get; set; }
    }
}

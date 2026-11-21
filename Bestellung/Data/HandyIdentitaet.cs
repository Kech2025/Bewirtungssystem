namespace Bestellung.Data;

/// <summary>
/// Kapselt die dauerhafte, gerätegebundene Handy-Identität (Seriennummer + Name)
/// im SecureStorage. Die Seriennummer wird einmalig beim ersten Start erfasst.
/// </summary>
public static class HandyIdentitaet
{
    private const string SeriennummerKey = "handy_seriennummer";
    private const string NameKey = "handy_name";
    private const string BestellungKey = "handy_bestellung";

    /// <summary>Gespeicherte Seriennummer oder null, wenn noch keine erfasst wurde.</summary>
    public static async Task<string?> GetSeriennummerAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(SeriennummerKey);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Gespeicherter Handy-Name oder null.</summary>
    public static async Task<string?> GetNameAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(NameKey);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Speichert Seriennummer und zugehörigen Namen dauerhaft.</summary>
    public static async Task SpeichereAsync(string seriennummer, string name)
    {
        await SetSicherAsync(SeriennummerKey, seriennummer);
        await SetSicherAsync(NameKey, name ?? string.Empty);
    }

    /// <summary>
    /// Schreibt in den SecureStorage. Schlägt das fehl (z. B. defekter Schlüssel nach
    /// "Speicher löschen" oder Backup-Wiederherstellung), wird der Speicher geleert und erneut geschrieben.
    /// </summary>
    private static async Task SetSicherAsync(string key, string wert)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, wert);
        }
        catch
        {
            SecureStorage.Default.RemoveAll();
            await SecureStorage.Default.SetAsync(key, wert);
        }
    }

    /// <summary>Speichert das Bestellmodus-Flag für dieses Gerät.</summary>
    public static async Task SpeichereBestellungAsync(bool bestellung)
    {
        await SetSicherAsync(BestellungKey, bestellung ? "1" : "0");
    }

    /// <summary>Gespeichertes Bestellmodus-Flag (Default true, falls nicht gesetzt).</summary>
    public static async Task<bool> GetBestellungAsync()
    {
        try
        {
            var wert = await SecureStorage.Default.GetAsync(BestellungKey);
            return wert is null || wert == "1";
        }
        catch
        {
            return true;
        }
    }

    /// <summary>Löscht Seriennummer, Name und Bestellmodus, damit das Gerät neu registriert werden kann.</summary>
    public static void Zuruecksetzen()
    {
        SecureStorage.Default.Remove(SeriennummerKey);
        SecureStorage.Default.Remove(NameKey);
        SecureStorage.Default.Remove(BestellungKey);
    }

    /// <summary>True, wenn bereits eine Seriennummer hinterlegt ist.</summary>
    public static async Task<bool> IstRegistriertAsync()
    {
        var seriennummer = await GetSeriennummerAsync();
        return !string.IsNullOrWhiteSpace(seriennummer);
    }
}

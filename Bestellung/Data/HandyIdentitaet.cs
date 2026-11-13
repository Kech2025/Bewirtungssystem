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
        await SecureStorage.Default.SetAsync(SeriennummerKey, seriennummer);
        await SecureStorage.Default.SetAsync(NameKey, name ?? string.Empty);
    }

    /// <summary>Speichert das Bestellmodus-Flag für dieses Gerät.</summary>
    public static async Task SpeichereBestellungAsync(bool bestellung)
    {
        await SecureStorage.Default.SetAsync(BestellungKey, bestellung ? "1" : "0");
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

    /// <summary>True, wenn bereits eine Seriennummer hinterlegt ist.</summary>
    public static async Task<bool> IstRegistriertAsync()
    {
        var seriennummer = await GetSeriennummerAsync();
        return !string.IsNullOrWhiteSpace(seriennummer);
    }
}

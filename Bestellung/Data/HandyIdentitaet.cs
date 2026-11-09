namespace Bestellung.Data;

/// <summary>
/// Kapselt die dauerhafte, gerätegebundene Handy-Identität (Seriennummer + Name)
/// im SecureStorage. Die Seriennummer wird einmalig beim ersten Start erfasst.
/// </summary>
public static class HandyIdentitaet
{
    private const string SeriennummerKey = "handy_seriennummer";
    private const string NameKey = "handy_name";

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

    /// <summary>True, wenn bereits eine Seriennummer hinterlegt ist.</summary>
    public static async Task<bool> IstRegistriertAsync()
    {
        var seriennummer = await GetSeriennummerAsync();
        return !string.IsNullOrWhiteSpace(seriennummer);
    }
}

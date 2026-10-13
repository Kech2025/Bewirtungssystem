namespace Bestellung.Data;

/// <summary>
/// Zentrale Konfiguration für den Zugriff auf die Web-API des lokalen Servers.
/// </summary>
public static class ApiConfig
{
    // Port der Web-API (siehe appsettings.json im Bewirtung.Api-Projekt).
    private const int Port = 5080;

    // Für ein echtes Gerät im LAN hier die IP des Server-PCs eintragen
    // (z. B. "192.168.1.10"); dann wird diese immer verwendet.
    public static string? ServerIpOverride { get; set; } = null;

    // Basis-URL der Web-API. Wird automatisch je Plattform gewählt:
    //  - Android-Emulator:  http://10.0.2.2:<Port>  (Alias für "localhost des Host-PCs")
    //  - Windows/sonst:     http://localhost:<Port>
    //  - ServerIpOverride:  http://<IP>:<Port>       (für echte Geräte im Netzwerk)
    public static string BaseUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ServerIpOverride))
            {
                return $"http://{ServerIpOverride}:{Port}";
            }

            var host = DeviceInfo.Platform == DevicePlatform.Android
                ? "10.0.2.2"
                : "localhost";

            return $"http://{host}:{Port}";
        }
    }
}

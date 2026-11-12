namespace Bestellung.Data;

/// <summary>
/// Zentrale Konfiguration für den Zugriff auf die Web-API des lokalen Servers.
/// </summary>
public static class ApiConfig
{
    // Port der Web-API (NodePort des bewirtung-api-Service im k3s-Cluster).
    private const int Port = 30580;

    // IP des Raspberry Pi (Server) im LAN. Wird für echte Geräte verwendet.
    private const string PiIpAddress = "192.168.170.31";

    // Optionaler manueller Override. Wenn gesetzt, wird immer diese IP genutzt
    // (unabhängig von Emulator/Gerät). Nützlich, wenn der Server (API) auf der
    // Pi läuft und auch der Emulator die Pi ansprechen soll.
    public static string? ServerIpOverride { get; set; } = "192.168.170.31";

    // Basis-URL der Web-API. Wird automatisch gewählt:
    //  - ServerIpOverride gesetzt: http://<IP>:<Port>        (manueller Override)
    //  - Android-Emulator:         http://10.0.2.2:<Port>    (Alias für "localhost des Host-PCs")
    //  - Echtes Gerät:             http://192.168.178.111:<Port>  (Raspberry Pi im LAN)
    //  - Windows/sonst (Emulator): http://localhost:<Port>
    public static string BaseUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ServerIpOverride))
            {
                return $"http://{ServerIpOverride}:{Port}";
            }

            // Echtes (physisches) Gerät -> Raspberry Pi im LAN ansprechen.
            if (DeviceInfo.DeviceType == DeviceType.Physical)
            {
                return $"http://{PiIpAddress}:{Port}";
            }

            // Emulator/Simulator -> lokalen Dev-PC ansprechen.
            var host = DeviceInfo.Platform == DevicePlatform.Android
                ? "10.0.2.2"
                : "localhost";

            return $"http://{host}:{Port}";
        }
    }
}

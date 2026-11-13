using System.Net.Http.Json;

namespace Bestellung.Data;

/// <summary>
/// Protokolliert An- und Abmeldungen der Bestellung-App in der Tabelle journal
/// über die Web-API.
/// </summary>
public class JournalApiClient
{
    private const string OffeneSitzungKey = "journal_offene_userid";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    /// <summary>
    /// Legt einen Journal-Eintrag für den Login an (POST /api/journal/login).
    /// </summary>
    public async Task LoginAsync(
        long userId, int? modus, int? termin, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/journal/login";
        var response = await _httpClient.PostAsJsonAsync(
            url, new { UserId = userId, Modus = modus, Termin = termin }, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Offene Sitzung persistent merken, damit sie auch nach hartem App-Ende
        // (ohne Abmelden) beim nächsten Start nachgeholt werden kann.
        Preferences.Set(OffeneSitzungKey, (int)userId);
    }

    /// <summary>
    /// Schließt den letzten offenen Journal-Eintrag des Benutzers ab (POST /api/journal/logout).
    /// </summary>
    public async Task LogoutAsync(long userId, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiConfig.BaseUrl.TrimEnd('/')}/api/journal/logout";
        var response = await _httpClient.PostAsJsonAsync(
            url, new { UserId = userId }, cancellationToken);
        response.EnsureSuccessStatusCode();

        Preferences.Remove(OffeneSitzungKey);
    }

    /// <summary>
    /// Meldet eine aus einer früheren Sitzung noch offene Anmeldung ab (z. B. wenn die
    /// App geschlossen wurde, ohne den Abmelden-Button zu drücken). Tut nichts, wenn
    /// keine offene Sitzung gespeichert ist.
    /// </summary>
    public async Task BeendeOffeneSitzungAsync(CancellationToken cancellationToken = default)
    {
        if (!Preferences.ContainsKey(OffeneSitzungKey))
        {
            return;
        }

        var userId = Preferences.Get(OffeneSitzungKey, 0);
        if (userId <= 0)
        {
            Preferences.Remove(OffeneSitzungKey);
            return;
        }

        await LogoutAsync(userId, cancellationToken);
    }
}

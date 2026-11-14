using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Bewirtung.Webserver.Data;

/// <summary>
/// Zustand eines einzelnen Cluster-Knotens (Raspberry Pi).
/// </summary>
public sealed class ClusterNodeStatus
{
    public string Name { get; init; } = string.Empty;

    /// <summary>true = Node ist im Zustand "Ready", false = nicht erreichbar/NotReady.</summary>
    public bool Ready { get; init; }
}

/// <summary>
/// Fragt den Ready-Zustand der Cluster-Knoten über die Kubernetes-API ab.
/// Nutzt die In-Cluster-Konfiguration (ServiceAccount-Token + CA) des Pods.
/// </summary>
public sealed class ClusterStatusService
{
    // Erwartete/anzuzeigende Knoten (feste Reihenfolge).
    private static readonly string[] ErwarteteNodes = { "pi-1", "pi-2", "pi-3" };

    private const string TokenPfad = "/var/run/secrets/kubernetes.io/serviceaccount/token";
    private const string CaPfad = "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt";

    private readonly ILogger<ClusterStatusService> _logger;

    public ClusterStatusService(ILogger<ClusterStatusService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Liefert für jeden erwarteten Node seinen Ready-Zustand. Nicht gefundene oder
    /// nicht abfragbare Nodes werden als nicht bereit (rot) zurückgegeben.
    /// </summary>
    public async Task<List<ClusterNodeStatus>> GetNodeStatusAsync(CancellationToken cancellationToken = default)
    {
        var readyNamen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            readyNamen = await LadeReadyNodesAsync(_logger, cancellationToken);
            _logger.LogDebug(
                "Cluster-Status abgefragt. Ready-Nodes: {ReadyNodes}",
                readyNamen.Count == 0 ? "(keine)" : string.Join(", ", readyNamen));
        }
        catch (Exception ex)
        {
            // API nicht erreichbar -> alle Nodes als nicht bereit anzeigen.
            _logger.LogError(ex, "Cluster-Status konnte nicht von der Kubernetes-API gelesen werden.");
            readyNamen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return ErwarteteNodes
            .Select(name => new ClusterNodeStatus
            {
                Name = name,
                Ready = readyNamen.Contains(name)
            })
            .ToList();
    }

    private static async Task<HashSet<string>> LadeReadyNodesAsync(ILogger logger, CancellationToken cancellationToken)
    {
        var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
        var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT_HTTPS") ?? "443";

        if (string.IsNullOrEmpty(host) || !File.Exists(TokenPfad))
        {
            throw new InvalidOperationException("Keine In-Cluster-Kubernetes-Konfiguration verfügbar.");
        }

        var token = await File.ReadAllTextAsync(TokenPfad, cancellationToken);

        using var handler = new HttpClientHandler();
        if (File.Exists(CaPfad))
        {
            // k3s legt die CA im PEM-Format ab; daher PEM-konform laden (nur Zertifikat, kein Key).
            var caCert = X509Certificate2.CreateFromPem(await File.ReadAllTextAsync(CaPfad, cancellationToken));
            handler.ServerCertificateCustomValidationCallback = (_, cert, chain, _) =>
            {
                if (cert is null)
                {
                    return false;
                }

                chain ??= new X509Chain();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(caCert);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                return chain.Build(new X509Certificate2(cert));
            };
        }

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://{host}:{port}"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        using var response = await client.GetAsync("/api/v1/nodes", cancellationToken);
        logger.LogDebug("Kubernetes-API {Host}:{Port} /api/v1/nodes -> {Status}", host, port, (int)response.StatusCode);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var ready = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (doc.RootElement.TryGetProperty("items", out var items))
        {
            foreach (var node in items.EnumerateArray())
            {
                if (!node.TryGetProperty("metadata", out var meta) ||
                    !meta.TryGetProperty("name", out var nameEl))
                {
                    continue;
                }

                var name = nameEl.GetString();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var istReady = IstReady(node);
                logger.LogDebug("Node gefunden: {Name} (Ready={Ready})", name, istReady);
                if (istReady)
                {
                    ready.Add(name);
                }
            }
        }

        return ready;
    }

    private static bool IstReady(JsonElement node)
    {
        if (!node.TryGetProperty("status", out var status) ||
            !status.TryGetProperty("conditions", out var conditions))
        {
            return false;
        }

        foreach (var condition in conditions.EnumerateArray())
        {
            if (condition.TryGetProperty("type", out var type) &&
                string.Equals(type.GetString(), "Ready", StringComparison.OrdinalIgnoreCase) &&
                condition.TryGetProperty("status", out var cstatus))
            {
                return string.Equals(cstatus.GetString(), "True", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }
}

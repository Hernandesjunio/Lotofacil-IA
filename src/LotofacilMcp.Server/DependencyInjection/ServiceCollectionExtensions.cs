using LotofacilMcp.Server.Tools;
using Microsoft.Extensions.Options;
using LotofacilMcp.Infrastructure.Providers;
using System.Net;
using System.Net.Security;

namespace LotofacilMcp.Server.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddV0Server(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.Configure<DatasetOptions>(configuration.GetSection(DatasetOptions.SectionName));
        services.Configure<AccessTogglesOptions>(configuration.GetSection(AccessTogglesOptions.SectionName));

        services.AddHttpClient("dataset-http", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("LotofacilMcp.Server/v0");

                // Some corporate proxies / TLS middleboxes break HTTP/2 negotiation.
                // Force HTTP/1.1 for dataset downloads to avoid spurious TLS/cert handshake failures.
                client.DefaultRequestVersion = HttpVersion.Version11;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                // V0: in Development allow localhost HTTPS test servers with self-signed certs.
                // Production/default behavior preserves platform TLS validation.
                var handler = new HttpClientHandler();
                if (hostEnvironment.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback = (request, _, _, sslPolicyErrors) =>
                    {
                        var host = request?.RequestUri?.Host;

                        // In Development, allow self-signed certs only for local test servers.
                        // For any other host, preserve default platform validation.
                        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(host, "[::1]", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        return sslPolicyErrors == SslPolicyErrors.None;
                    };
                }

                return handler;
            });

        services.AddSingleton(sp =>
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "LotofacilMcp", "dataset-snapshots-v0");
            return new HttpJsonDatasetSnapshotCache(
                httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient("dataset-http"),
                cacheDirectory: cacheDir);
        });

        services.AddSingleton(sp =>
        {
            var datasetOptions = sp.GetRequiredService<IOptions<DatasetOptions>>().Value;
            var accessOptions = sp.GetRequiredService<IOptions<AccessTogglesOptions>>().Value;
            var httpSnapshotCache = sp.GetRequiredService<HttpJsonDatasetSnapshotCache>();

            EnsureAccessTogglesAreDisabled(accessOptions);

            var sourceUri = datasetOptions.DrawsSourceUri;
            return new V0Tools(
                drawsSourceUri: sourceUri,
                contentRootPath: hostEnvironment.ContentRootPath,
                httpSnapshotCache: httpSnapshotCache);
        });

        return services;
    }

    private static void EnsureAccessTogglesAreDisabled(AccessTogglesOptions options)
    {
        if (options.AuthEnabled || options.ThrottleEnabled || options.QuotaEnabled)
        {
            throw new InvalidOperationException(
                "V0 requires access toggles disabled: auth/throttle/quota must remain off.");
        }
    }

    private static string? ResolveDrawsSourceUriToLocalPathOrNull(string contentRootPath, string? configuredDrawsSourceUri)
    {
        if (string.IsNullOrWhiteSpace(configuredDrawsSourceUri))
        {
            return null;
        }

        var trimmed = configuredDrawsSourceUri.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            return Path.GetFullPath(uri.LocalPath);
        }

        if (Path.IsPathRooted(trimmed))
        {
            return Path.GetFullPath(trimmed);
        }

        // Deterministic resolution (ADR 0022): relative paths resolve against content root only.
        return Path.GetFullPath(Path.Combine(contentRootPath, trimmed));        
    }
}

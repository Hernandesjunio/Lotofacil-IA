using LotofacilMcp.Server.Tools;
using Microsoft.Extensions.Options;

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

        services.AddSingleton(sp =>
        {
            var datasetOptions = sp.GetRequiredService<IOptions<DatasetOptions>>().Value;
            var accessOptions = sp.GetRequiredService<IOptions<AccessTogglesOptions>>().Value;

            EnsureAccessTogglesAreDisabled(accessOptions);

            var sourceUri = datasetOptions.DrawsSourceUri;
            var resolvedLocalPath = ResolveDrawsSourceUriToLocalPathOrNull(hostEnvironment.ContentRootPath, sourceUri);
            return new V0Tools(resolvedLocalPath);
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

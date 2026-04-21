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
        services.Configure<V0DataOptions>(configuration.GetSection(V0DataOptions.SectionName));
        services.Configure<AccessTogglesOptions>(configuration.GetSection(AccessTogglesOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var dataOptions = sp.GetRequiredService<IOptions<V0DataOptions>>().Value;
            var accessOptions = sp.GetRequiredService<IOptions<AccessTogglesOptions>>().Value;

            EnsureAccessTogglesAreDisabled(accessOptions);

            var fixturePath = ResolveFixturePath(hostEnvironment.ContentRootPath, dataOptions.FixturePath);
            return new V0Tools(fixturePath);
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

    private static string ResolveFixturePath(string contentRootPath, string configuredFixturePath)
    {
        if (string.IsNullOrWhiteSpace(configuredFixturePath))
        {
            throw new InvalidOperationException(
                $"Missing required configuration '{V0DataOptions.SectionName}:FixturePath'.");
        }

        if (Path.IsPathRooted(configuredFixturePath))
        {
            return configuredFixturePath;
        }

        var candidatePath = Path.GetFullPath(Path.Combine(contentRootPath, configuredFixturePath));
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }

        var currentDirectory = new DirectoryInfo(contentRootPath);
        while (currentDirectory is not null)
        {
            candidatePath = Path.GetFullPath(Path.Combine(currentDirectory.FullName, configuredFixturePath));
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not resolve fixture file '{configuredFixturePath}' from content root '{contentRootPath}'.");
    }
}

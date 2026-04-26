namespace LotofacilMcp.Server.DependencyInjection;

public sealed class DatasetOptions
{
    public const string SectionName = "Dataset";

    public string? DrawsSourceUri { get; init; }
}

public sealed class AccessTogglesOptions
{
    public const string SectionName = "AccessToggles";

    public bool AuthEnabled { get; init; }

    public bool ThrottleEnabled { get; init; }

    public bool QuotaEnabled { get; init; }
}

namespace LotofacilMcp.Server.DependencyInjection;

public sealed class V0DataOptions
{
    public const string SectionName = "V0Data";

    public string FixturePath { get; init; } = "tests/fixtures/synthetic_min_window.json";
}

public sealed class AccessTogglesOptions
{
    public const string SectionName = "AccessToggles";

    public bool AuthEnabled { get; init; }

    public bool ThrottleEnabled { get; init; }

    public bool QuotaEnabled { get; init; }
}

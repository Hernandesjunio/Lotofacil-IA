using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase23DiscoverCapabilitiesContractTests
{
    [Fact]
    public void DiscoverCapabilities_ReturnsDeterministicStructuredBuildSurface()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());

        var first = sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest());
        var second = sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest());

        var payloadA = Assert.IsType<DiscoverCapabilitiesResponse>(first);
        var payloadB = Assert.IsType<DiscoverCapabilitiesResponse>(second);

        Assert.Equal(payloadA.ToolVersion, payloadB.ToolVersion);
        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("v0", payloadA.BuildProfile);
        Assert.NotEmpty(payloadA.DatasetRequirements);

        Assert.Contains(payloadA.Tools, tool => tool.Name is "discover_capabilities");
        Assert.Contains(payloadA.Tools, tool => tool.Name is "compute_window_metrics");
        Assert.Contains(payloadA.Tools, tool => tool.Name is "summarize_window_aggregates");
        Assert.Contains(payloadA.Tools, tool => tool.Name is "generate_candidate_games");

        Assert.Contains(payloadA.Metrics.ImplementedMetricNames, name => name is "frequencia_por_dezena");
        Assert.Contains(payloadA.Metrics.PendingMetricNames, name => name is "repeticao_concurso_anterior");
        Assert.Contains(payloadA.Metrics.ComputeWindowMetricsAllowed, name => name is "frequencia_por_dezena");
        Assert.DoesNotContain(payloadA.Metrics.ComputeWindowMetricsAllowed, name => name is "repeticao_concurso_anterior");
        Assert.Contains(payloadA.Metrics.SummarizeWindowAggregatesAllowedSources, name => name is "repeticao_concurso_anterior");
        Assert.Contains(payloadA.Metrics.AssociationAllowedIndicators, name => name is "entropia_linha_por_concurso");

        Assert.True(payloadA.Generation.Strategies.Count >= 2);
        Assert.Contains(payloadA.Generation.Strategies, strategy => strategy.Name is "common_repetition_frequency");
        Assert.Contains(payloadA.Generation.Strategies, strategy => strategy.Name is "declared_composite_profile");
        Assert.Contains("greedy_topk", payloadA.Generation.SearchMethods);
        Assert.Contains("max_neighbor_count", payloadA.Generation.SupportedFilters);

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("build_profile", out _));
        Assert.True(root.TryGetProperty("dataset_requirements", out _));
        Assert.True(root.TryGetProperty("window_modes_supported", out _));
        Assert.True(root.TryGetProperty("tools", out _));
        Assert.True(root.TryGetProperty("metrics", out _));
        Assert.True(root.TryGetProperty("generation", out _));
    }

    [Fact]
    public void DiscoverCapabilities_DeclaresChatSafeDefaultAndWindowOperationalConstraints()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var payload = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));

        Assert.Contains(payload.ContentChannelRules, rule =>
            rule.Contains("resultado principal", StringComparison.OrdinalIgnoreCase) &&
            rule.Contains("Content", StringComparison.Ordinal));

        var getDrawWindow = Assert.Single(payload.Tools, tool => tool.Name is "get_draw_window");
        Assert.Equal(["standard"], getDrawWindow.SupportedParameters["verbosity.default_recommended"]);
        Assert.Equal(["window_size > 0"], getDrawWindow.SupportedParameters["window_size.constraint"]);
        Assert.Equal(["start_contest_id requires end_contest_id"], getDrawWindow.SupportedParameters["start_contest_id.constraint"]);
        Assert.Equal(["start_contest_id must be <= end_contest_id"], getDrawWindow.SupportedParameters["start_end.constraint"]);
        Assert.Equal(
            ["if start_contest_id/end_contest_id are provided, window_size must be omitted/0 or equal to (end-start+1)"],
            getDrawWindow.SupportedParameters["window_size_start_end.coherence"]);
        Assert.Equal(
            ["window_size=1 anchors the latest available contest when end_contest_id is omitted"],
            getDrawWindow.SupportedParameters["window_size.quickstart"]);
    }

    [Fact]
    public void Help_QuickStart_ExplainsMinimalFullFlowAndTraceabilityFields()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var payload = Assert.IsType<HelpResponse>(sut.Help());
        var quickStart = Assert.IsType<string>(payload.QuickStartMarkdown);

        Assert.Contains("get_draw_window(window_size=1)", quickStart, StringComparison.Ordinal);
        Assert.Contains("compute_window_metrics", quickStart, StringComparison.Ordinal);
        Assert.Contains("verbosity=\"minimal\"", quickStart, StringComparison.Ordinal);
        Assert.Contains("verbosity=\"standard\"", quickStart, StringComparison.Ordinal);
        Assert.Contains("verbosity=\"full\"", quickStart, StringComparison.Ordinal);
        Assert.Contains("dataset_version", quickStart, StringComparison.Ordinal);
        Assert.Contains("tool_version", quickStart, StringComparison.Ordinal);
        Assert.Contains("deterministic_hash", quickStart, StringComparison.Ordinal);
        Assert.Contains("window", quickStart, StringComparison.Ordinal);
        Assert.Contains("fields", quickStart, StringComparison.Ordinal);
        Assert.Contains("paginação", quickStart, StringComparison.OrdinalIgnoreCase);
    }
}

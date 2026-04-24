using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase23DiscoverCapabilitiesContractTests
{
    [Fact]
    public void DiscoverCapabilities_ReturnsDeterministicStructuredBuildSurface()
    {
        var sut = new V0Tools();

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
}

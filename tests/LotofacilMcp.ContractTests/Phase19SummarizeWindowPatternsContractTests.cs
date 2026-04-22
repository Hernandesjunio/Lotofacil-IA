using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase19SummarizeWindowPatternsContractTests
{
    private static SummarizeWindowPatternsRequest BuildGoldenRequest() => new(
        WindowSize: 5,
        EndContestId: 1005,
        Features:
        [
            new WindowPatternFeatureRequest("pares_no_concurso", null)
        ],
        CoverageThreshold: 0.8,
        RangeMethod: "iqr");

    [Fact]
    public void SummarizeWindowPatterns_IqrSingleFeature_DeterministicGoldenFixture()
    {
        var sut = new V0Tools();
        var request = BuildGoldenRequest();
        var first = sut.SummarizeWindowPatterns(request);
        var second = sut.SummarizeWindowPatterns(request);
        var payloadA = Assert.IsType<SummarizeWindowPatternsResponse>(first);
        var payloadB = Assert.IsType<SummarizeWindowPatternsResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("1.0.0", payloadA.ToolVersion);
        Assert.Equal("iqr", payloadA.RangeMethod);
        Assert.Equal(0.8, payloadA.CoverageThreshold, precision: 12);
        Assert.Equal(5, payloadA.Window.Size);
        Assert.Equal(1001, payloadA.Window.StartContestId);
        Assert.Equal(1005, payloadA.Window.EndContestId);
        Assert.Single(payloadA.Summaries);

        var summary = payloadA.Summaries[0];
        Assert.Equal("pares_no_concurso", summary.MetricName, StringComparer.Ordinal);
        Assert.Equal("identity", summary.Aggregation, StringComparer.Ordinal);
        Assert.Equal(8.0, summary.Mode, precision: 12);
        Assert.Equal(8.0, summary.Q1, precision: 12);
        Assert.Equal(8.0, summary.Median, precision: 12);
        Assert.Equal(9.0, summary.Q3, precision: 12);
        Assert.Equal(1.0, summary.Iqr, precision: 12);
        Assert.Equal(3, summary.CoverageCount);
        Assert.Equal(5, summary.TotalCount);
        Assert.Equal(1, summary.OutlierCount);
        Assert.Equal(0.6, summary.CoverageObserved, precision: 12);
        Assert.False(summary.CoverageThresholdMet);

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("range_method", out _));
        Assert.True(root.TryGetProperty("coverage_threshold", out _));
        Assert.True(root.TryGetProperty("summaries", out var summaries));
        Assert.Equal(JsonValueKind.Array, summaries.ValueKind);
        var firstSummary = summaries[0];
        Assert.True(firstSummary.TryGetProperty("q1", out _));
        Assert.True(firstSummary.TryGetProperty("median", out _));
        Assert.True(firstSummary.TryGetProperty("q3", out _));
        Assert.True(firstSummary.TryGetProperty("iqr", out _));
        Assert.True(firstSummary.TryGetProperty("coverage_observed", out _));
        Assert.True(firstSummary.TryGetProperty("coverage_count", out _));
        Assert.True(firstSummary.TryGetProperty("total_count", out _));
        Assert.True(firstSummary.TryGetProperty("outlier_count", out _));
    }

    [Fact]
    public void SummarizeWindowPatterns_UnsupportedRangeMethod_ReturnsUnsupportedRangeMethod()
    {
        var sut = new V0Tools();
        var request = new SummarizeWindowPatternsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Features:
            [
                new WindowPatternFeatureRequest("pares_no_concurso", null)
            ],
            CoverageThreshold: 0.8,
            RangeMethod: "mad");

        var response = sut.SummarizeWindowPatterns(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_RANGE_METHOD", error.Code);
    }
}

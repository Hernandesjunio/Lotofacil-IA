using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase10AnalyzeIndicatorStabilityContractTests
{
    [Fact]
    public void AnalyzeIndicatorStability_ResponseContainsEnvelopeAndRanking()
    {
        var sut = new V0Tools();
        var request = new AnalyzeIndicatorStabilityRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Indicators:
            [
                new StabilityIndicatorRequestDto("repeticao_concurso_anterior", null),
                new StabilityIndicatorRequestDto("distribuicao_linha_por_concurso", "per_component")
            ],
            NormalizationMethod: null,
            TopK: 3,
            MinHistory: 3);

        var response = sut.AnalyzeIndicatorStability(request);

        var payload = Assert.IsType<AnalyzeIndicatorStabilityResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(payload.DatasetVersion));
        Assert.False(string.IsNullOrWhiteSpace(payload.ToolVersion));
        Assert.False(string.IsNullOrWhiteSpace(payload.DeterministicHash));
        Assert.Equal("madn", payload.NormalizationMethod);
        Assert.Equal(5, payload.Window.Size);
        Assert.Equal(1001, payload.Window.StartContestId);
        Assert.Equal(1005, payload.Window.EndContestId);
        Assert.Equal(3, payload.Ranking.Count);

        Assert.All(payload.Ranking, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.IndicatorName));
            Assert.False(string.IsNullOrWhiteSpace(entry.Aggregation));
            Assert.False(string.IsNullOrWhiteSpace(entry.Shape));
            Assert.InRange(entry.StabilityScore, 0d, 1d);
            Assert.True(entry.Dispersion >= 0d);
            Assert.False(string.IsNullOrWhiteSpace(entry.Explanation));
        });

        using var json = JsonSerializer.SerializeToDocument(payload);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("window", out _));
        Assert.True(root.TryGetProperty("normalization_method", out _));
        Assert.True(root.TryGetProperty("ranking", out _));
    }

    [Fact]
    public void AnalyzeIndicatorStability_VectorWithoutAggregation_ReturnsUnsupportedAggregation()
    {
        var sut = new V0Tools();
        var request = new AnalyzeIndicatorStabilityRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Indicators:
            [
                new StabilityIndicatorRequestDto("distribuicao_linha_por_concurso", null)
            ],
            NormalizationMethod: "madn",
            TopK: 3,
            MinHistory: 3);

        var response = sut.AnalyzeIndicatorStability(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_AGGREGATION", error.Code);
    }
}

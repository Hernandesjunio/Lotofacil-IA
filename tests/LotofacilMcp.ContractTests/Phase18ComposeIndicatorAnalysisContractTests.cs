using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase18ComposeIndicatorAnalysisContractTests
{
    private static ComposeIndicatorAnalysisRequest BuildGoldenRequest(int topK = 10) => new(
        WindowSize: 5,
        EndContestId: 1005,
        Target: "dezena",
        Operator: "weighted_rank",
        Components:
        [
            new ComposeIndicatorComponentRequest("frequencia_por_dezena", "normalize_max", 0.4),
            new ComposeIndicatorComponentRequest("atraso_por_dezena", "invert_normalize_max", 0.3),
            new ComposeIndicatorComponentRequest("assimetria_blocos", "shift_scale_unit_interval", 0.3)
        ],
        TopK: topK);

    [Fact]
    public void ComposeIndicatorAnalysis_WeightedRankDezena_MatchesFixedGoldenOutputs()
    {
        var sut = new V0Tools();
        var request = BuildGoldenRequest(topK: 10);
        var first = sut.ComposeIndicatorAnalysis(request);
        var second = sut.ComposeIndicatorAnalysis(request);
        var payloadA = Assert.IsType<ComposeIndicatorAnalysisResponse>(first);
        var payloadB = Assert.IsType<ComposeIndicatorAnalysisResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("1.0.0", payloadA.ToolVersion);
        Assert.Equal("dezena", payloadA.Target);
        Assert.Equal("weighted_rank", payloadA.Operator);
        Assert.Equal(5, payloadA.Window.Size);
        Assert.Equal(1001, payloadA.Window.StartContestId);
        Assert.Equal(1005, payloadA.Window.EndContestId);
        Assert.Equal(10, payloadA.Ranking.Count);

        // Valores dourados do recorte (synthetic_min_window, janela 5 -> 1001..1005)
        Assert.Equal(10, payloadA.Ranking[0].Dezena);
        Assert.Equal(1.0, payloadA.Ranking[0].Score, precision: 12);
        Assert.Equal(1, payloadA.Ranking[0].Rank);
        Assert.Equal(12, payloadA.Ranking[1].Dezena);
        Assert.Equal(1.0, payloadA.Ranking[1].Score, precision: 12);
        Assert.Equal(2, payloadA.Ranking[1].Rank);

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("window", out _));
        Assert.True(root.TryGetProperty("target", out _));
        Assert.True(root.TryGetProperty("operator", out _));
        Assert.True(root.TryGetProperty("ranking", out _));
    }

    [Fact]
    public void ComposeIndicatorAnalysis_WeightsNotSummingToOneWithinTolerance_ReturnsIncompatibleComposition()
    {
        var sut = new V0Tools();
        var request = new ComposeIndicatorAnalysisRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Target: "dezena",
            Operator: "weighted_rank",
            Components:
            [
                new ComposeIndicatorComponentRequest("frequencia_por_dezena", "normalize_max", 0.4),
                new ComposeIndicatorComponentRequest("atraso_por_dezena", "invert_normalize_max", 0.3),
                new ComposeIndicatorComponentRequest("assimetria_blocos", "shift_scale_unit_interval", 0.25)
            ],
            TopK: 3);

        var response = sut.ComposeIndicatorAnalysis(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INCOMPATIBLE_COMPOSITION", error.Code);
    }

    [Fact]
    public void ComposeIndicatorAnalysis_InvalidTransform_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new ComposeIndicatorAnalysisRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Target: "dezena",
            Operator: "weighted_rank",
            Components:
            [
                new ComposeIndicatorComponentRequest("frequencia_por_dezena", "not_a_listed_transform", 0.4),
                new ComposeIndicatorComponentRequest("atraso_por_dezena", "invert_normalize_max", 0.3),
                new ComposeIndicatorComponentRequest("assimetria_blocos", "shift_scale_unit_interval", 0.3)
            ],
            TopK: 3);

        var response = sut.ComposeIndicatorAnalysis(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }
}

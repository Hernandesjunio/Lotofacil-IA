using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase19AnalyzeIndicatorAssociationsContractTests
{
    private static AnalyzeIndicatorAssociationsRequest BuildScalarSpearmanRequest(int topK = 10) => new(
        WindowSize: 5,
        EndContestId: 1005,
        Items:
        [
            new AssociationItemRequest("repeticao_concurso_anterior", null),
            new AssociationItemRequest("pares_no_concurso", null),
            new AssociationItemRequest("quantidade_vizinhos_por_concurso", null),
            new AssociationItemRequest("sequencia_maxima_vizinhos_por_concurso", null)
        ],
        Method: "spearman",
        TopK: topK,
        StabilityCheck: null);

    [Fact]
    public void AnalyzeIndicatorAssociations_ScalarSpearman_DeterministicGoldenFixture()
    {
        var sut = new V0Tools();
        var request = BuildScalarSpearmanRequest(topK: 6);
        var first = sut.AnalyzeIndicatorAssociations(request);
        var second = sut.AnalyzeIndicatorAssociations(request);
        var payloadA = Assert.IsType<AnalyzeIndicatorAssociationsResponse>(first);
        var payloadB = Assert.IsType<AnalyzeIndicatorAssociationsResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("1.0.0", payloadA.ToolVersion);
        Assert.Equal("spearman", payloadA.Method);
        Assert.Equal("spearman", payloadA.AssociationMagnitude.Method);
        Assert.Equal(5, payloadA.Window.Size);
        Assert.Equal(1001, payloadA.Window.StartContestId);
        Assert.Equal(1005, payloadA.Window.EndContestId);
        Assert.Equal(6, payloadA.AssociationMagnitude.TopPairs.Count);

        // Valores dourados (synthetic_min_window, janela 5 -> 1001..1005), Spearman via ranks+Pearson
        var top = payloadA.AssociationMagnitude.TopPairs[0];
        Assert.Equal("quantidade_vizinhos_por_concurso", top.IndicatorA, StringComparer.Ordinal);
        Assert.Equal("identity", top.AggregationA, StringComparer.Ordinal);
        Assert.Null(top.ComponentIndexA);
        Assert.Equal("sequencia_maxima_vizinhos_por_concurso", top.IndicatorB, StringComparer.Ordinal);
        Assert.Equal("identity", top.AggregationB, StringComparer.Ordinal);
        Assert.Null(top.ComponentIndexB);
        Assert.Equal(0.80295506854696608, top.AssociationStrength, precision: 12);

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("association_magnitude", out _));
    }

    [Fact]
    public void AnalyzeIndicatorAssociations_VectorWithoutAggregation_ReturnsUnsupportedAggregation()
    {
        var sut = new V0Tools();
        var request = new AnalyzeIndicatorAssociationsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Items:
            [
                new AssociationItemRequest("repeticao_concurso_anterior", null),
                new AssociationItemRequest("distribuicao_linha_por_concurso", null)
            ],
            Method: "spearman",
            TopK: 3,
            StabilityCheck: null);

        var response = sut.AnalyzeIndicatorAssociations(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_AGGREGATION", error.Code);
    }

    [Fact]
    public void AnalyzeIndicatorAssociations_WithStabilityCheckInUnsupportedBuild_ReturnsUnsupportedStabilityCheck()
    {
        var sut = new V0Tools();
        var request = new AnalyzeIndicatorAssociationsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Items:
            [
                new AssociationItemRequest("repeticao_concurso_anterior", null),
                new AssociationItemRequest("pares_no_concurso", null)
            ],
            Method: "spearman",
            TopK: 3,
            StabilityCheck: new
            {
                method = "rolling_window",
                subwindow_size = 3
            });

        var response = sut.AnalyzeIndicatorAssociations(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_STABILITY_CHECK", error.Code);
    }
}

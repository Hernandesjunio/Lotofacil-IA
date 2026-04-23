using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase21Adr0006GapsContractTests
{
    [Fact]
    public void GapB_ExplainReferencesRepetitionMetricWhileComputeRouteStillRejectsIt()
    {
        var sut = new V0Tools();
        var explainRequest = new ExplainCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Games:
            [
                [1, 3, 4, 5, 7, 8, 10, 11, 13, 15, 17, 18, 20, 22, 24]
            ],
            IncludeMetricBreakdown: true,
            IncludeExclusionBreakdown: true);

        var explainResponse = sut.ExplainCandidateGames(explainRequest);
        var explainPayload = Assert.IsType<ExplainCandidateGamesResponse>(explainResponse);
        var explanation = Assert.Single(explainPayload.Explanations);
        var commonRepetitionStrategy = explanation.CandidateStrategies
            .Single(strategy => string.Equals(strategy.StrategyName, "common_repetition_frequency", StringComparison.Ordinal));

        // Congela o GAP B do ADR 0006: a explicação/estratégia usa repetição, mas a rota de compute ainda não promoveu essa métrica.
        Assert.Contains(
            commonRepetitionStrategy.MetricBreakdown,
            metric => string.Equals(metric.MetricName, "repeticao_concurso_anterior", StringComparison.Ordinal));

        var computeRequest = new ComputeWindowMetricsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Metrics: [new MetricRequest("repeticao_concurso_anterior")]);

        var computeResponse = sut.ComputeWindowMetrics(computeRequest);
        var computeError = Assert.IsType<ContractErrorEnvelope>(computeResponse).Error;
        Assert.Equal("UNKNOWN_METRIC", computeError.Code);
        Assert.Equal("repeticao_concurso_anterior", computeError.Details["metric_name"]);

        var allowedMetrics = Assert.IsType<string[]>(computeError.Details["allowed_metrics"]);
        Assert.Contains("frequencia_por_dezena", allowedMetrics);
        Assert.DoesNotContain("repeticao_concurso_anterior", allowedMetrics);
    }

    [Fact]
    public void GapE_PairsAndRowEntropySpearman_IsDeterministicAndMatchesGoldenMagnitude()
    {
        var sut = new V0Tools();
        var request = new AnalyzeIndicatorAssociationsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Items:
            [
                new AssociationItemRequest("pares_no_concurso", null),
                new AssociationItemRequest("entropia_linha_por_concurso", null)
            ],
            Method: "spearman",
            TopK: 1,
            StabilityCheck: null);

        var first = sut.AnalyzeIndicatorAssociations(request);
        var second = sut.AnalyzeIndicatorAssociations(request);
        var payloadA = Assert.IsType<AnalyzeIndicatorAssociationsResponse>(first);
        var payloadB = Assert.IsType<AnalyzeIndicatorAssociationsResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("spearman", payloadA.Method);
        Assert.Equal("spearman", payloadA.AssociationMagnitude.Method);

        var topPair = Assert.Single(payloadA.AssociationMagnitude.TopPairs);
        Assert.Equal("pares_no_concurso", topPair.IndicatorA, StringComparer.Ordinal);
        Assert.Equal("entropia_linha_por_concurso", topPair.IndicatorB, StringComparer.Ordinal);
        Assert.Equal("identity", topPair.AggregationA, StringComparer.Ordinal);
        Assert.Equal("identity", topPair.AggregationB, StringComparer.Ordinal);

        var golden = LoadGapEPairsEntropyGolden();
        Assert.Equal(golden.DatasetVersion, payloadA.DatasetVersion);
        Assert.Equal(golden.Method, payloadA.Method);
        var magnitudeDelta = Math.Abs(topPair.AssociationStrength - golden.ExpectedAssociationStrength);
        Assert.True(
            magnitudeDelta <= golden.MagnitudeTolerance,
            $"Magnitude Spearman fora da tolerancia fixa. Esperado={golden.ExpectedAssociationStrength}, obtido={topPair.AssociationStrength}, delta={magnitudeDelta}, tolerancia={golden.MagnitudeTolerance}.");
    }

    private static GapEPairsEntropyGolden LoadGapEPairsEntropyGolden()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var goldenPath = Path.Combine(
            repositoryRoot,
            "tests",
            "fixtures",
            "golden",
            "phase21",
            "gap-e-pares-entropia-spearman.synthetic_min_window.golden.json");

        using var json = JsonDocument.Parse(File.ReadAllText(goldenPath));
        var root = json.RootElement;
        return new GapEPairsEntropyGolden(
            DatasetVersion: root.GetProperty("dataset_version").GetString()!,
            Method: root.GetProperty("method").GetString()!,
            ExpectedAssociationStrength: root.GetProperty("expected_association_strength").GetDouble(),
            MagnitudeTolerance: root.GetProperty("magnitude_tolerance").GetDouble());
    }

    private sealed record GapEPairsEntropyGolden(
        string DatasetVersion,
        string Method,
        double ExpectedAssociationStrength,
        double MagnitudeTolerance);
}

using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase20ExplainCandidateGamesContractTests
{
    private static ExplainCandidateGamesRequest BuildGoldenRequest() => new(
        WindowSize: 5,
        EndContestId: 1005,
        Games:
        [
            [1, 3, 4, 5, 7, 8, 10, 11, 13, 15, 17, 18, 20, 22, 24]
        ],
        IncludeMetricBreakdown: true,
        IncludeExclusionBreakdown: true);

    [Fact]
    public void ExplainCandidateGames_EmptyGames_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new ExplainCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Games: [],
            IncludeMetricBreakdown: true,
            IncludeExclusionBreakdown: true);

        var response = sut.ExplainCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void ExplainCandidateGames_GoldenRequest_IsDeterministicAndContainsTraceableBreakdown()
    {
        var sut = new V0Tools();
        var request = BuildGoldenRequest();
        var first = sut.ExplainCandidateGames(request);
        var second = sut.ExplainCandidateGames(request);
        var payloadA = Assert.IsType<ExplainCandidateGamesResponse>(first);
        var payloadB = Assert.IsType<ExplainCandidateGamesResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("1.1.0", payloadA.ToolVersion);
        var gameExplanation = Assert.Single(payloadA.Explanations);
        Assert.Equal(15, gameExplanation.Game.Count);
        Assert.NotEmpty(gameExplanation.CandidateStrategies);

        var ordered = gameExplanation.CandidateStrategies
            .Select(item => item.Score)
            .ToArray();
        var expected = ordered.OrderByDescending(value => value).ToArray();
        Assert.Equal(expected, ordered);

        var firstStrategy = gameExplanation.CandidateStrategies[0];
        Assert.False(string.IsNullOrWhiteSpace(firstStrategy.StrategyName));
        Assert.False(string.IsNullOrWhiteSpace(firstStrategy.StrategyVersion));
        Assert.NotEmpty(firstStrategy.MetricBreakdown);
        Assert.NotEmpty(firstStrategy.ExclusionBreakdown);
        Assert.NotEmpty(firstStrategy.ConstraintBreakdown);
        Assert.All(firstStrategy.MetricBreakdown, metric => Assert.False(string.IsNullOrWhiteSpace(metric.MetricVersion)));
        Assert.All(firstStrategy.ExclusionBreakdown, exclusion => Assert.False(string.IsNullOrWhiteSpace(exclusion.ExclusionVersion)));

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("window", out _));
        Assert.True(root.TryGetProperty("explanations", out var explanations));
        Assert.Equal(JsonValueKind.Array, explanations.ValueKind);

        var explanation = explanations[0];
        Assert.True(explanation.TryGetProperty("game", out _));
        Assert.True(explanation.TryGetProperty("candidate_strategies", out var candidateStrategies));
        Assert.Equal(JsonValueKind.Array, candidateStrategies.ValueKind);

        var strategy = candidateStrategies[0];
        Assert.True(strategy.TryGetProperty("metric_breakdown", out var metricBreakdown));
        Assert.Equal(JsonValueKind.Array, metricBreakdown.ValueKind);
        Assert.True(strategy.TryGetProperty("exclusion_breakdown", out var exclusionBreakdown));
        Assert.Equal(JsonValueKind.Array, exclusionBreakdown.ValueKind);
        Assert.True(strategy.TryGetProperty("constraint_breakdown", out var constraintBreakdown));
        Assert.Equal(JsonValueKind.Array, constraintBreakdown.ValueKind);

        var firstConstraint = constraintBreakdown[0];
        Assert.True(firstConstraint.TryGetProperty("kind", out _));
        Assert.True(firstConstraint.TryGetProperty("name", out _));
        Assert.True(firstConstraint.TryGetProperty("mode", out _));
        Assert.True(firstConstraint.TryGetProperty("observed_value", out _));
        Assert.True(firstConstraint.TryGetProperty("applied", out var applied));
        Assert.Equal(JsonValueKind.Object, applied.ValueKind);
        Assert.True(firstConstraint.TryGetProperty("result", out var result));
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        Assert.True(result.TryGetProperty("passed", out _));
        Assert.True(result.TryGetProperty("penalty", out _));
    }
}

using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase20GenerateCandidateGamesContractTests
{
    private static GenerateCandidateGamesRequest BuildGoldenRequest() => new(
        WindowSize: 5,
        EndContestId: 1005,
        Seed: 424242UL,
        Plan:
        [
            new GenerateCandidatePlanItemRequest(
                StrategyName: "common_repetition_frequency",
                Count: 2,
                SearchMethod: null)
        ]);

    [Fact]
    public void GenerateCandidateGames_MissingSeedForGreedyTopK_ReturnsNonDeterministicConfiguration()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: null,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 1,
                    SearchMethod: null)
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("NON_DETERMINISTIC_CONFIGURATION", error.Code);
    }

    [Fact]
    public void GenerateCandidateGames_GoldenRequest_IsDeterministicAndContainsLineage()
    {
        var sut = new V0Tools();
        var request = BuildGoldenRequest();
        var first = sut.GenerateCandidateGames(request);
        var second = sut.GenerateCandidateGames(request);
        var payloadA = Assert.IsType<GenerateCandidateGamesResponse>(first);
        var payloadB = Assert.IsType<GenerateCandidateGamesResponse>(second);

        Assert.Equal(payloadA.DeterministicHash, payloadB.DeterministicHash);
        Assert.Equal("1.0.0", payloadA.ToolVersion);
        Assert.Equal(5, payloadA.Window.Size);
        Assert.Equal(1001, payloadA.Window.StartContestId);
        Assert.Equal(1005, payloadA.Window.EndContestId);
        Assert.Equal(2, payloadA.CandidateGames.Count);

        var firstGame = payloadA.CandidateGames[0];
        Assert.Equal("common_repetition_frequency", firstGame.StrategyName, StringComparer.Ordinal);
        Assert.Equal("1.0.0", firstGame.StrategyVersion, StringComparer.Ordinal);
        Assert.Equal("greedy_topk", firstGame.SearchMethod, StringComparer.Ordinal);
        Assert.Equal("lexicographic_numbers_asc", firstGame.TieBreakRule, StringComparer.Ordinal);
        Assert.Equal(424242UL, firstGame.SeedUsed);
        Assert.Equal(15, firstGame.Numbers.Count);

        using var json = JsonSerializer.SerializeToDocument(payloadA);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));
        Assert.True(root.TryGetProperty("window", out _));
        Assert.True(root.TryGetProperty("candidate_games", out var games));
        Assert.Equal(JsonValueKind.Array, games.ValueKind);

        var firstGameJson = games[0];
        Assert.True(firstGameJson.TryGetProperty("strategy_name", out _));
        Assert.True(firstGameJson.TryGetProperty("strategy_version", out _));
        Assert.True(firstGameJson.TryGetProperty("search_method", out _));
        Assert.True(firstGameJson.TryGetProperty("tie_break_rule", out _));
        Assert.True(firstGameJson.TryGetProperty("seed_used", out _));
    }
}

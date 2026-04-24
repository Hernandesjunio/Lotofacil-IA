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
                SearchMethod: null),
            new GenerateCandidatePlanItemRequest(
                StrategyName: "declared_composite_profile",
                Count: 2,
                SearchMethod: "sampled",
                Weights:
                [
                    new GenerateCandidateWeightRequest("freq_alignment", 0.4),
                    new GenerateCandidateWeightRequest("repeat_alignment", 0.2),
                    new GenerateCandidateWeightRequest("slot_alignment", 0.2),
                    new GenerateCandidateWeightRequest("row_entropy_norm", 0.1),
                    new GenerateCandidateWeightRequest("hhi_linha_inverse", 0.1)
                ])
        ],
        StructuralExclusions: new GenerateStructuralExclusionsRequest(
            MaxConsecutiveRun: 8,
            MaxNeighborCount: 7,
            MinRowEntropyNorm: 0.82,
            MaxHhiLinha: 0.30,
            RepeatRange: new GenerateRepeatRangeRequest(0, 15),
            MinSlotAlignment: 0.08,
            MaxOutlierScore: 1.0));

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
    public void GenerateCandidateGames_GoldenRequest_IsDeterministicAndIncludesAppliedConfiguration()
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
        Assert.Equal(4, payloadA.CandidateGames.Count);

        Assert.Contains(payloadA.CandidateGames, game => game.StrategyName == "common_repetition_frequency");
        Assert.Contains(payloadA.CandidateGames, game => game.StrategyName == "declared_composite_profile");

        var firstGame = payloadA.CandidateGames[0];
        Assert.NotNull(firstGame.AppliedConfiguration);
        Assert.NotEmpty(firstGame.AppliedConfiguration.Filters);
        Assert.NotEmpty(firstGame.AppliedConfiguration.ResolvedDefaults);

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
        Assert.True(firstGameJson.TryGetProperty("applied_configuration", out var appliedConfiguration));
        Assert.True(appliedConfiguration.TryGetProperty("resolved_defaults", out _));
    }

    [Fact]
    public void GenerateCandidateGames_ConfigurableFilters_AreAppliedDuringGeneration()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 123UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 1,
                    SearchMethod: "greedy_topk",
                    Filters:
                    [
                        new GenerateCandidateFilterRequest("max_neighbor_count", 10),
                        new GenerateCandidateFilterRequest("max_consecutive_run", 15)
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        Assert.Single(payload.CandidateGames);

        foreach (var game in payload.CandidateGames)
        {
            Assert.True(CountNeighbors(game.Numbers) <= 10);
            Assert.True(MaxConsecutiveRun(game.Numbers) <= 15);
            Assert.Contains(game.AppliedConfiguration.Filters, filter => filter.Name == "max_neighbor_count" && filter.Value == 10);
            Assert.Contains(game.AppliedConfiguration.Filters, filter => filter.Name == "max_consecutive_run" && filter.Value == 15);
        }
    }

    private static int CountNeighbors(IReadOnlyList<int> game)
    {
        var count = 0;
        for (var i = 1; i < game.Count; i++)
        {
            if (game[i] - game[i - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }

    private static int MaxConsecutiveRun(IReadOnlyList<int> game)
    {
        var best = 1;
        var run = 1;
        for (var i = 1; i < game.Count; i++)
        {
            if (game[i] - game[i - 1] == 1)
            {
                run++;
                if (run > best)
                {
                    best = run;
                }
            }
            else
            {
                run = 1;
            }
        }

        return best;
    }
}

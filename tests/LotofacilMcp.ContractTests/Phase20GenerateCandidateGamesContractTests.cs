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

    [Fact]
    public void GenerateCandidateGames_UniqueGamesTrue_DoesNotReturnDuplicates()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 10,
                    SearchMethod: "sampled")
            ],
            GlobalConstraints: new GenerateGlobalConstraintsRequest(
                UniqueGames: true,
                SortedNumbers: true),
            StructuralExclusions: new GenerateStructuralExclusionsRequest(
                MaxConsecutiveRun: 15,
                MaxNeighborCount: 15,
                MinRowEntropyNorm: 0.0,
                MaxHhiLinha: 1.0,
                RepeatRange: new GenerateRepeatRangeRequest(0, 15),
                MinSlotAlignment: 0.0,
                MaxOutlierScore: 1.0));

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        Assert.Equal(10, payload.CandidateGames.Count);

        var keys = payload.CandidateGames.Select(game => string.Join(",", game.Numbers)).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void GenerateCandidateGames_DifferentSeeds_ProduceDifferentDeterministicHash()
    {
        var sut = new V0Tools();

        var requestA = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 1UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 3,
                    SearchMethod: "sampled")
            ]);

        var requestB = requestA with { Seed = 2UL };

        var responseA = sut.GenerateCandidateGames(requestA);
        var responseB = sut.GenerateCandidateGames(requestB);
        var payloadA = Assert.IsType<GenerateCandidateGamesResponse>(responseA);
        var payloadB = Assert.IsType<GenerateCandidateGamesResponse>(responseB);

        Assert.NotEqual(payloadA.DeterministicHash, payloadB.DeterministicHash);
    }

    [Fact]
    public void GenerateCandidateGames_MixedCriterionModes_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "declared_composite_profile",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "repeat_count",
                            Value: 8,
                            Range: new GenerateRangeSpecRequest(7, 9))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void GenerateCandidateGames_EmptyAllowedValues_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "declared_composite_profile",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "neighbors_count",
                            AllowedValues: new GenerateAllowedValuesSpecRequest(Array.Empty<double>()))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void GenerateCandidateGames_NonIntegerAllowedValuesForIntegerConstraint_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "declared_composite_profile",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "neighbors_count",
                            AllowedValues: new GenerateAllowedValuesSpecRequest([8.5, 9]))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void GenerateCandidateGames_AllowedValuesAreNormalized_AndDefaultModeIsResolved()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "declared_composite_profile",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "neighbors_count",
                            AllowedValues: new GenerateAllowedValuesSpecRequest([10, 8, 10, 9]))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        Assert.Single(payload.CandidateGames);

        using var json = JsonSerializer.SerializeToDocument(payload);
        var resolvedDefaults = json.RootElement
            .GetProperty("candidate_games")[0]
            .GetProperty("applied_configuration")
            .GetProperty("resolved_defaults");

        Assert.True(resolvedDefaults.TryGetProperty("plan[0].criteria[0].mode", out var mode));
        Assert.Equal("hard", mode.GetString());

        Assert.True(resolvedDefaults.TryGetProperty("plan[0].criteria[0].allowed_values.values", out var allowed));
        var normalized = allowed.EnumerateArray().Select(static item => item.GetDouble()).ToArray();
        Assert.Equal([8d, 9d, 10d], normalized);
    }

    [Fact]
    public void GenerateCandidateGames_TypicalRangeIqr_ResolvesAndEchoesCoverageObservedMethodVersion()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "min_top10_overlap",
                            TypicalRange: new GenerateTypicalRangeSpecRequest(
                                MetricName: "repeticao_concurso_anterior",
                                Method: "iqr",
                                Coverage: 0.8))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        Assert.Single(payload.CandidateGames);

        using var json = JsonSerializer.SerializeToDocument(payload);
        var resolvedDefaults = json.RootElement
            .GetProperty("candidate_games")[0]
            .GetProperty("applied_configuration")
            .GetProperty("resolved_defaults");

        Assert.True(resolvedDefaults.TryGetProperty("plan[0].criteria[0].typical_range.resolved_range", out var resolvedRange));
        Assert.True(resolvedRange.TryGetProperty("min", out _));
        Assert.True(resolvedRange.TryGetProperty("max", out _));
        Assert.True(resolvedRange.TryGetProperty("inclusive", out var inclusive));
        Assert.True(inclusive.GetBoolean());

        Assert.True(resolvedDefaults.TryGetProperty("plan[0].criteria[0].typical_range.coverage_observed", out var coverageObserved));
        Assert.InRange(coverageObserved.GetDouble(), 0d, 1d);

        Assert.True(resolvedDefaults.TryGetProperty("plan[0].criteria[0].typical_range.method_version", out var methodVersion));
        Assert.Equal("1.0.0", methodVersion.GetString());
    }

    [Fact]
    public void GenerateCandidateGames_TypicalRangeCoverageOutsideRange_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "min_top10_overlap",
                            TypicalRange: new GenerateTypicalRangeSpecRequest(
                                MetricName: "repeticao_concurso_anterior",
                                Method: "iqr",
                                Coverage: 1.2))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void GenerateCandidateGames_TypicalRangePercentileWithInvalidParams_ReturnsInvalidRequest()
    {
        var sut = new V0Tools();
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 424242UL,
            Plan:
            [
                new GenerateCandidatePlanItemRequest(
                    StrategyName: "common_repetition_frequency",
                    Count: 1,
                    SearchMethod: "sampled",
                    Criteria:
                    [
                        new GenerateCandidateCriterionRequest(
                            Name: "min_top10_overlap",
                            TypicalRange: new GenerateTypicalRangeSpecRequest(
                                MetricName: "repeticao_concurso_anterior",
                                Method: "percentile",
                                Coverage: 0.8,
                                Params: new GenerateTypicalRangeParamsRequest(0.9, 0.1)))
                    ])
            ]);

        var response = sut.GenerateCandidateGames(request);
        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
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

using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// ADR 0020 D1–D2: modo explícito vs legado para <c>structural_exclusions</c> e eco em <c>resolved_defaults</c>.
/// </summary>
public sealed class Phase20GenerationMode0020ContractTests
{
    private static GenerateCandidatePlanItemRequest SampledPlanItem(int count = 1) =>
        new(
            StrategyName: "common_repetition_frequency",
            Count: count,
            SearchMethod: "sampled");

    [Fact]
    public void GenerateCandidateGames_LegacyImplicitMode_AppliesConservativeStructuralDefaultsInFilters()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 100001UL,
            Plan: [SampledPlanItem()],
            StructuralExclusions: null,
            GenerationMode: null);

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        var game = Assert.Single(payload.CandidateGames);
        Assert.Contains(
            game.AppliedConfiguration.Filters,
            f => f.Name == "max_neighbor_count" && f.Value == 7);
        Assert.True(game.AppliedConfiguration.ResolvedDefaults.TryGetValue("generation_mode", out var mode));
        Assert.Equal("legacy_implicit_structural_defaults", mode);
        Assert.Equal("legacy_conservative_defaults_fill", game.AppliedConfiguration.ResolvedDefaults["structural_exclusions.policy"]);
    }

    [Fact]
    public void GenerateCandidateGames_RandomUnrestricted_WithoutStructural_DoesNotInjectConservativeStructuralFilters()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 100001UL,
            Plan: [SampledPlanItem()],
            StructuralExclusions: null,
            GenerationMode: "random_unrestricted");

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        var game = Assert.Single(payload.CandidateGames);
        Assert.DoesNotContain(game.AppliedConfiguration.Filters, f => f.Name == "max_neighbor_count");
        Assert.DoesNotContain(game.AppliedConfiguration.Filters, f => f.Name == "min_row_entropy_norm");
        Assert.Equal("random_unrestricted", game.AppliedConfiguration.ResolvedDefaults["generation_mode"]);
        Assert.Equal("explicit_opt_in_only", game.AppliedConfiguration.ResolvedDefaults["structural_exclusions.policy"]);
        Assert.Empty(Assert.IsType<string[]>(game.AppliedConfiguration.ResolvedDefaults["structural_exclusions.active_constraint_keys"]));
    }

    [Fact]
    public void GenerateCandidateGames_BehaviorFiltered_DeclaresOnlyNeighborCap_AppliesThatFilterAndEchoesResolvedDefaults()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        const int neighborCap = 10;
        var request = new GenerateCandidateGamesRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Seed: 100002UL,
            Plan: [SampledPlanItem()],
            StructuralExclusions: new GenerateStructuralExclusionsRequest(MaxNeighborCount: neighborCap),
            GenerationMode: "behavior_filtered");

        var response = sut.GenerateCandidateGames(request);
        var payload = Assert.IsType<GenerateCandidateGamesResponse>(response);
        var game = Assert.Single(payload.CandidateGames);
        Assert.Single(game.AppliedConfiguration.Filters);
        Assert.Contains(game.AppliedConfiguration.Filters, f => f.Name == "max_neighbor_count" && f.Value == neighborCap);
        Assert.True(CountNeighbors(game.Numbers) <= neighborCap);
        Assert.Equal("behavior_filtered", game.AppliedConfiguration.ResolvedDefaults["generation_mode"]);
        Assert.Equal("explicit_opt_in_only", game.AppliedConfiguration.ResolvedDefaults["structural_exclusions.policy"]);
        var active = Assert.IsType<string[]>(game.AppliedConfiguration.ResolvedDefaults["structural_exclusions.active_constraint_keys"]);
        Assert.Equal(["max_neighbor_count"], active);
        Assert.True(game.AppliedConfiguration.ResolvedDefaults.ContainsKey("structural_exclusions.resolved_effective"));
    }

    private static int CountNeighbors(IReadOnlyList<int> sortedNumbers)
    {
        var count = 0;
        for (var i = 1; i < sortedNumbers.Count; i++)
        {
            if (sortedNumbers[i] - sortedNumbers[i - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }
}

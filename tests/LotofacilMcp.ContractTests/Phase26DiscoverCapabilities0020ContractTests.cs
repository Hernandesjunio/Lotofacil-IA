using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase26DiscoverCapabilities0020ContractTests
{
    [Fact]
    public void DiscoverCapabilities_GenerationSurface_MatchesRequestLimits_And_SeedByPath_Policy()
    {
        var sut = new V0Tools();
        var a = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));
        var b = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));

        Assert.Equal(a.DeterministicHash, b.DeterministicHash);
        Assert.Equal(GenerationRequestLimits.MaxSumPlanCountPerRequest, a.Generation.MaxSumPlanCountPerRequest);
        Assert.True(a.Generation.RequestSeedOptional);
        Assert.Equal(
            [GenerationModes.RandomUnrestricted, GenerationModes.BehaviorFiltered],
            a.Generation.SupportedGenerationModes);

        Assert.Contains(
            a.Generation.SeedBySearchMethod,
            row => row.SearchMethod is "sampled" && row.SeedRequiredForReplayGuaranteed);
        Assert.Contains(
            a.Generation.SeedBySearchMethod,
            row => row.SearchMethod is "greedy_topk" && row.SeedRequiredForReplayGuaranteed);
        Assert.Contains(
            a.Generation.SeedBySearchMethod,
            row => row.SearchMethod is "exhaustive" && !row.SeedRequiredForReplayGuaranteed);
        Assert.Equal(3, a.Generation.SeedBySearchMethod.Count);
    }

    [Fact]
    public void GenerateCandidateGames_ToolDiscovery_Includes_GenerationMode_Enums()
    {
        var sut = new V0Tools();
        var discover = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));
        var gen = Assert.Single(discover.Tools, t => t.Name is "generate_candidate_games");
        var modes = gen.SupportedParameters["generation_mode"];
        Assert.Contains(GenerationModes.RandomUnrestricted, modes);
        Assert.Contains(GenerationModes.BehaviorFiltered, modes);
    }
}

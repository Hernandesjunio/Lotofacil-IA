using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public class V0Phase2ContractRedTests
{
    [Fact]
    public void ComputeWindowMetrics_WithoutMetrics_ReturnsInvalidRequestContractError()
    {
        var sut = new V0Tools();
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: null);

        var response = sut.ComputeWindowMetrics(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
        Assert.Equal("metrics", error.Details["missing_field"]);
    }

    [Fact]
    public void ComputeWindowMetrics_WithUnknownMetric_ReturnsUnknownMetricError()
    {
        var sut = new V0Tools();
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequest("metrica_inexistente")]);

        var response = sut.ComputeWindowMetrics(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("metrica_inexistente", error.Details["metric_name"]);
    }

    [Fact]
    public void GetDrawWindow_ReturnsDrawsInAscendingContestOrder()
    {
        var fixture = LoadSyntheticFixture();
        var expectedIds = fixture.Select(d => d.ContestId).OrderBy(id => id).ToArray();
        var sut = new V0Tools();
        var request = new GetDrawWindowRequest(WindowSize: 4, EndContestId: 1004);

        var response = sut.GetDrawWindow(request);

        var window = Assert.IsType<GetDrawWindowResponse>(response);
        Assert.Equal(expectedIds, window.Draws.Select(d => d.ContestId).ToArray());
    }

    private static IReadOnlyList<FixtureDraw> LoadSyntheticFixture()
    {
        var fixturePath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
        var json = File.ReadAllText(fixturePath);
        var fixture = JsonSerializer.Deserialize<FixtureRoot>(json);

        Assert.NotNull(fixture);
        Assert.NotNull(fixture.Draws);

        return fixture.Draws;
    }

    private sealed record FixtureRoot(
        [property: JsonPropertyName("draws")] IReadOnlyList<FixtureDraw> Draws);

    private sealed record FixtureDraw(
        [property: JsonPropertyName("contest_id")] int ContestId,
        [property: JsonPropertyName("draw_date")] string DrawDate,
        [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);
}

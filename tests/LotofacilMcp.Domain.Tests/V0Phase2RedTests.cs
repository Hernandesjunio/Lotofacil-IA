using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Normalization;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public class V0Phase2RedTests
{
    [Fact]
    public void DrawNormalizationBarrier_NormalizesPotentiallyNonCanonicalInput()
    {
        var raw = new RawDraw(
            ContestId: 7001,
            DrawDate: "2026-02-01",
            Numbers: [15, 4, 10, 8, 6, 14, 1, 2, 11, 5, 3, 12, 9, 13, 7]);

        var sut = new DrawNormalizer();

        var normalized = sut.Normalize(raw);

        Assert.Equal(7001, normalized.ContestId);
        Assert.Equal(new DateOnly(2026, 2, 1), normalized.DrawDate);
        Assert.Equal(Enumerable.Range(1, 15).ToArray(), normalized.Numbers);
    }

    [Fact]
    public void WindowResolution_UsesWindowSizeAndEndContestId()
    {
        var draws = LoadSyntheticFixture()
            .Select(ToDomainDraw)
            .ToList();

        var sut = new WindowResolver();

        var window = sut.Resolve(draws, windowSize: 3, endContestId: 1004);

        Assert.Equal(3, window.Size);
        Assert.Equal(1002, window.StartContestId);
        Assert.Equal(1004, window.EndContestId);
        Assert.Equal([1002, 1003, 1004], window.Draws.Select(d => d.ContestId).ToArray());
    }

    [Fact]
    public void FrequenciaPorDezena_ComputesManualCountsOnSyntheticFixture()
    {
        var draws = LoadSyntheticFixture()
            .Where(d => d.ContestId <= 1003)
            .Select(ToDomainDraw)
            .ToList();
        var window = new DrawWindow(
            Size: 3,
            StartContestId: 1001,
            EndContestId: 1003,
            Draws: draws);
        var sut = new FrequencyByDezenaMetric();

        var metric = sut.Compute(window);

        Assert.Equal("frequencia_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(
            [2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0],
            metric.Value.ToArray());
    }

    [Fact]
    public void FrequenciaPorDezena_HasConservationProperty_15TimesWindowSize()
    {
        var draws = LoadSyntheticFixture()
            .Where(d => d.ContestId <= 1003)
            .Select(ToDomainDraw)
            .ToList();
        var window = new DrawWindow(
            Size: 3,
            StartContestId: 1001,
            EndContestId: 1003,
            Draws: draws);
        var sut = new FrequencyByDezenaMetric();

        var metric = sut.Compute(window);

        Assert.Equal(15 * window.Size, metric.Value.Sum());
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

    private static Draw ToDomainDraw(FixtureDraw fixtureDraw)
    {
        return new Draw(
            fixtureDraw.ContestId,
            DateOnly.Parse(fixtureDraw.DrawDate),
            fixtureDraw.Numbers);
    }

    private sealed record FixtureRoot(
        [property: JsonPropertyName("draws")] IReadOnlyList<FixtureDraw> Draws);

    private sealed record FixtureDraw(
        [property: JsonPropertyName("contest_id")] int ContestId,
        [property: JsonPropertyName("draw_date")] string DrawDate,
        [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);
}

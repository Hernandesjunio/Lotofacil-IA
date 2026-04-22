using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class HhiColunaPorConcursoMetricTests
{
    [Fact]
    public void Serie_OnMinimalFixture_MatchesColumnHistogramHhi()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = new HhiColunaPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("hhi_coluna_por_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, metric.Value.Count);

        Span<int> col = stackalloc int[5];
        var expected = new double[window.Size];
        for (var i = 0; i < window.Size; i++)
        {
            VolanteRowColumnCounts.FillColumnCounts(window.Draws[i], col);
            expected[i] = HerfindahlHirschmanIndex.FromNonNegativeCounts(col);
        }

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], metric.Value[i], 12);
            Assert.False(double.IsNaN(metric.Value[i]));
            Assert.False(double.IsInfinity(metric.Value[i]));
        }
    }

    [Fact]
    public void Draw_OneToFifteen_UniformColumns_MatchesHhi()
    {
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new HhiColunaPorConcursoMetric();

        var metric = sut.Compute(window);

        var h = HerfindahlHirschmanIndex.FromNonNegativeCounts([3, 3, 3, 3, 3]);
        Assert.Equal(h, Assert.Single(metric.Value), 12);
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesHhiColunaPorConcurso()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("hhi_coluna_por_concurso", window);

        Assert.Equal("hhi_coluna_por_concurso", metric.MetricName);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal(3, metric.Value.Count);
    }

    private static DrawWindow BuildWindowFromSyntheticFixture(int endContestIdInclusive)
    {
        var fixturePath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
        var json = File.ReadAllText(fixturePath);
        var fixture = JsonSerializer.Deserialize<FixtureRoot>(json);

        Assert.NotNull(fixture);
        Assert.NotNull(fixture!.Draws);

        var draws = fixture.Draws
            .Where(d => d.ContestId <= endContestIdInclusive)
            .Select(s => new Draw(s.ContestId, DateOnly.Parse(s.DrawDate), s.Numbers))
            .ToList();

        return new DrawWindow(
            Size: draws.Count,
            StartContestId: draws.First().ContestId,
            EndContestId: draws.Last().ContestId,
            Draws: draws);
    }

    private sealed record FixtureRoot(
        [property: JsonPropertyName("draws")] IReadOnlyList<FixtureDraw> Draws);

    private sealed record FixtureDraw(
        [property: JsonPropertyName("contest_id")] int ContestId,
        [property: JsonPropertyName("draw_date")] string DrawDate,
        [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);
}

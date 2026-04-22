using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class EntropiaLinhaPorConcursoMetricTests
{
    [Fact]
    public void ShannonEntropy_FromCounts_AllMassInOneBin_IsZeroBits()
    {
        Assert.Equal(0d, ShannonEntropyBits.FromNonNegativeCounts([15, 0, 0, 0, 0]));
    }

    [Fact]
    public void ShannonEntropy_FromCounts_UniformFiveBins_IsLog2Of5()
    {
        var expected = Math.Log2(5);
        Assert.Equal(expected, ShannonEntropyBits.FromNonNegativeCounts([3, 3, 3, 3, 3]), 12);
    }

    [Fact]
    public void ShannonEntropy_NegativeCount_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            ShannonEntropyBits.FromNonNegativeCounts([16, -1, 0, 0, 0]));
    }

    [Fact]
    public void Serie_OnMinimalFixture_MatchesRowHistogramEntropy()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = new EntropiaLinhaPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("entropia_linha_por_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("bits", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, metric.Value.Count);

        var expected = new[]
        {
            ShannonEntropyBits.FromNonNegativeCounts([3, 3, 3, 3, 3]),
            ShannonEntropyBits.FromNonNegativeCounts([3, 3, 4, 3, 2]),
            ShannonEntropyBits.FromNonNegativeCounts([2, 5, 3, 3, 2])
        };

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], metric.Value[i], 12);
            Assert.False(double.IsNaN(metric.Value[i]));
            Assert.False(double.IsInfinity(metric.Value[i]));
        }
    }

    [Fact]
    public void Draw_OneToFifteen_ThreeRowsFullyFilled_EntropyMatchesHistogram()
    {
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new EntropiaLinhaPorConcursoMetric();

        var metric = sut.Compute(window);

        var h = ShannonEntropyBits.FromNonNegativeCounts([5, 5, 5, 0, 0]);
        Assert.Equal(h, Assert.Single(metric.Value), 12);
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesEntropiaLinhaPorConcurso()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("entropia_linha_por_concurso", window);

        Assert.Equal("entropia_linha_por_concurso", metric.MetricName);
        Assert.Equal("bits", metric.Unit);
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

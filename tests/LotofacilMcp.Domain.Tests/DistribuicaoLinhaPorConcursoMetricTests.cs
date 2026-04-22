using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class DistribuicaoLinhaPorConcursoMetricTests
{
    [Fact]
    public void Serie_OnMinimalFixture_FirstThreeContests_Deterministic()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = new DistribuicaoLinhaPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("distribuicao_linha_por_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series_of_count_vector[5]", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, window.Size);
        Assert.Equal(15, metric.Value.Count);
        // Por concurso: [3,3,3,3,3], [3,3,4,3,2], [2,5,3,3,2] — ver synthetic_min_window (contest_id 1..3)
        Assert.Equal(
            [3, 3, 3, 3, 3, 3, 3, 4, 3, 2, 2, 5, 3, 3, 2],
            metric.Value.ToArray());
        for (var c = 0; c < window.Size; c++)
        {
            var sum = 0;
            for (var r = 0; r < 5; r++)
            {
                sum += metric.Value[c * 5 + r];
            }

            Assert.Equal(15, sum);
        }
    }

    [Fact]
    public void Draw_OneToFifteen_ContiguousFirstThreeRows_15()
    {
        // 1..5 row0 (5), 6..10 row1 (5), 11..15 row2 (5)
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new DistribuicaoLinhaPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal([5, 5, 5, 0, 0], metric.Value.ToArray());
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesDistribuicaoLinhaPorConcurso()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var frequency = new FrequencyByDezenaMetric();
        var sut = new WindowMetricDispatcher(
            frequency,
            new Top10MaisSorteadosMetric(frequency),
            new Top10MenosSorteadosMetric(frequency),
            new ParesNoConcursoMetric(),
            new RepeticaoConcursoAnteriorMetric(),
            new QuantidadeVizinhosPorConcursoMetric(),
            new SequenciaMaximaVizinhosPorConcursoMetric(),
            new DistribuicaoLinhaPorConcursoMetric());

        var metric = sut.Dispatch("distribuicao_linha_por_concurso", window);

        Assert.Equal("distribuicao_linha_por_concurso", metric.MetricName);
        Assert.Equal("series_of_count_vector[5]", metric.Shape);
        Assert.Equal(
            [3, 3, 3, 3, 3, 3, 3, 4, 3, 2, 2, 5, 3, 3, 2],
            metric.Value.ToArray());
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

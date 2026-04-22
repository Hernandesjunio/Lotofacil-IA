using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

/// <summary>
/// Série escalar <c>sequencia_maxima_vizinhos_por_concurso@1.0.0</c> (catálogo, test-plan séries por concurso).
/// </summary>
public sealed class SequenciaMaximaVizinhosPorConcursoMetricTests
{
    [Fact]
    public void VizinhosConsecutivos_ListaSemParesDiferençaUm_RunMáximaUnidade()
    {
        // test-plan (borda): run unitária — maior bloco = 1 dezena (nenhum par (d, d+1) na sequência)
        Assert.Equal(1, VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(new[] { 1, 3, 5, 7, 9 }));
    }

    [Fact]
    public void Serie_OnMinimalFixture_FirstThreeContests_Deterministic()
    {
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = new SequenciaMaximaVizinhosPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("sequencia_maxima_vizinhos_por_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, window.Size);
        Assert.Equal([3, 4, 7], metric.Value.ToArray());
    }

    [Fact]
    public void Draw_ContiguousBlock1To15_MaxRun15()
    {
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new SequenciaMaximaVizinhosPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal(15, metric.Value[0]);
    }

    [Fact]
    public void Draw_OnlyIsolatedNeighborPairs_MaxRun2()
    {
        // Borda: maior run mínima não trivial (apenas pares isolados, sem trechos de 3+ consecutivos)
        var numbers = new[] { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 17, 19, 20, 22 };
        var draw = new Draw(1, new DateOnly(2026, 1, 1), numbers);
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new SequenciaMaximaVizinhosPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal(2, metric.Value[0]);
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesSequenciaMaximaVizinhosPorConcurso()
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
            new SequenciaMaximaVizinhosPorConcursoMetric());

        var metric = sut.Dispatch("sequencia_maxima_vizinhos_por_concurso", window);

        Assert.Equal("sequencia_maxima_vizinhos_por_concurso", metric.MetricName);
        Assert.Equal([3, 4, 7], metric.Value.ToArray());
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

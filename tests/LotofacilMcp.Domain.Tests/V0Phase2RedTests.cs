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

        var window = sut.Resolve(draws, windowSize: 3, endContestId: 4);

        Assert.Equal(3, window.Size);
        Assert.Equal(2, window.StartContestId);
        Assert.Equal(4, window.EndContestId);
        Assert.Equal([2, 3, 4], window.Draws.Select(d => d.ContestId).ToArray());
        Assert.NotNull(window.PrecedingDraw);
        Assert.Equal(1, window.PrecedingDraw!.ContestId);
    }

    [Fact]
    public void FrequenciaPorDezena_ComputesManualCountsOnSyntheticFixture()
    {
        var draws = LoadSyntheticFixture()
            .Where(d => d.ContestId <= 3)
            .Select(ToDomainDraw)
            .ToList();
        var window = new DrawWindow(
            Size: 3,
            StartContestId: 1,
            EndContestId: 3,
            Draws: draws);
        var sut = new FrequencyByDezenaMetric();

        var metric = sut.Compute(window);

        Assert.Equal("frequencia_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(
            [2, 1, 1, 2, 2, 3, 2, 1, 3, 2, 3, 2, 2, 2, 1, 3, 1, 1, 1, 3, 0, 0, 3, 3, 1],
            metric.Value.ToArray());
    }

    [Fact]
    public void FrequenciaPorDezena_HasConservationProperty_15TimesWindowSize()
    {
        var draws = LoadSyntheticFixture()
            .Where(d => d.ContestId <= 3)
            .Select(ToDomainDraw)
            .ToList();
        var window = new DrawWindow(
            Size: 3,
            StartContestId: 1,
            EndContestId: 3,
            Draws: draws);
        var sut = new FrequencyByDezenaMetric();

        var metric = sut.Compute(window);

        Assert.Equal(15 * window.Size, metric.Value.Sum());
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesByCanonicalMetricName()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("frequencia_por_dezena", window);

        Assert.Equal("frequencia_por_dezena", metric.MetricName);
        Assert.Equal(25, metric.Value.Count);
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesTop10MaisSorteados()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("top10_mais_sorteados", window);

        Assert.Equal("top10_mais_sorteados", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("dezena_list[10]", metric.Shape);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal([6, 9, 11, 16, 20, 23, 24, 1, 4, 5], metric.Value.ToArray());
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesTop10MenosSorteados()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("top10_menos_sorteados", window);

        Assert.Equal("top10_menos_sorteados", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("dezena_list[10]", metric.Shape);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal([21, 22, 2, 3, 8, 15, 17, 18, 19, 25], metric.Value.ToArray());
    }

    [Fact]
    public void Top10MaisSorteados_TieBreakUsesAscendingDezenaWhenFrequenciesMatch()
    {
        var draw = new Draw(
            1,
            new DateOnly(2026, 1, 1),
            Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(
            Size: 1,
            StartContestId: 1,
            EndContestId: 1,
            Draws: [draw]);
        var sut = new Top10MaisSorteadosMetric(new FrequencyByDezenaMetric());

        var metric = sut.Compute(window);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], metric.Value.ToArray());
    }

    [Fact]
    public void Top10MenosSorteados_TieBreakUsesAscendingDezenaWhenFrequenciesMatch()
    {
        var draw = new Draw(
            1,
            new DateOnly(2026, 1, 1),
            Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(
            Size: 1,
            StartContestId: 1,
            EndContestId: 1,
            Draws: [draw]);
        var sut = new Top10MenosSorteadosMetric(new FrequencyByDezenaMetric());

        var metric = sut.Compute(window);

        Assert.Equal([16, 17, 18, 19, 20, 21, 22, 23, 24, 25], metric.Value.ToArray());
    }

    [Fact]
    public void ParesNoConcurso_OnMinimalFixture_FirstThreeContests_DeterministicSeries()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = new ParesNoConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("pares_no_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, metric.Value.Count);
        Assert.Equal(window.Size, metric.Value.Count);
        Assert.Equal([8, 6, 9], metric.Value.ToArray());
    }

    [Fact]
    public void ParesNoConcurso_SingleDraw_MaximumEvenDezenas_Is12()
    {
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24 };
        var draw = new Draw(1, new DateOnly(2026, 1, 1), numbers);
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new ParesNoConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal(12, metric.Value[0]);
    }

    [Fact]
    public void ParesNoConcurso_SingleDraw_MinimumEvenDezenas_Is2()
    {
        var numbers = new[] { 1, 2, 3, 4, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25 };
        var draw = new Draw(1, new DateOnly(2026, 1, 1), numbers);
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new ParesNoConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal(2, metric.Value[0]);
    }

    [Fact]
    public void QuantidadeVizinhosPorConcurso_OnMinimalFixture_FirstThreeContests_DeterministicSeries()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = new QuantidadeVizinhosPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal("quantidade_vizinhos_por_concurso", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, metric.Value.Count);
        Assert.Equal(window.Size, metric.Value.Count);
        Assert.Equal([7, 8, 8], metric.Value.ToArray());
    }

    [Fact]
    public void QuantidadeVizinhosPorConcurso_SingleDraw_FullConsecutiveBlock_Has14Adjacencies()
    {
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = new QuantidadeVizinhosPorConcursoMetric();

        var metric = sut.Compute(window);

        Assert.Equal(14, metric.Value[0]);
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesParesNoConcurso()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("pares_no_concurso", window);

        Assert.Equal("pares_no_concurso", metric.MetricName);
        Assert.Equal([8, 6, 9], metric.Value.ToArray());
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesQuantidadeVizinhosPorConcurso()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("quantidade_vizinhos_por_concurso", window);

        Assert.Equal("quantidade_vizinhos_por_concurso", metric.MetricName);
        Assert.Equal([7, 8, 8], metric.Value.ToArray());
    }

    [Fact]
    public void WindowMetricDispatcher_DispatchesRepeticaoConcursoAnterior()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var metric = sut.Dispatch("repeticao_concurso_anterior", window);

        Assert.Equal("repeticao_concurso_anterior", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal(2, metric.Value.Count);
        Assert.Equal(RepeticaoConcursoAnteriorSeries.Build(window), metric.Value);
    }

    [Fact]
    public void WindowMetricDispatcher_WithUnknownMetric_ThrowsDomainInvariantViolation()
    {
        var window = BuildWindow(endContestIdInclusive: 3);
        var sut = CreateWindowMetricDispatcher();

        var error = Assert.Throws<DomainInvariantViolationException>(() =>
        {
            _ = sut.Dispatch("metrica_inexistente", window);
        });

        Assert.Contains("UNKNOWN_METRIC", error.Message, StringComparison.Ordinal);
        Assert.Contains("metrica_inexistente", error.Message, StringComparison.Ordinal);
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

    private static DrawWindow BuildWindow(int endContestIdInclusive)
    {
        var draws = LoadSyntheticFixture()
            .Where(d => d.ContestId <= endContestIdInclusive)
            .Select(ToDomainDraw)
            .ToList();

        return new DrawWindow(
            Size: draws.Count,
            StartContestId: draws.First().ContestId,
            EndContestId: draws.Last().ContestId,
            Draws: draws);
    }

    private static WindowMetricDispatcher CreateWindowMetricDispatcher()
    {
        var frequency = new FrequencyByDezenaMetric();
        return new WindowMetricDispatcher(
            frequency,
            new Top10MaisSorteadosMetric(frequency),
            new Top10MenosSorteadosMetric(frequency),
            new ParesNoConcursoMetric(),
            new RepeticaoConcursoAnteriorMetric(),
            new QuantidadeVizinhosPorConcursoMetric(),
            new SequenciaMaximaVizinhosPorConcursoMetric(),
            new DistribuicaoLinhaPorConcursoMetric());
    }

    private sealed record FixtureRoot(
        [property: JsonPropertyName("draws")] IReadOnlyList<FixtureDraw> Draws);

    private sealed record FixtureDraw(
        [property: JsonPropertyName("contest_id")] int ContestId,
        [property: JsonPropertyName("draw_date")] string DrawDate,
        [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);
}

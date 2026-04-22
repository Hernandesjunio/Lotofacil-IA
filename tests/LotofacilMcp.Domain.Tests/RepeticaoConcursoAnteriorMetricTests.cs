using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

/// <summary>
/// Série escalar repeticao_concurso_anterior@1.0.0 (catálogo + ADR 0001 D18).
/// </summary>
public sealed class RepeticaoConcursoAnteriorMetricTests
{
    private static readonly DateOnly Date = new(2003, 9, 29);

    [Fact]
    public void Serie_ComPredecessorForaDaJanela_TemComprimentoN()
    {
        // predecessor 1..15; d0 shares 1..14 with pred (15->16)
        var preceding = D(1, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        var d0 = D(2, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16]);
        var d1 = D(3, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 16]);
        var d2 = D(4, [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 17]);

        var window = new DrawWindow(
            Size: 3,
            StartContestId: 2,
            EndContestId: 4,
            Draws: [d0, d1, d2],
            PrecedingDraw: preceding);

        var sut = new RepeticaoConcursoAnteriorMetric();
        var metric = sut.Compute(window);

        Assert.Equal("repeticao_concurso_anterior", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(3, metric.Value.Count);

        Assert.Equal(14, metric.Value[0]);
        Assert.Equal(14, metric.Value[1]);
        Assert.Equal(13, metric.Value[2]);
    }

    [Fact]
    public void Serie_SemPredecessor_PrimeiroSorteioDoHistorico_TemComprimentoNMenos1()
    {
        var d0 = D(1, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        var d1 = D(2, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16]);
        var d2 = D(3, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 17]);

        var window = new DrawWindow(
            Size: 3,
            StartContestId: 1,
            EndContestId: 3,
            Draws: [d0, d1, d2],
            PrecedingDraw: null);

        var sut = new RepeticaoConcursoAnteriorMetric();
        var metric = sut.Compute(window);

        Assert.Equal(2, metric.Value.Count);
        Assert.Equal(14, metric.Value[0]);
        Assert.Equal(13, metric.Value[1]);
    }

    [Fact]
    public void Serie_SemPredecessor_EUmUnicoConcurso_Lanca()
    {
        var d0 = D(1, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);

        var window = new DrawWindow(
            Size: 1,
            StartContestId: 1,
            EndContestId: 1,
            Draws: [d0],
            PrecedingDraw: null);

        var sut = new RepeticaoConcursoAnteriorMetric();

        Assert.Throws<DomainInvariantViolationException>(() => sut.Compute(window));
    }

    [Fact]
    public void WindowResolver_QuandoJanelaNaoIniciaNoPrimeiroSorteio_PreenchePrecedingDraw()
    {
        var draws = Enumerable.Range(1, 6)
            .Select(i => new Draw(i, Date, Enumerable.Range(1, 15).ToArray()))
            .ToList();

        var sut = new WindowResolver();
        var window = sut.Resolve(draws, windowSize: 3, endContestId: 5);

        Assert.NotNull(window.PrecedingDraw);
        Assert.Equal(2, window.PrecedingDraw!.ContestId);
        Assert.Equal(3, window.StartContestId);
    }

    [Fact]
    public void WindowResolver_QuandoJanelaIniciaNoPrimeiroSorteio_PrecedingDrawNulo()
    {
        var draws = Enumerable.Range(1, 5)
            .Select(i => new Draw(i, Date, Enumerable.Range(1, 15).ToArray()))
            .ToList();

        var sut = new WindowResolver();
        var window = sut.Resolve(draws, windowSize: 3, endContestId: 3);

        Assert.Null(window.PrecedingDraw);
        Assert.Equal(1, window.StartContestId);
    }

    [Fact]
    public void DrawWindow_PrecedingDrawComContestIdNaoAnterior_Lanca()
    {
        var d0 = new Draw(2, Date, Enumerable.Range(1, 15).ToArray());
        var bad = new Draw(3, Date, Enumerable.Range(1, 15).ToArray());

        Assert.Throws<DomainInvariantViolationException>(() => _ = new DrawWindow(
            Size: 1,
            StartContestId: 2,
            EndContestId: 2,
            Draws: [d0],
            PrecedingDraw: bad));
    }

    private static Draw D(int contestId, int[] numbers) => new(contestId, Date, numbers);
}

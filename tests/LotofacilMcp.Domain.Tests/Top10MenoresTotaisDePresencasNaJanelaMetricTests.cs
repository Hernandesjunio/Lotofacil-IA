using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class Top10MenoresTotaisDePresencasNaJanelaMetricTests
{
    [Fact]
    public void Window_OnSyntheticFixture_FirstThreeContests_MatchesTop10MenosSorteados_WhenTotalsEqualFrequencies()
    {
        // Normative references:
        // - docs/metric-catalog.md (Tabela 2): ordenar total_de_presencas_na_janela_por_dezena asc; empate por dezena asc; top 10.
        // - docs/test-plan.md: ties estáveis (desempate dezena asc).
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("top10_menores_totais_de_presencas_na_janela", window);

        Assert.Equal("top10_menores_totais_de_presencas_na_janela", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("dezena_list[10]", metric.Shape);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal([21, 22, 2, 3, 8, 15, 17, 18, 19, 25], metric.Value.Select(static x => (int)x).ToArray());
    }

    [Fact]
    public void Window_Size1_TieBreakUsesAscendingDezena_WhenTotalsMatch()
    {
        var draw = new Draw(1, new DateOnly(2026, 1, 1), Enumerable.Range(1, 15).ToArray());
        var window = new DrawWindow(1, 1, 1, [draw]);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("top10_menores_totais_de_presencas_na_janela", window);

        // All absent dezenas tie at 0; tie-break is dezena asc.
        Assert.Equal([16, 17, 18, 19, 20, 21, 22, 23, 24, 25], metric.Value.Select(static x => (int)x).ToArray());
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


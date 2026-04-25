using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class TotalDePresencasNaJanelaPorDezenaMetricTests
{
    [Fact]
    public void Window_OnSyntheticFixture_FirstThreeContests_EqualsFrequencyByDezena_And_HasConservation15TimesWindowSize()
    {
        // Normative references:
        // - docs/metric-catalog.md (Tabela 2): total[d] = contagem(d em concursos da janela) (equivalente a frequencia_por_dezena)
        // - docs/test-plan.md (Cobertura por métrica): equivalência + conservação 15×N
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("total_de_presencas_na_janela_por_dezena", window);

        Assert.Equal("total_de_presencas_na_janela_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);

        Assert.Equal(25, metric.Value.Count);
        Assert.Equal(
            // Expected vector for contests 1..3 in synthetic_min_window.json (same as frequencia_por_dezena).
            [2, 1, 1, 2, 2, 3, 2, 1, 3, 2, 3, 2, 2, 2, 1, 3, 1, 1, 1, 3, 0, 0, 3, 3, 1],
            metric.Value.Select(static x => (int)x).ToArray());

        Assert.Equal(15 * window.Size, metric.Value.Sum());
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


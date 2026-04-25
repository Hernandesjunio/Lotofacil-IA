using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Tests;

public sealed class SequenciaAtualDePresencasPorDezenaMetricTests
{
    [Fact]
    public void Window_OnSyntheticFixture_FirstThreeContests_ComputesCurrentPresenceStreak_ResettingOnAbsence()
    {
        // Normative references:
        // - docs/metric-catalog.md (Tabela 2): streak por dezena; reinicia ao ausente; retorna c[d] no fim do recorte.
        // - docs/test-plan.md (Cobertura por métrica): borda "ausente no último concurso => 0"
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 3);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("sequencia_atual_de_presencas_por_dezena", window);

        Assert.Equal("sequencia_atual_de_presencas_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);

        // Computed by hand from contests 1..3 in synthetic_min_window.json.
        // Example border: dezena 5 saiu nos concursos 1 e 2, mas não no 3 => streak atual = 0.
        Assert.Equal(0, (int)metric.Value[5 - 1]);
        Assert.Equal(
            [2, 0, 0, 2, 0, 3, 2, 1, 3, 1, 3, 2, 0, 1, 0, 3, 1, 0, 0, 3, 0, 0, 3, 3, 0],
            metric.Value.Select(static x => (int)x).ToArray());
    }

    [Fact]
    public void Window_Size1_BehavesLikePresenceIndicator_PerDezena()
    {
        // Normative references:
        // - docs/test-plan.md (Cobertura por métrica): janela N=1 => 1 se saiu, senão 0.
        var window = BuildWindowFromSyntheticFixture(endContestIdInclusive: 1);
        var sut = WindowMetricDispatcherFactory.Create();

        var metric = sut.Dispatch("sequencia_atual_de_presencas_por_dezena", window);

        // Contest 1 numbers: 2,3,5,6,9,10,11,13,14,16,18,20,23,24,25
        Assert.Equal(1, (int)metric.Value[2 - 1]);
        Assert.Equal(1, (int)metric.Value[25 - 1]);
        Assert.Equal(0, (int)metric.Value[1 - 1]);
        Assert.Equal(0, (int)metric.Value[4 - 1]);
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


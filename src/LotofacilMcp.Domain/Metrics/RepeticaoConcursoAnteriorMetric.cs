using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

/// <summary>
/// Série r_t = |J_t ∩ J_{t-1}| com comprimento N ou N-1 conforme ADR 0001 D18.
/// </summary>
public sealed class RepeticaoConcursoAnteriorMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var series = RepeticaoConcursoAnteriorSeries.Build(window);
        return new WindowMetricValue(
            MetricName: "repeticao_concurso_anterior",
            Scope: "series",
            Shape: "series",
            Unit: "count",
            Version: "1.0.0",
            Value: series.Select(static x => (double)x).ToArray());
    }
}

public static class RepeticaoConcursoAnteriorSeries
{
    /// <summary>Valores inteiros da série (comprimento normativo D18).</summary>
    public static IReadOnlyList<int> Build(DrawWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (window.Draws.Count != window.Size)
        {
            throw new DomainInvariantViolationException(
                "repeticao_concurso_anterior: draw count must match the resolved window size.");
        }

        var draws = window.Draws;
        var n = draws.Count;

        if (window.PrecedingDraw is not null)
        {
            var result = new int[n];
            result[0] = IntersectionSize(window.PrecedingDraw, draws[0]);
            for (var i = 1; i < n; i++)
            {
                result[i] = IntersectionSize(draws[i - 1], draws[i]);
            }

            return result;
        }

        if (n < 2)
        {
            throw new DomainInvariantViolationException(
                "repeticao_concurso_anterior: sem concurso anterior fora da janela, sao necessarios pelo menos dois concursos na janela.");
        }

        var pairCount = n - 1;
        var repetitions = new int[pairCount];
        for (var i = 0; i < pairCount; i++)
        {
            repetitions[i] = IntersectionSize(draws[i], draws[i + 1]);
        }

        return repetitions;
    }

    private static int IntersectionSize(Draw a, Draw b)
    {
        var set = new HashSet<int>(a.Numbers);
        return b.Numbers.Count(n => set.Contains(n));
    }
}

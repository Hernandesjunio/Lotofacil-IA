using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class QuantidadeVizinhosPorConcursoMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        if (window.Draws.Count != window.Size)
        {
            throw new DomainInvariantViolationException(
                "quantidade_vizinhos_por_concurso: draw count must match the resolved window size.");
        }

        var series = new int[window.Size];
        for (var i = 0; i < window.Size; i++)
        {
            series[i] = CountConsecutiveAdjacencies(window.Draws[i]);
        }

        return new WindowMetricValue(
            MetricName: "quantidade_vizinhos_por_concurso",
            Scope: "series",
            Shape: "series",
            Unit: "count",
            Version: "1.0.0",
            Value: Array.ConvertAll(series, static x => (double)x));
    }

    private static int CountConsecutiveAdjacencies(Draw draw)
    {
        var count = 0;
        for (var index = 1; index < draw.Numbers.Count; index++)
        {
            if (draw.Numbers[index] - draw.Numbers[index - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }
}

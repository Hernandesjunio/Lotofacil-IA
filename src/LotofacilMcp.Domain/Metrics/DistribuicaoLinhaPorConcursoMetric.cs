using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class DistribuicaoLinhaPorConcursoMetric
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
                "distribuicao_linha_por_concurso: draw count must match the resolved window size.");
        }

        var flat = new int[window.Size * 5];
        for (var i = 0; i < window.Size; i++)
        {
            FillRowCounts(window.Draws[i], flat, i * 5);
        }

        return new WindowMetricValue(
            MetricName: "distribuicao_linha_por_concurso",
            Scope: "series",
            Shape: "series_of_count_vector[5]",
            Unit: "count",
            Version: "1.0.0",
            Value: flat);
    }

    private static void FillRowCounts(Draw draw, int[] destination, int offset)
    {
        foreach (var number in draw.Numbers)
        {
            var rowIndex = (number - 1) / 5;
            destination[offset + rowIndex]++;
        }
    }
}

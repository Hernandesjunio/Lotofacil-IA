using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class HhiLinhaPorConcursoMetric
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
                "hhi_linha_por_concurso: draw count must match the resolved window size.");
        }

        var series = new double[window.Size];
        Span<int> counts = stackalloc int[5];
        for (var i = 0; i < window.Size; i++)
        {
            VolanteRowColumnCounts.FillRowCounts(window.Draws[i], counts);
            series[i] = HerfindahlHirschmanIndex.FromNonNegativeCounts(counts);
        }

        return new WindowMetricValue(
            MetricName: "hhi_linha_por_concurso",
            Scope: "series",
            Shape: "series",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: series);
    }
}

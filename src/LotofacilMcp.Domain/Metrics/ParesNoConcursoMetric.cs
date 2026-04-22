using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class ParesNoConcursoMetric
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
                "pares_no_concurso: draw count must match the resolved window size.");
        }

        var series = new int[window.Size];
        for (var i = 0; i < window.Size; i++)
        {
            series[i] = CountEvenDezenas(window.Draws[i]);
        }

        return new WindowMetricValue(
            MetricName: "pares_no_concurso",
            Scope: "series",
            Shape: "series",
            Unit: "count",
            Version: "1.0.0",
            Value: series);
    }

    private static int CountEvenDezenas(Draw draw)
    {
        var c = 0;
        foreach (var n in draw.Numbers)
        {
            if (n % 2 == 0)
            {
                c++;
            }
        }

        return c;
    }
}

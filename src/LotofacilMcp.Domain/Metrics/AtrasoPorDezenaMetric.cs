using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class AtrasoPorDezenaMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var w = window.Draws.Count;
        var values = new double[25];
        for (var d = 0; d < 25; d++)
        {
            var dezena = d + 1;
            var lastIndex = -1;
            for (var i = 0; i < w; i++)
            {
                if (DezenaPresente(window.Draws[i].Numbers, dezena))
                {
                    lastIndex = i;
                }
            }

            if (lastIndex < 0)
            {
                values[d] = w;
            }
            else
            {
                values[d] = w - 1 - lastIndex;
            }
        }

        return new WindowMetricValue(
            MetricName: "atraso_por_dezena",
            Scope: "window",
            Shape: "vector_by_dezena",
            Unit: "count",
            Version: "1.0.0",
            Value: values);
    }

    private static bool DezenaPresente(IReadOnlyList<int> numbers, int dezena)
    {
        foreach (var n in numbers)
        {
            if (n == dezena)
            {
                return true;
            }
        }

        return false;
    }
}

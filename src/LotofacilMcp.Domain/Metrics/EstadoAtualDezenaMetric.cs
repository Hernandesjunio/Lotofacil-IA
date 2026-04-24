using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class EstadoAtualDezenaMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var values = new double[25];
        for (var dezena = 1; dezena <= 25; dezena++)
        {
            values[dezena - 1] = ComputeCurrentState(window, dezena);
        }

        return new WindowMetricValue(
            MetricName: "estado_atual_dezena",
            Scope: "window",
            Shape: "vector_by_dezena",
            Unit: "count",
            Version: "1.0.0",
            Value: values);
    }

    private static int ComputeCurrentState(DrawWindow window, int dezena)
    {
        for (var drawIndex = window.Draws.Count - 1; drawIndex >= 0; drawIndex--)
        {
            if (window.Draws[drawIndex].Numbers.Contains(dezena))
            {
                return window.Draws.Count - 1 - drawIndex;
            }
        }

        return window.Draws.Count;
    }
}

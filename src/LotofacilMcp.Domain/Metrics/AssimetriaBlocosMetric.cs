using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

/// <summary>
/// Por dezena: (pres - aus) / (pres + aus) com pres + aus = W na janela; ver metric-catalog.
/// </summary>
public sealed class AssimetriaBlocosMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var w = window.Draws.Count;
        if (w == 0)
        {
            throw new DomainInvariantViolationException("window cannot be empty.");
        }

        var frequencies = new int[25];
        foreach (var draw in window.Draws)
        {
            foreach (var n in draw.Numbers)
            {
                frequencies[n - 1]++;
            }
        }

        var values = new double[25];
        for (var d = 0; d < 25; d++)
        {
            var pres = frequencies[d];
            var aus = w - pres;
            values[d] = (double)(pres - aus) / (pres + aus);
        }

        return new WindowMetricValue(
            MetricName: "assimetria_blocos",
            Scope: "window",
            Shape: "vector_by_dezena",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: values);
    }
}

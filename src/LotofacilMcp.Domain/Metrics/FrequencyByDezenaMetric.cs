using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class FrequencyByDezenaMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var frequencies = new int[25];

        foreach (var draw in window.Draws)
        {
            foreach (var number in draw.Numbers)
            {
                frequencies[number - 1]++;
            }
        }

        return new WindowMetricValue(
            MetricName: "frequencia_por_dezena",
            Scope: "window",
            Shape: "vector_by_dezena",
            Unit: "count",
            Version: "1.0.0",
            Value: Array.ConvertAll(frequencies, static f => (double)f));
    }
}

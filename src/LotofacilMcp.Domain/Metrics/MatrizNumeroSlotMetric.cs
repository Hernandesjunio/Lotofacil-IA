using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class MatrizNumeroSlotMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var matrix = new double[25 * 15];
        foreach (var draw in window.Draws)
        {
            for (var slot = 0; slot < draw.Numbers.Count; slot++)
            {
                var dezenaIndex = draw.Numbers[slot] - 1;
                var flatIndex = (dezenaIndex * 15) + slot;
                matrix[flatIndex]++;
            }
        }

        return new WindowMetricValue(
            MetricName: "matriz_numero_slot",
            Scope: "window",
            Shape: "count_matrix[25x15]",
            Unit: "count",
            Version: "1.0.0",
            Value: matrix);
    }
}

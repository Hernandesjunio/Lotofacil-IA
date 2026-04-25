using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class SequenciaAtualDePresencasPorDezenaMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var streaks = new int[25];

        foreach (var draw in window.Draws)
        {
            var present = new bool[25];
            foreach (var number in draw.Numbers)
            {
                present[number - 1] = true;
            }

            for (var i = 0; i < 25; i++)
            {
                streaks[i] = present[i] ? (streaks[i] + 1) : 0;
            }
        }

        return new WindowMetricValue(
            MetricName: "sequencia_atual_de_presencas_por_dezena",
            Scope: "window",
            Shape: "vector_by_dezena",
            Unit: "count",
            Version: "1.0.0",
            Value: Array.ConvertAll(streaks, static s => (double)s));
    }
}

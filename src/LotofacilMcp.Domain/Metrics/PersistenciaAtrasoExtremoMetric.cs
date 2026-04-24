using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class PersistenciaAtrasoExtremoMetric
{
    private readonly AtrasoPorDezenaMetric _atrasoPorDezenaMetric = new();

    public WindowMetricValue Compute(DrawWindow window, double referencePercentileThreshold)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var atraso = _atrasoPorDezenaMetric.Compute(window).Value;
        var count = atraso.Count(value => value > referencePercentileThreshold);

        return new WindowMetricValue(
            MetricName: "persistencia_atraso_extremo",
            Scope: "window",
            Shape: "scalar",
            Unit: "count",
            Version: "2.0.0",
            Value: [count]);
    }
}

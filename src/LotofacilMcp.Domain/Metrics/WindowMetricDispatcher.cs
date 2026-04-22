using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class WindowMetricDispatcher
{
    private readonly IReadOnlyDictionary<string, Func<DrawWindow, FrequencyByDezenaMetricValue>> _dispatchByMetricName;

    public WindowMetricDispatcher(FrequencyByDezenaMetric frequencyByDezenaMetric)
    {
        ArgumentNullException.ThrowIfNull(frequencyByDezenaMetric);

        _dispatchByMetricName = new Dictionary<string, Func<DrawWindow, FrequencyByDezenaMetricValue>>(StringComparer.Ordinal)
        {
            ["frequencia_por_dezena"] = frequencyByDezenaMetric.Compute
        };
    }

    public FrequencyByDezenaMetricValue Dispatch(string metricName, DrawWindow window)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new DomainInvariantViolationException("UNKNOWN_METRIC: ");
        }

        if (_dispatchByMetricName.TryGetValue(metricName, out var computeMetric))
        {
            return computeMetric(window);
        }

        throw new DomainInvariantViolationException($"UNKNOWN_METRIC: {metricName}");
    }
}

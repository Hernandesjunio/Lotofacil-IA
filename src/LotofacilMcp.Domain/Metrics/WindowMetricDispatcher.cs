using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class WindowMetricDispatcher
{
    private readonly IReadOnlyDictionary<string, Func<DrawWindow, WindowMetricValue>> _dispatchByMetricName;

    public WindowMetricDispatcher(
        FrequencyByDezenaMetric frequencyByDezenaMetric,
        Top10MaisSorteadosMetric top10MaisSorteadosMetric)
    {
        ArgumentNullException.ThrowIfNull(frequencyByDezenaMetric);
        ArgumentNullException.ThrowIfNull(top10MaisSorteadosMetric);

        _dispatchByMetricName = new Dictionary<string, Func<DrawWindow, WindowMetricValue>>(StringComparer.Ordinal)
        {
            ["frequencia_por_dezena"] = frequencyByDezenaMetric.Compute,
            ["top10_mais_sorteados"] = top10MaisSorteadosMetric.Compute
        };
    }

    public WindowMetricValue Dispatch(string metricName, DrawWindow window)
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

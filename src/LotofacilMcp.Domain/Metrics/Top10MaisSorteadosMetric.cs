using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class Top10MaisSorteadosMetric
{
    private readonly FrequencyByDezenaMetric _frequencyByDezena;

    public Top10MaisSorteadosMetric(FrequencyByDezenaMetric frequencyByDezena)
    {
        _frequencyByDezena = frequencyByDezena ?? throw new ArgumentNullException(nameof(frequencyByDezena));
    }

    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var freq = _frequencyByDezena.Compute(window);
        var frequencies = freq.Value;

        var top10 = Enumerable.Range(1, 25)
            .OrderByDescending(d => frequencies[d - 1])
            .ThenBy(d => d)
            .Take(10)
            .ToArray();

        return new WindowMetricValue(
            MetricName: "top10_mais_sorteados",
            Scope: "window",
            Shape: "dezena_list[10]",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: top10);
    }
}

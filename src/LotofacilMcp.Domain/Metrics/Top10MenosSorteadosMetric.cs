using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class Top10MenosSorteadosMetric
{
    private readonly FrequencyByDezenaMetric _frequencyByDezena;

    public Top10MenosSorteadosMetric(FrequencyByDezenaMetric frequencyByDezena)
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
            .OrderBy(d => frequencies[d - 1])
            .ThenBy(d => d)
            .Take(10)
            .ToArray();

        return new WindowMetricValue(
            MetricName: "top10_menos_sorteados",
            Scope: "window",
            Shape: "dezena_list[10]",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: Array.ConvertAll(top10, static d => (double)d));
    }
}

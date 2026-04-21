using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed record FrequencyByDezenaMetricValue(
    string MetricName,
    string Scope,
    string Shape,
    string Unit,
    string Version,
    IReadOnlyList<int> Value);

public sealed class FrequencyByDezenaMetric
{
    public FrequencyByDezenaMetricValue Compute(DrawWindow window)
    {
        throw new NotImplementedException("frequencia_por_dezena@1.0.0 is implemented in Phase 3.");
    }
}

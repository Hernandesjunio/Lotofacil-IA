namespace LotofacilMcp.Domain.Metrics;

public sealed record WindowMetricValue(
    string MetricName,
    string Scope,
    string Shape,
    string Unit,
    string Version,
    IReadOnlyList<double> Value);

namespace LotofacilMcp.Domain.Analytics;

public sealed record ResolvedIndicatorScalarSeries(
    string IndicatorName,
    string Aggregation,
    int? ComponentIndex,
    string Shape,
    IReadOnlyList<double> Values);

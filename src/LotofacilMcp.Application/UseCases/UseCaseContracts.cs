namespace LotofacilMcp.Application.UseCases;

public sealed record WindowDescriptor(
    int Size,
    int StartContestId,
    int EndContestId);

public sealed record DrawView(
    int ContestId,
    string DrawDate,
    IReadOnlyList<int> Numbers);

public sealed record MetricRequestInput(string Name);

public sealed record StabilityIndicatorRequestInput(
    string Name,
    string? Aggregation);

public sealed record FrequencyMetricValueView(
    string MetricName,
    string Scope,
    string Shape,
    string Unit,
    string Version,
    WindowDescriptor Window,
    IReadOnlyList<double> Value,
    string Explanation);

public sealed record StabilityRankingEntryView(
    string IndicatorName,
    string Aggregation,
    int? ComponentIndex,
    string Shape,
    double Dispersion,
    double StabilityScore,
    string Explanation);

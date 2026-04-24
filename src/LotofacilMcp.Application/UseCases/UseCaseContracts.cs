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

public sealed record AssociationStabilityCheckInput(
    string Method,
    int SubwindowSize,
    int Stride,
    int MinSubwindows);

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

public sealed record CompositionComponentInput(
    string MetricName,
    string Transform,
    double Weight);

public sealed record ComposeIndicatorAnalysisInput(
    int WindowSize,
    int? EndContestId,
    string Target,
    string Operator,
    IReadOnlyList<CompositionComponentInput> Components,
    int TopK,
    string FixturePath = "");

public sealed record ComposeIndicatorAnalysisDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    string Target,
    string Operator,
    int TopK,
    IReadOnlyList<CompositionComponentInput> Components);

public sealed record WeightedDezenaRankingEntryView(
    int Dezena,
    int Rank,
    double Score,
    string Explanation);

public sealed record ComposeIndicatorAnalysisResult(
    string DatasetVersion,
    string ToolVersion,
    ComposeIndicatorAnalysisDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    string Target,
    string Operator,
    IReadOnlyList<WeightedDezenaRankingEntryView> Ranking);

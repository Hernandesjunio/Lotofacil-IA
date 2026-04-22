using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record WindowPatternFeatureInput(
    string MetricName,
    string? Aggregation);

public sealed record SummarizeWindowPatternsInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<WindowPatternFeatureInput> Features,
    double CoverageThreshold,
    string RangeMethod,
    string FixturePath = "");

public sealed record SummarizeWindowPatternsDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<WindowPatternFeatureInput> Features,
    double CoverageThreshold,
    string RangeMethod);

public sealed record WindowPatternSummaryView(
    string MetricName,
    string Aggregation,
    double Mode,
    double Q1,
    double Median,
    double Q3,
    double Iqr,
    double CoverageObserved,
    int CoverageCount,
    int TotalCount,
    int OutlierCount,
    double OutlierLowerFence,
    double OutlierUpperFence,
    bool CoverageThresholdMet,
    string Explanation);

public sealed record SummarizeWindowPatternsResult(
    string DatasetVersion,
    string ToolVersion,
    SummarizeWindowPatternsDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    string RangeMethod,
    double CoverageThreshold,
    IReadOnlyList<WindowPatternSummaryView> Summaries);

public sealed class SummarizeWindowPatternsUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public SummarizeWindowPatternsUseCase(
        SyntheticFixtureProvider fixtureProvider,
        DatasetVersionService datasetVersionService,
        WindowResolver windowResolver,
        WindowMetricDispatcher windowMetricDispatcher,
        V0CrossFieldValidator validator,
        V0RequestMapper mapper)
    {
        _fixtureProvider = fixtureProvider;
        _datasetVersionService = datasetVersionService;
        _windowResolver = windowResolver;
        _windowMetricDispatcher = windowMetricDispatcher;
        _validator = validator;
        _mapper = mapper;
    }

    public SummarizeWindowPatternsResult Execute(SummarizeWindowPatternsInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateSummarizeWindowPatterns(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var summaries = input.Features
                .Select(feature => BuildSummary(window, feature, input.CoverageThreshold))
                .ToArray();

            return new SummarizeWindowPatternsResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new SummarizeWindowPatternsDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Features.ToArray(),
                    input.CoverageThreshold,
                    "iqr"),
                Window: windowView,
                RangeMethod: "iqr",
                CoverageThreshold: input.CoverageThreshold,
                Summaries: summaries);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private WindowPatternSummaryView BuildSummary(
        DrawWindow window,
        WindowPatternFeatureInput feature,
        double coverageThreshold)
    {
        var metric = _windowMetricDispatcher.Dispatch(feature.MetricName, window);
        var values = metric.Value;
        var sorted = values.OrderBy(static value => value).ToArray();

        var mode = ComputeMode(values);
        var q1 = ComputePercentile(sorted, 0.25);
        var median = ComputePercentile(sorted, 0.5);
        var q3 = ComputePercentile(sorted, 0.75);
        var iqr = q3 - q1;
        var lowerFence = q1 - (1.5d * iqr);
        var upperFence = q3 + (1.5d * iqr);
        var coverageCount = values.Count(value => value >= q1 && value <= q3);
        var totalCount = values.Count;
        var coverageObserved = totalCount == 0 ? 0d : coverageCount / (double)totalCount;
        var outlierCount = values.Count(value => value < lowerFence || value > upperFence);

        return new WindowPatternSummaryView(
            MetricName: feature.MetricName,
            Aggregation: "identity",
            Mode: mode,
            Q1: q1,
            Median: median,
            Q3: q3,
            Iqr: iqr,
            CoverageObserved: coverageObserved,
            CoverageCount: coverageCount,
            TotalCount: totalCount,
            OutlierCount: outlierCount,
            OutlierLowerFence: lowerFence,
            OutlierUpperFence: upperFence,
            CoverageThresholdMet: coverageObserved >= coverageThreshold,
            Explanation:
            "Faixa tipica via IQR ([Q1,Q3]); cobertura e outliers calculados deterministicamente na janela.");
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        if (ex.Message.StartsWith("UNKNOWN_METRIC:", StringComparison.Ordinal))
        {
            var metricName = ex.Message["UNKNOWN_METRIC:".Length..].Trim();
            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is not available in V0.",
                details: new Dictionary<string, object?>
                {
                    ["metric_name"] = metricName
                });
        }

        if (ex.Message.Contains("requested end_contest_id", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INVALID_CONTEST_ID",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("insufficient history", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INSUFFICIENT_HISTORY",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        return new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }

    private static double ComputeMode(IReadOnlyList<double> values)
    {
        return values
            .GroupBy(static value => value)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;
    }

    private static double ComputePercentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0d;
        }

        var position = (sortedValues.Count - 1) * percentile;
        var left = (int)Math.Floor(position);
        var right = (int)Math.Ceiling(position);
        if (left == right)
        {
            return sortedValues[left];
        }

        var fraction = position - left;
        return sortedValues[left] + ((sortedValues[right] - sortedValues[left]) * fraction);
    }
}

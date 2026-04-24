using System.Text.Json;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record WindowAggregateRequestInput(
    string Id,
    string SourceMetricName,
    string AggregateType,
    JsonElement Params);

public sealed record SummarizeWindowAggregatesInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<WindowAggregateRequestInput>? Aggregates,
    string FixturePath = "");

public sealed record SummarizeWindowAggregatesDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<WindowAggregateRequestInput> Aggregates);

public sealed record HistogramBucketView(double X, int Count, double? Ratio);

public sealed record PatternCountItemView(IReadOnlyList<int> Pattern, int Count, double? Ratio);

public sealed record WindowAggregateView(
    string Id,
    string SourceMetricName,
    string AggregateType,
    IReadOnlyList<HistogramBucketView>? Buckets = null,
    IReadOnlyList<PatternCountItemView>? Items = null,
    IReadOnlyList<IReadOnlyList<int>>? Matrix = null);

public sealed record SummarizeWindowAggregatesResult(
    string DatasetVersion,
    string ToolVersion,
    SummarizeWindowAggregatesDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    IReadOnlyList<WindowAggregateView> Aggregates);

public sealed class SummarizeWindowAggregatesUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public SummarizeWindowAggregatesUseCase(
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

    public SummarizeWindowAggregatesResult Execute(SummarizeWindowAggregatesInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateSummarizeWindowAggregates(input);
        var aggregates = input.Aggregates!;

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var aggregateViews = aggregates
                .Select(aggregate => BuildAggregateView(window, aggregate))
                .ToArray();

            return new SummarizeWindowAggregatesResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new SummarizeWindowAggregatesDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    aggregates.Select(CloneAggregateForDeterminism).ToArray()),
                Window: windowView,
                Aggregates: aggregateViews);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private WindowAggregateView BuildAggregateView(DrawWindow window, WindowAggregateRequestInput aggregateRequest)
    {
        if (!MetricAvailabilityCatalog.IsKnownMetric(aggregateRequest.SourceMetricName))
        {
            throw new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "metric name is not listed in the metric catalog.",
                details: new Dictionary<string, object?>
                {
                    ["metric_name"] = aggregateRequest.SourceMetricName
                });
        }

        if (!MetricAvailabilityCatalog.IsExposedInSummarizeWindowAggregates(aggregateRequest.SourceMetricName))
        {
            throw new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is known in the catalog but unavailable in this summarize_window_aggregates build.",
                details: new Dictionary<string, object?>
                {
                    ["metric_name"] = aggregateRequest.SourceMetricName,
                    ["allowed_metrics"] = MetricAvailabilityCatalog.GetSummarizeWindowAggregatesAllowedSources().ToArray()
                });
        }

        var metric = _windowMetricDispatcher.Dispatch(aggregateRequest.SourceMetricName, window);

        return aggregateRequest.AggregateType switch
        {
            "histogram_scalar_series" => BuildScalarHistogram(aggregateRequest, metric),
            "topk_patterns_count_vector5_series" => BuildTopKPatterns(aggregateRequest, metric),
            "histogram_count_vector5_series_per_position_matrix" => BuildPerPositionMatrix(aggregateRequest, metric),
            _ => throw new ApplicationValidationException(
                code: "UNSUPPORTED_AGGREGATE_TYPE",
                message: "aggregate_type is not supported.",
                details: new Dictionary<string, object?>
                {
                    ["aggregate_type"] = aggregateRequest.AggregateType
                })
        };
    }

    private static WindowAggregateRequestInput CloneAggregateForDeterminism(WindowAggregateRequestInput aggregate)
    {
        return new WindowAggregateRequestInput(
            aggregate.Id,
            aggregate.SourceMetricName,
            aggregate.AggregateType,
            aggregate.Params.Clone());
    }

    private static WindowAggregateView BuildScalarHistogram(
        WindowAggregateRequestInput aggregateRequest,
        WindowMetricValue metric)
    {
        EnsureShape(metric, requiredScope: "series", requiredShape: "series", aggregateRequest.AggregateType);

        var includeRatios = TryReadBoolean(aggregateRequest.Params, "include_ratios");
        var bucketSpec = ReadRequiredObject(aggregateRequest.Params, "bucket_spec");
        var buckets = ParseBuckets(bucketSpec);
        var counts = new int[buckets.Count];

        foreach (var value in metric.Value)
        {
            for (var i = 0; i < buckets.Count; i++)
            {
                if (buckets[i].Contains(value))
                {
                    counts[i]++;
                    break;
                }
            }
        }

        var total = metric.Value.Count;
        var bucketViews = buckets
            .Select((bucket, index) => new HistogramBucketView(
                X: bucket.X,
                Count: counts[index],
                Ratio: includeRatios && total > 0 ? counts[index] / (double)total : null))
            .OrderBy(bucket => bucket.X)
            .ToArray();

        return new WindowAggregateView(
            Id: aggregateRequest.Id,
            SourceMetricName: aggregateRequest.SourceMetricName,
            AggregateType: aggregateRequest.AggregateType,
            Buckets: bucketViews);
    }

    private static WindowAggregateView BuildTopKPatterns(
        WindowAggregateRequestInput aggregateRequest,
        WindowMetricValue metric)
    {
        EnsureShape(
            metric,
            requiredScope: "series",
            requiredShape: "series_of_count_vector[5]",
            aggregateRequest.AggregateType);

        var topK = ReadRequiredPositiveInt(aggregateRequest.Params, "top_k");
        var includeRatios = TryReadBoolean(aggregateRequest.Params, "include_ratios");
        var patterns = ParseCountVector5Series(metric.Value);
        var total = patterns.Count;
        var grouped = patterns
            .GroupBy(static pattern => string.Join(",", pattern))
            .Select(group => new
            {
                Pattern = group.First(),
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Pattern, PatternLexicographicComparer.Instance)
            .Take(topK)
            .Select(item => new PatternCountItemView(
                Pattern: item.Pattern,
                Count: item.Count,
                Ratio: includeRatios && total > 0 ? item.Count / (double)total : null))
            .ToArray();

        return new WindowAggregateView(
            Id: aggregateRequest.Id,
            SourceMetricName: aggregateRequest.SourceMetricName,
            AggregateType: aggregateRequest.AggregateType,
            Items: grouped);
    }

    private static WindowAggregateView BuildPerPositionMatrix(
        WindowAggregateRequestInput aggregateRequest,
        WindowMetricValue metric)
    {
        EnsureShape(
            metric,
            requiredScope: "series",
            requiredShape: "series_of_count_vector[5]",
            aggregateRequest.AggregateType);

        var valueMin = ReadRequiredInt(aggregateRequest.Params, "value_min");
        var valueMax = ReadRequiredInt(aggregateRequest.Params, "value_max");
        if (valueMin > valueMax)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "value_min must be less than or equal to value_max.",
                details: new Dictionary<string, object?>
                {
                    ["value_min"] = valueMin,
                    ["value_max"] = valueMax
                });
        }

        var values = ParseCountVector5Series(metric.Value);
        var width = (valueMax - valueMin) + 1;
        var matrix = new int[5][];
        for (var row = 0; row < 5; row++)
        {
            matrix[row] = new int[width];
        }

        foreach (var vector in values)
        {
            for (var position = 0; position < 5; position++)
            {
                var value = vector[position];
                if (value >= valueMin && value <= valueMax)
                {
                    matrix[position][value - valueMin]++;
                }
            }
        }

        return new WindowAggregateView(
            Id: aggregateRequest.Id,
            SourceMetricName: aggregateRequest.SourceMetricName,
            AggregateType: aggregateRequest.AggregateType,
            Matrix: matrix.Select(static row => (IReadOnlyList<int>)row).ToArray());
    }

    private static void EnsureShape(
        WindowMetricValue metric,
        string requiredScope,
        string requiredShape,
        string aggregateType)
    {
        if (!string.Equals(metric.Scope, requiredScope, StringComparison.Ordinal) ||
            !string.Equals(metric.Shape, requiredShape, StringComparison.Ordinal))
        {
            throw new ApplicationValidationException(
                code: "UNSUPPORTED_SHAPE",
                message: "source metric shape/scope is incompatible with aggregate_type.",
                details: new Dictionary<string, object?>
                {
                    ["aggregate_type"] = aggregateType,
                    ["source_scope"] = metric.Scope,
                    ["source_shape"] = metric.Shape
                });
        }
    }

    private static IReadOnlyList<int[]> ParseCountVector5Series(IReadOnlyList<double> values)
    {
        if (values.Count % 5 != 0)
        {
            throw new ApplicationValidationException(
                code: "UNSUPPORTED_SHAPE",
                message: "source metric value does not represent a series_of_count_vector[5].",
                details: new Dictionary<string, object?>());
        }

        var output = new List<int[]>(values.Count / 5);
        for (var i = 0; i < values.Count; i += 5)
        {
            var vector = new int[5];
            for (var j = 0; j < 5; j++)
            {
                vector[j] = checked((int)Math.Round(values[i + j], MidpointRounding.AwayFromZero));
            }

            output.Add(vector);
        }

        return output;
    }

    private static IReadOnlyList<HistogramBucket> ParseBuckets(JsonElement bucketSpec)
    {
        var hasBucketValues = bucketSpec.TryGetProperty("bucket_values", out var bucketValuesElement);
        var hasMin = bucketSpec.TryGetProperty("min", out var minElement);
        var hasMax = bucketSpec.TryGetProperty("max", out var maxElement);
        var hasWidth = bucketSpec.TryGetProperty("width", out var widthElement);

        var discreteMode = hasBucketValues;
        var continuousMode = hasMin || hasMax || hasWidth;

        if (discreteMode == continuousMode)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_spec must declare exactly one mode: bucket_values or min/max/width.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec"
                });
        }

        if (discreteMode)
        {
            if (bucketValuesElement.ValueKind != JsonValueKind.Array || bucketValuesElement.GetArrayLength() == 0)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "bucket_values must be a non-empty array.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "aggregates[].params.bucket_spec.bucket_values"
                    });
            }

            return bucketValuesElement
                .EnumerateArray()
                .Select(x => new HistogramBucket(
                    X: ReadNumber(x, "aggregates[].params.bucket_spec.bucket_values[]"),
                    LowerBound: ReadNumber(x, "aggregates[].params.bucket_spec.bucket_values[]"),
                    UpperBoundExclusive: ReadNumber(x, "aggregates[].params.bucket_spec.bucket_values[]"),
                    IncludeUpperBound: true))
                .OrderBy(bucket => bucket.X)
                .ToArray();
        }

        if (!hasMin || !hasMax || !hasWidth)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_spec continuous mode requires min, max and width.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec"
                });
        }

        var min = ReadNumber(minElement, "aggregates[].params.bucket_spec.min");
        var max = ReadNumber(maxElement, "aggregates[].params.bucket_spec.max");
        var width = ReadNumber(widthElement, "aggregates[].params.bucket_spec.width");

        if (width <= 0d || max < min)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_spec continuous mode requires width > 0 and max >= min.",
                details: new Dictionary<string, object?>
                {
                    ["min"] = min,
                    ["max"] = max,
                    ["width"] = width
                });
        }

        var buckets = new List<HistogramBucket>();
        var cursor = min;
        while (cursor <= max + 1e-12)
        {
            var upper = cursor + width;
            var includeUpper = upper >= max;
            buckets.Add(new HistogramBucket(
                X: cursor,
                LowerBound: cursor,
                UpperBoundExclusive: upper,
                IncludeUpperBound: includeUpper));
            cursor = upper;
        }

        return buckets;
    }

    private static JsonElement ReadRequiredObject(JsonElement parent, string propertyName)
    {
        if (parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(propertyName, out var child) ||
            child.ValueKind != JsonValueKind.Object)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: $"{propertyName} must be an object.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = $"aggregates[].params.{propertyName}"
                });
        }

        return child;
    }

    private static bool TryReadBoolean(JsonElement parent, string propertyName)
    {
        if (parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(propertyName, out var value))
        {
            return false;
        }

        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return value.GetBoolean();
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: $"{propertyName} must be a boolean when provided.",
            details: new Dictionary<string, object?>
            {
                ["field"] = $"aggregates[].params.{propertyName}"
            });
    }

    private static int ReadRequiredPositiveInt(JsonElement parent, string propertyName)
    {
        var value = ReadRequiredInt(parent, propertyName);
        if (value < 1)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: $"{propertyName} must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    [propertyName] = value
                });
        }

        return value;
    }

    private static int ReadRequiredInt(JsonElement parent, string propertyName)
    {
        if (parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(propertyName, out var valueElement) ||
            valueElement.ValueKind != JsonValueKind.Number ||
            !valueElement.TryGetInt32(out var value))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: $"{propertyName} must be an integer.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = $"aggregates[].params.{propertyName}"
                });
        }

        return value;
    }

    private static double ReadNumber(JsonElement element, string fieldPath)
    {
        if (element.ValueKind != JsonValueKind.Number ||
            !element.TryGetDouble(out var value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: $"{fieldPath} must be a finite number.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = fieldPath
                });
        }

        return value;
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        if (ex.Message.StartsWith("UNKNOWN_METRIC:", StringComparison.Ordinal))
        {
            var metricName = ex.Message["UNKNOWN_METRIC:".Length..].Trim();
            var details = new Dictionary<string, object?>
            {
                ["metric_name"] = metricName
            };

            if (MetricAvailabilityCatalog.IsKnownMetric(metricName))
            {
                details["allowed_metrics"] = MetricAvailabilityCatalog.GetSummarizeWindowAggregatesAllowedSources().ToArray();
            }

            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is not available in this summarize_window_aggregates build.",
                details: details);
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

    private sealed record HistogramBucket(
        double X,
        double LowerBound,
        double UpperBoundExclusive,
        bool IncludeUpperBound)
    {
        public bool Contains(double value)
        {
            if (IncludeUpperBound)
            {
                return value >= LowerBound && value <= UpperBoundExclusive;
            }

            return value >= LowerBound && value < UpperBoundExclusive;
        }
    }

    private sealed class PatternLexicographicComparer : IComparer<IReadOnlyList<int>>
    {
        public static PatternLexicographicComparer Instance { get; } = new();

        public int Compare(IReadOnlyList<int>? x, IReadOnlyList<int>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var minLength = Math.Min(x.Count, y.Count);
            for (var i = 0; i < minLength; i++)
            {
                var cmp = x[i].CompareTo(y[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            return x.Count.CompareTo(y.Count);
        }
    }
}

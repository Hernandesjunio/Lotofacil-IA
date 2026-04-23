using LotofacilMcp.Application.Composition;
using LotofacilMcp.Application.UseCases;
using System.Text.Json;

namespace LotofacilMcp.Application.Validation;

public sealed class V0CrossFieldValidator
{
    private static readonly HashSet<string> SupportedComposeTransforms =
    [
        "normalize_max",
        "invert_normalize_max",
        "rank_percentile",
        "identity_unit_interval",
        "one_minus_unit_interval",
        "shift_scale_unit_interval"
    ];

    private static readonly HashSet<string> SupportedNormalizationMethods =
    [
        "madn",
        "coefficient_of_variation"
    ];

    private static readonly HashSet<string> SupportedWindowPatternFeatures =
    [
        "pares_no_concurso"
    ];

    private static readonly HashSet<string> SupportedRangeMethods =
    [
        "iqr"
    ];

    private static readonly HashSet<string> SupportedGenerationStrategies =
    [
        "common_repetition_frequency"
    ];

    private static readonly HashSet<string> SupportedSearchMethods =
    [
        "exhaustive",
        "sampled",
        "greedy_topk"
    ];

    private static readonly HashSet<string> SupportedAggregateTypes =
    [
        "histogram_scalar_series",
        "topk_patterns_count_vector5_series",
        "histogram_count_vector5_series_per_position_matrix"
    ];

    public void ValidateGetDrawWindow(GetDrawWindowInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }
    }

    public void ValidateComputeWindowMetrics(ComputeWindowMetricsInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Metrics is null || input.Metrics.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "metrics is required.",
                details: new Dictionary<string, object?>
                {
                    ["missing_field"] = "metrics"
                });
        }

        foreach (var metric in input.Metrics)
        {
            if (metric is null || string.IsNullOrWhiteSpace(metric.Name))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "metric item must have a non-empty name.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "metrics[].name"
                    });
            }

            if (!MetricAvailabilityCatalog.IsKnownMetric(metric.Name))
            {
                throw new ApplicationValidationException(
                    code: "UNKNOWN_METRIC",
                    message: "metric name is not listed in the metric catalog.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = metric.Name
                    });
            }

            if (!MetricAvailabilityCatalog.IsExposedInComputeWindowMetrics(metric.Name))
            {
                throw new ApplicationValidationException(
                    code: "UNKNOWN_METRIC",
                    message: "requested metric is known in the catalog but unavailable in this compute_window_metrics build.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = metric.Name,
                        ["allowed_metrics"] = MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics().ToArray()
                    });
            }
        }
    }

    public void ValidateAnalyzeIndicatorStability(AnalyzeIndicatorStabilityInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Indicators is null || input.Indicators.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "indicators is required.",
                details: new Dictionary<string, object?>
                {
                    ["missing_field"] = "indicators"
                });
        }

        if (input.TopK <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "top_k must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["top_k"] = input.TopK
                });
        }

        if (input.MinHistory <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "min_history must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["min_history"] = input.MinHistory
                });
        }

        if (!string.IsNullOrWhiteSpace(input.NormalizationMethod) &&
            !SupportedNormalizationMethods.Contains(input.NormalizationMethod))
        {
            throw new ApplicationValidationException(
                code: "UNSUPPORTED_NORMALIZATION_METHOD",
                message: "normalization_method is not supported.",
                details: new Dictionary<string, object?>
                {
                    ["normalization_method"] = input.NormalizationMethod
                });
        }

        foreach (var indicator in input.Indicators)
        {
            if (indicator is null || string.IsNullOrWhiteSpace(indicator.Name))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "indicator item must have a non-empty name.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "indicators[].name"
                    });
            }
        }
    }

    public void ValidateComposeIndicatorAnalysis(ComposeIndicatorAnalysisInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (string.IsNullOrWhiteSpace(input.Target) ||
            !string.Equals(input.Target, "dezena", StringComparison.Ordinal))
        {
            throw new ApplicationValidationException(
                code: "INCOMPATIBLE_COMPOSITION",
                message: "target must be dezena for this composition recorte.",
                details: new Dictionary<string, object?>
                {
                    ["target"] = input.Target ?? string.Empty
                });
        }

        if (string.IsNullOrWhiteSpace(input.Operator) ||
            !string.Equals(input.Operator, "weighted_rank", StringComparison.Ordinal))
        {
            throw new ApplicationValidationException(
                code: "INCOMPATIBLE_COMPOSITION",
                message: "operator must be weighted_rank for this composition recorte.",
                details: new Dictionary<string, object?>
                {
                    ["operator"] = input.Operator ?? string.Empty
                });
        }

        if (input.TopK is < 1 or > 25)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "top_k must be between 1 and 25 for target dezena.",
                details: new Dictionary<string, object?>
                {
                    ["top_k"] = input.TopK
                });
        }

        if (input.Components is null || input.Components.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "components is required and must be non-empty.",
                details: new Dictionary<string, object?>
                {
                    ["missing_field"] = "components"
                });
        }

        double sum = 0.0;
        foreach (var c in input.Components)
        {
            if (c is null || string.IsNullOrWhiteSpace(c.MetricName))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "component item must have a non-empty metric_name.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "components[].metric_name"
                    });
            }

            if (string.IsNullOrWhiteSpace(c.Transform))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "component item must have a non-empty transform.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "components[].transform"
                    });
            }

            if (!SupportedComposeTransforms.Contains(c.Transform))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "transform is not a supported value.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "components[].transform",
                        ["transform"] = c.Transform
                    });
            }

            sum += c.Weight;
        }

        if (Math.Abs(sum - 1.0) > IndicatorTransformFunctions.WeightSumTolerance)
        {
            throw new ApplicationValidationException(
                code: "INCOMPATIBLE_COMPOSITION",
                message: "component weights must sum to 1.0 within tolerance 1e-9.",
                details: new Dictionary<string, object?>
                {
                    ["weight_sum"] = sum
                });
        }
    }

    public void ValidateAnalyzeIndicatorAssociations(AnalyzeIndicatorAssociationsInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Items is null || input.Items.Count < 2)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "items is required and must list at least two association inputs.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "items"
                });
        }

        if (input.TopK <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "top_k must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["top_k"] = input.TopK
                });
        }

        if (string.IsNullOrWhiteSpace(input.Method))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "method is required.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "method"
                });
        }

        if (!string.Equals(input.Method, "spearman", StringComparison.Ordinal))
        {
            throw new ApplicationValidationException(
                code: "UNSUPPORTED_ASSOCIATION_METHOD",
                message: "this recorte only supports method spearman.",
                details: new Dictionary<string, object?>
                {
                    ["method"] = input.Method
                });
        }

        foreach (var item in input.Items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "each items entry must have a non-empty name.",
                    details: new Dictionary<string, object?>
                {
                    ["field"] = "items[].name"
                });
            }
        }
    }

    public void ValidateSummarizeWindowPatterns(SummarizeWindowPatternsInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Features is null || input.Features.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "features is required and must be non-empty.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "features"
                });
        }

        if (input.CoverageThreshold is < 0d or > 1d)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "coverage_threshold must be in the inclusive range [0,1].",
                details: new Dictionary<string, object?>
                {
                    ["coverage_threshold"] = input.CoverageThreshold
                });
        }

        if (string.IsNullOrWhiteSpace(input.RangeMethod))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "range_method is required.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "range_method"
                });
        }

        if (!SupportedRangeMethods.Contains(input.RangeMethod))
        {
            throw new ApplicationValidationException(
                code: "UNSUPPORTED_RANGE_METHOD",
                message: "this recorte only supports range_method iqr.",
                details: new Dictionary<string, object?>
                {
                    ["range_method"] = input.RangeMethod
                });
        }

        foreach (var feature in input.Features)
        {
            if (feature is null || string.IsNullOrWhiteSpace(feature.MetricName))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "each feature entry must have a non-empty metric_name.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "features[].metric_name"
                    });
            }

            if (!SupportedWindowPatternFeatures.Contains(feature.MetricName))
            {
                throw new ApplicationValidationException(
                    code: "UNKNOWN_METRIC",
                    message: "requested metric is not available in this summarize_window_patterns recorte.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = feature.MetricName
                    });
            }

            if (!string.IsNullOrWhiteSpace(feature.Aggregation) &&
                !string.Equals(feature.Aggregation, "identity", StringComparison.Ordinal))
            {
                throw new ApplicationValidationException(
                    code: "UNSUPPORTED_AGGREGATION",
                    message: "this recorte only supports identity aggregation for scalar features.",
                    details: new Dictionary<string, object?>
                    {
                        ["aggregation"] = feature.Aggregation
                    });
            }
        }
    }

    public void ValidateGenerateCandidateGames(GenerateCandidateGamesInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Plan is null || input.Plan.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "plan is required and must be non-empty.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "plan"
                });
        }

        var totalCount = 0;
        foreach (var planItem in input.Plan)
        {
            if (planItem is null || string.IsNullOrWhiteSpace(planItem.StrategyName))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "each plan entry must have a non-empty strategy_name.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "plan[].strategy_name"
                    });
            }

            if (!SupportedGenerationStrategies.Contains(planItem.StrategyName))
            {
                throw new ApplicationValidationException(
                    code: "UNKNOWN_STRATEGY",
                    message: "requested strategy is not available in this recorte.",
                    details: new Dictionary<string, object?>
                    {
                        ["strategy_name"] = planItem.StrategyName
                    });
            }

            if (planItem.Count is < 1 or > 100)
            {
                throw new ApplicationValidationException(
                    code: "PLAN_BUDGET_EXCEEDED",
                    message: "count must be between 1 and 100 for each plan item.",
                    details: new Dictionary<string, object?>
                    {
                        ["strategy_name"] = planItem.StrategyName,
                        ["count"] = planItem.Count
                    });
            }

            totalCount += planItem.Count;
            var effectiveSearchMethod = string.IsNullOrWhiteSpace(planItem.SearchMethod)
                ? "greedy_topk"
                : planItem.SearchMethod;

            if (!SupportedSearchMethods.Contains(effectiveSearchMethod))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "search_method is not supported.",
                    details: new Dictionary<string, object?>
                    {
                        ["search_method"] = effectiveSearchMethod
                    });
            }

            if ((string.Equals(effectiveSearchMethod, "sampled", StringComparison.Ordinal) ||
                 string.Equals(effectiveSearchMethod, "greedy_topk", StringComparison.Ordinal)) &&
                !input.Seed.HasValue)
            {
                throw new ApplicationValidationException(
                    code: "NON_DETERMINISTIC_CONFIGURATION",
                    message: "seed is required when search_method is sampled or greedy_topk.",
                    details: new Dictionary<string, object?>
                    {
                        ["missing_field"] = "seed",
                        ["search_method"] = effectiveSearchMethod
                    });
            }
        }

        if (totalCount > 250)
        {
            throw new ApplicationValidationException(
                code: "PLAN_BUDGET_EXCEEDED",
                message: "total planned count exceeds maximum budget of 250.",
                details: new Dictionary<string, object?>
                {
                    ["total_count"] = totalCount
                });
        }
    }

    public void ValidateExplainCandidateGames(ExplainCandidateGamesInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Games is null || input.Games.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "games is required and must be non-empty.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "games"
                });
        }

        for (var gameIndex = 0; gameIndex < input.Games.Count; gameIndex++)
        {
            var game = input.Games[gameIndex];
            if (game is null || game.Count != 15)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "each game must contain exactly 15 dezenas.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "games[]",
                        ["game_index"] = gameIndex
                    });
            }

            var seen = new HashSet<int>();
            var previous = 0;
            for (var i = 0; i < game.Count; i++)
            {
                var number = game[i];
                if (number is < 1 or > 25)
                {
                    throw new ApplicationValidationException(
                        code: "INVALID_REQUEST",
                        message: "game dezenas must be within [1, 25].",
                        details: new Dictionary<string, object?>
                        {
                            ["field"] = "games[][]",
                            ["game_index"] = gameIndex,
                            ["number"] = number
                        });
                }

                if (!seen.Add(number))
                {
                    throw new ApplicationValidationException(
                        code: "INVALID_REQUEST",
                        message: "game dezenas must be unique.",
                        details: new Dictionary<string, object?>
                        {
                            ["field"] = "games[][]",
                            ["game_index"] = gameIndex,
                            ["number"] = number
                        });
                }

                if (i > 0 && number <= previous)
                {
                    throw new ApplicationValidationException(
                        code: "INVALID_REQUEST",
                        message: "game dezenas must be strictly increasing.",
                        details: new Dictionary<string, object?>
                        {
                            ["field"] = "games[][]",
                            ["game_index"] = gameIndex
                        });
                }

                previous = number;
            }
        }
    }

    public void ValidateSummarizeWindowAggregates(SummarizeWindowAggregatesInput input)
    {
        if (input.WindowSize <= 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_WINDOW_SIZE",
                message: "window_size must be greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["window_size"] = input.WindowSize
                });
        }

        if (input.Aggregates is null || input.Aggregates.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "aggregates is required and must be non-empty.",
                details: new Dictionary<string, object?>
                {
                    ["missing_field"] = "aggregates"
                });
        }

        foreach (var aggregate in input.Aggregates)
        {
            if (aggregate is null)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "aggregate entries must be objects.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "aggregates[]"
                    });
            }

            if (string.IsNullOrWhiteSpace(aggregate.SourceMetricName))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "source_metric_name is required.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "aggregates[].source_metric_name"
                    });
            }

            if (string.IsNullOrWhiteSpace(aggregate.AggregateType))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "aggregate_type is required.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "aggregates[].aggregate_type"
                    });
            }

            if (!SupportedAggregateTypes.Contains(aggregate.AggregateType))
            {
                throw new ApplicationValidationException(
                    code: "UNSUPPORTED_AGGREGATE_TYPE",
                    message: "aggregate_type is not supported.",
                    details: new Dictionary<string, object?>
                    {
                        ["aggregate_type"] = aggregate.AggregateType
                    });
            }

            if (aggregate.Params.ValueKind != JsonValueKind.Object)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "params must be an object.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "aggregates[].params"
                    });
            }

            switch (aggregate.AggregateType)
            {
                case "histogram_scalar_series":
                    ValidateHistogramScalarSeriesParams(aggregate.Params);
                    break;
                case "topk_patterns_count_vector5_series":
                    ValidateTopkPatternsParams(aggregate.Params);
                    break;
                case "histogram_count_vector5_series_per_position_matrix":
                    ValidateMatrixParams(aggregate.Params);
                    break;
            }
        }
    }

    private static void ValidateHistogramScalarSeriesParams(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("bucket_spec", out var bucketSpec) || bucketSpec.ValueKind != JsonValueKind.Object)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_spec is required for histogram_scalar_series.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec"
                });
        }

        var hasBucketValues = bucketSpec.TryGetProperty("bucket_values", out var bucketValues);
        var hasMin = bucketSpec.TryGetProperty("min", out _);
        var hasMax = bucketSpec.TryGetProperty("max", out _);
        var hasWidth = bucketSpec.TryGetProperty("width", out _);

        var usesDiscrete = hasBucketValues;
        var usesContinuous = hasMin || hasMax || hasWidth;
        if (usesDiscrete == usesContinuous)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_spec must use exactly one mode (bucket_values or min/max/width).",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec"
                });
        }

        if (usesDiscrete && (bucketValues.ValueKind != JsonValueKind.Array || bucketValues.GetArrayLength() == 0))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "bucket_values must be a non-empty array.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec.bucket_values"
                });
        }

        if (usesContinuous && (!hasMin || !hasMax || !hasWidth))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "continuous bucket_spec requires min, max and width.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.bucket_spec"
                });
        }
    }

    private static void ValidateTopkPatternsParams(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("top_k", out var topKElement) ||
            topKElement.ValueKind != JsonValueKind.Number ||
            !topKElement.TryGetInt32(out var topK) ||
            topK < 1)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "top_k must be an integer greater than zero.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.top_k"
                });
        }
    }

    private static void ValidateMatrixParams(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("value_min", out var valueMinElement) ||
            valueMinElement.ValueKind != JsonValueKind.Number ||
            !valueMinElement.TryGetInt32(out var valueMin))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "value_min must be an integer.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.value_min"
                });
        }

        if (!parameters.TryGetProperty("value_max", out var valueMaxElement) ||
            valueMaxElement.ValueKind != JsonValueKind.Number ||
            !valueMaxElement.TryGetInt32(out var valueMax))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "value_max must be an integer.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "aggregates[].params.value_max"
                });
        }

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
    }
}

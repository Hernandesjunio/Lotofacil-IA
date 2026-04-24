using LotofacilMcp.Application.Composition;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Domain.Generation;
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
        "pares_no_concurso",
        "repeticao_concurso_anterior",
        "quantidade_vizinhos_por_concurso",
        "sequencia_maxima_vizinhos_por_concurso",
        "entropia_linha_por_concurso",
        "entropia_coluna_por_concurso",
        "hhi_linha_por_concurso",
        "hhi_coluna_por_concurso"
    ];

    private static readonly HashSet<string> SupportedRangeMethods =
    [
        "iqr"
    ];

    private static readonly HashSet<string> SupportedGenerationStrategies =
    [
        "common_repetition_frequency",
        "declared_composite_profile"
    ];

    private static readonly HashSet<string> SupportedSearchMethods =
    [
        "exhaustive",
        "sampled",
        "greedy_topk"
    ];

    private static readonly HashSet<string> SupportedGenerationFilters =
    [
        "max_consecutive_run",
        "max_neighbor_count",
        "min_row_entropy_norm",
        "max_hhi_linha",
        "repeat_range",
        "min_slot_alignment",
        "max_outlier_score"
    ];

    private static readonly HashSet<string> SupportedAggregateTypes =
    [
        "histogram_scalar_series",
        "topk_patterns_count_vector5_series",
        "histogram_count_vector5_series_per_position_matrix"
    ];

    private static readonly HashSet<string> SupportedConstraintModes =
    [
        "hard",
        "soft"
    ];

    private static readonly HashSet<string> SupportedSoftFilters =
    [
        "max_consecutive_run",
        "max_neighbor_count"
    ];

    private static readonly HashSet<string> SupportedTypicalRangeMethods =
    [
        "iqr",
        "percentile"
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
                throw CreateUnknownMetricForRoute(
                    metric.Name,
                    "metric name is not listed in the metric catalog.");
            }

            if (!MetricAvailabilityCatalog.IsExposedInComputeWindowMetrics(metric.Name))
            {
                throw CreateUnknownMetricForRoute(
                    metric.Name,
                    "requested metric is known in the catalog but unavailable in this compute_window_metrics build.",
                    MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics());
            }

            if (MetricAvailabilityCatalog.IsPendingMetric(metric.Name) && !input.AllowPending)
            {
                throw CreateUnknownMetricForRoute(
                    metric.Name,
                    "requested metric is pending and requires allow_pending opt-in.",
                    MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics(allowPending: false),
                    reason: "pending_requires_opt_in");
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

            if (!MetricAvailabilityCatalog.IsKnownMetric(c.MetricName))
            {
                throw CreateUnknownMetricForRoute(
                    c.MetricName,
                    "metric name is not listed in the metric catalog.");
            }

            if (!MetricAvailabilityCatalog.IsExposedInComposeIndicatorAnalysis(c.MetricName))
            {
                throw CreateUnknownMetricForRoute(
                    c.MetricName,
                    "requested metric is known in the catalog but unavailable as compose component in this build.",
                    MetricAvailabilityCatalog.GetComposeIndicatorAnalysisAllowedComponents());
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

        if (input.StabilityCheck is not null)
        {
            if (!string.Equals(input.StabilityCheck.Method, "rolling_window", StringComparison.Ordinal))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "stability_check.method must be rolling_window.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "stability_check.method",
                        ["method"] = input.StabilityCheck.Method
                    });
            }

            if (input.StabilityCheck.SubwindowSize <= 1)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "stability_check.subwindow_size must be greater than 1.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "stability_check.subwindow_size",
                        ["subwindow_size"] = input.StabilityCheck.SubwindowSize
                    });
            }

            if (input.StabilityCheck.SubwindowSize > input.WindowSize)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "stability_check.subwindow_size must be less than or equal to window_size.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "stability_check.subwindow_size",
                        ["subwindow_size"] = input.StabilityCheck.SubwindowSize,
                        ["window_size"] = input.WindowSize
                    });
            }

            if (input.StabilityCheck.Stride < 1)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "stability_check.stride must be greater than or equal to 1.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "stability_check.stride",
                        ["stride"] = input.StabilityCheck.Stride
                    });
            }

            if (input.StabilityCheck.MinSubwindows < 2)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "stability_check.min_subwindows must be greater than or equal to 2.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "stability_check.min_subwindows",
                        ["min_subwindows"] = input.StabilityCheck.MinSubwindows
                    });
            }
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

            if (!MetricAvailabilityCatalog.IsKnownMetric(item.Name))
            {
                throw CreateUnknownMetricForRoute(
                    item.Name,
                    "metric name is not listed in the metric catalog.");
            }

            if (!MetricAvailabilityCatalog.IsExposedInAnalyzeIndicatorAssociations(item.Name))
            {
                throw CreateUnknownMetricForRoute(
                    item.Name,
                    "requested metric is known in the catalog but unavailable in this analyze_indicator_associations build.",
                    MetricAvailabilityCatalog.GetAnalyzeIndicatorAssociationsAllowedIndicators());
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

            if (!MetricAvailabilityCatalog.IsKnownMetric(feature.MetricName))
            {
                throw CreateUnknownMetricForRoute(
                    feature.MetricName,
                    "metric name is not listed in the metric catalog.");
            }

            var metricCapability = MetricAvailabilityCatalog.GetRegistryEntries()
                .First(capability => string.Equals(capability.MetricName, feature.MetricName, StringComparison.Ordinal));

            if (!string.Equals(metricCapability.Scope, "series", StringComparison.Ordinal))
            {
                throw new ApplicationValidationException(
                    code: "UNSUPPORTED_PATTERN_FEATURE",
                    message: "feature is incompatible with summarize_window_patterns; use summarize_window_aggregates for non-series summaries.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = feature.MetricName,
                        ["scope"] = metricCapability.Scope,
                        ["shape"] = metricCapability.Shape,
                        ["suggested_tool"] = "summarize_window_aggregates"
                    });
            }

            if (!string.Equals(metricCapability.Shape, "series", StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(feature.Aggregation))
                {
                    throw new ApplicationValidationException(
                        code: "UNSUPPORTED_AGGREGATION",
                        message: "non-scalar features require explicit aggregation; use summarize_window_aggregates for canonical vector summaries.",
                        details: new Dictionary<string, object?>
                        {
                            ["metric_name"] = feature.MetricName,
                            ["shape"] = metricCapability.Shape,
                            ["missing_field"] = "features[].aggregation",
                            ["suggested_tool"] = "summarize_window_aggregates"
                        });
                }

                throw new ApplicationValidationException(
                    code: "UNSUPPORTED_PATTERN_FEATURE",
                    message: "feature is incompatible with summarize_window_patterns; use summarize_window_aggregates for vector-derived summaries.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = feature.MetricName,
                        ["shape"] = metricCapability.Shape,
                        ["aggregation"] = feature.Aggregation,
                        ["suggested_tool"] = "summarize_window_aggregates"
                    });
            }

            if (!SupportedWindowPatternFeatures.Contains(feature.MetricName))
            {
                throw new ApplicationValidationException(
                    code: "UNSUPPORTED_PATTERN_FEATURE",
                    message: "requested scalar feature is not available in this summarize_window_patterns recorte.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = feature.MetricName,
                        ["supported_features"] = SupportedWindowPatternFeatures.OrderBy(static name => name, StringComparer.Ordinal).ToArray()
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
                        ["metric_name"] = feature.MetricName,
                        ["aggregation"] = feature.Aggregation
                    });
            }
        }
    }

    public void ValidateGenerateCandidateGames(GenerateCandidateGamesInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.GenerationMode))
        {
            var trimmed = input.GenerationMode.Trim();
            if (!string.Equals(trimmed, GenerationModes.RandomUnrestricted, StringComparison.Ordinal) &&
                !string.Equals(trimmed, GenerationModes.BehaviorFiltered, StringComparison.Ordinal))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "generation_mode must be random_unrestricted or behavior_filtered.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "generation_mode",
                        ["value"] = input.GenerationMode
                    });
            }
        }

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

        if (input.StructuralExclusions?.RepeatRange is not null)
        {
            var repeatRange = input.StructuralExclusions.RepeatRange;
            if (!repeatRange.Min.HasValue || !repeatRange.Max.HasValue || repeatRange.Min.Value > repeatRange.Max.Value)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "structural_exclusions.repeat_range must define min and max with min <= max.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "structural_exclusions.repeat_range"
                    });
            }
        }

        if (input.GenerationBudget is not null)
        {
            if (input.GenerationBudget.MaxAttempts.HasValue && input.GenerationBudget.MaxAttempts.Value <= 0)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "generation_budget.max_attempts must be greater than zero.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "generation_budget.max_attempts",
                        ["max_attempts"] = input.GenerationBudget.MaxAttempts.Value
                    });
            }

            if (input.GenerationBudget.PoolMultiplier.HasValue &&
                (!double.IsFinite(input.GenerationBudget.PoolMultiplier.Value) || input.GenerationBudget.PoolMultiplier.Value <= 0d))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "generation_budget.pool_multiplier must be a finite number greater than zero.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = "generation_budget.pool_multiplier",
                        ["pool_multiplier"] = input.GenerationBudget.PoolMultiplier.Value
                    });
            }
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
            var effectiveSearchMethod = ResolveDefaultSearchMethod(planItem.StrategyName, planItem.SearchMethod);

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

            if (planItem.Criteria is { Count: > 0 })
            {
                foreach (var criterion in planItem.Criteria)
                {
                    if (criterion is null || string.IsNullOrWhiteSpace(criterion.Name))
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "criteria entries must define name.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].criteria[].name"
                            });
                    }

                    ValidateConstraintMode(
                        hasLegacyMode: criterion.Value.HasValue,
                        hasRangeMode: criterion.Range is not null,
                        hasAllowedValuesMode: criterion.AllowedValues is not null,
                        hasTypicalRangeMode: criterion.TypicalRange is not null,
                        fieldPrefix: "plan[].criteria[]");

                    ValidateConstraintModeValue(criterion.Mode, "plan[].criteria[].mode");
                    ValidateSoftModeSupportForCriteria(criterion.Name, criterion.Mode, "plan[].criteria[].mode");

                    if (criterion.Range is not null && criterion.Range.Min > criterion.Range.Max)
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "range requires min <= max.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].criteria[].range"
                            });
                    }

                    if (criterion.AllowedValues is not null)
                    {
                        ValidateAllowedValues(
                            criterion.Name,
                            criterion.AllowedValues.Values,
                            "plan[].criteria[].allowed_values.values");
                    }

                    if (criterion.TypicalRange is not null)
                    {
                        ValidateTypicalRange(criterion.TypicalRange, "plan[].criteria[].typical_range");
                    }
                }
            }

            if (planItem.Weights is { Count: > 0 })
            {
                double weightSum = 0d;
                foreach (var weight in planItem.Weights)
                {
                    if (weight is null || string.IsNullOrWhiteSpace(weight.Name))
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "weights entries must define name.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].weights[].name"
                            });
                    }

                    weightSum += weight.Weight;
                }

                if (Math.Abs(weightSum - 1.0d) > IndicatorTransformFunctions.WeightSumTolerance)
                {
                    throw new ApplicationValidationException(
                        code: "INCOMPATIBLE_COMPOSITION",
                        message: "weights must sum to 1.0 within tolerance 1e-9.",
                        details: new Dictionary<string, object?>
                        {
                            ["weight_sum"] = weightSum
                        });
                }
            }

            if (planItem.Filters is { Count: > 0 })
            {
                foreach (var filter in planItem.Filters)
                {
                    if (filter is null || string.IsNullOrWhiteSpace(filter.Name))
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "filters entries must define name.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].filters[].name"
                            });
                    }

                    if (!SupportedGenerationFilters.Contains(filter.Name))
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "filter name is not supported in this build.",
                            details: new Dictionary<string, object?>
                            {
                                ["filter_name"] = filter.Name
                            });
                    }

                    if (string.Equals(filter.Name, "repeat_range", StringComparison.Ordinal))
                    {
                        ValidateConstraintMode(
                            hasLegacyMode: filter.Value.HasValue || filter.Min.HasValue || filter.Max.HasValue,
                            hasRangeMode: filter.Range is not null,
                            hasAllowedValuesMode: filter.AllowedValues is not null,
                            hasTypicalRangeMode: filter.TypicalRange is not null,
                            fieldPrefix: "plan[].filters[].repeat_range");
                    }
                    else
                    {
                        ValidateConstraintMode(
                            hasLegacyMode: filter.Value.HasValue || filter.Min.HasValue || filter.Max.HasValue,
                            hasRangeMode: filter.Range is not null,
                            hasAllowedValuesMode: filter.AllowedValues is not null,
                            hasTypicalRangeMode: filter.TypicalRange is not null,
                            fieldPrefix: "plan[].filters[]");
                    }

                    ValidateConstraintModeValue(filter.Mode, "plan[].filters[].mode");
                    ValidateSoftModeSupportForFilter(filter.Name, filter.Mode, "plan[].filters[].mode");

                    if (filter.Range is not null && filter.Range.Min > filter.Range.Max)
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "range requires min <= max.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].filters[].range"
                            });
                    }

                    if (filter.AllowedValues is not null)
                    {
                        ValidateAllowedValues(
                            filter.Name,
                            filter.AllowedValues.Values,
                            "plan[].filters[].allowed_values.values");
                    }

                    if (filter.TypicalRange is not null)
                    {
                        ValidateTypicalRange(filter.TypicalRange, "plan[].filters[].typical_range");
                    }

                    if (string.Equals(filter.Name, "repeat_range", StringComparison.Ordinal))
                    {
                        if (filter.Range is not null)
                        {
                            continue;
                        }

                        if (filter.TypicalRange is not null)
                        {
                            continue;
                        }

                        if (!filter.Min.HasValue || !filter.Max.HasValue || filter.Min.Value > filter.Max.Value)
                        {
                            throw new ApplicationValidationException(
                                code: "INVALID_REQUEST",
                                message: "repeat_range requires min and max with min <= max.",
                                details: new Dictionary<string, object?>
                                {
                                    ["field"] = "plan[].filters[].repeat_range"
                                });
                        }
                    }
                    else if (filter.Range is null && filter.AllowedValues is null && filter.TypicalRange is null && !filter.Value.HasValue)
                    {
                        throw new ApplicationValidationException(
                            code: "INVALID_REQUEST",
                            message: "numeric filters require value.",
                            details: new Dictionary<string, object?>
                            {
                                ["field"] = "plan[].filters[].value",
                                ["filter_name"] = filter.Name
                            });
                    }
                }
            }
        }

        if (totalCount > GenerationRequestLimits.MaxSumPlanCountPerRequest)
        {
            throw new ApplicationValidationException(
                code: "PLAN_BUDGET_EXCEEDED",
                message: $"total planned count exceeds maximum budget of {GenerationRequestLimits.MaxSumPlanCountPerRequest}; split into additional requests (multiple rounds) for larger volume.",
                details: new Dictionary<string, object?>
                {
                    ["total_count"] = totalCount,
                    ["max_sum_plan_count_per_request"] = GenerationRequestLimits.MaxSumPlanCountPerRequest
                });
        }
    }

    private static void ValidateConstraintMode(
        bool hasLegacyMode,
        bool hasRangeMode,
        bool hasAllowedValuesMode,
        bool hasTypicalRangeMode,
        string fieldPrefix)
    {
        var selectedModes = (hasLegacyMode ? 1 : 0) +
                            (hasRangeMode ? 1 : 0) +
                            (hasAllowedValuesMode ? 1 : 0) +
                            (hasTypicalRangeMode ? 1 : 0);
        if (selectedModes == 1)
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: "mixed or missing constraint mode is not allowed; use exactly one of legacy value/min/max, range, allowed_values, or typical_range.",
            details: new Dictionary<string, object?>
            {
                ["field"] = fieldPrefix
            });
    }

    private static void ValidateTypicalRange(GenerateTypicalRangeSpecInput typicalRange, string fieldPrefix)
    {
        if (string.IsNullOrWhiteSpace(typicalRange.MetricName))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "typical_range.metric_name is required.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = $"{fieldPrefix}.metric_name"
                });
        }

        if (!MetricAvailabilityCatalog.IsKnownMetric(typicalRange.MetricName))
        {
            throw CreateUnknownMetricForRoute(
                metricName: typicalRange.MetricName,
                message: "metric name is not listed in the metric catalog.");
        }

        if (string.IsNullOrWhiteSpace(typicalRange.Method) ||
            !SupportedTypicalRangeMethods.Contains(typicalRange.Method))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "typical_range.method must be one of: iqr, percentile.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = $"{fieldPrefix}.method",
                    ["method"] = typicalRange.Method
                });
        }

        if (typicalRange.Coverage is < 0d or > 1d)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "typical_range.coverage must be in the inclusive range [0,1].",
                details: new Dictionary<string, object?>
                {
                    ["field"] = $"{fieldPrefix}.coverage",
                    ["coverage"] = typicalRange.Coverage
                });
        }

        if (!string.Equals(typicalRange.Method, "percentile", StringComparison.Ordinal))
        {
            return;
        }

        var pLow = typicalRange.Params?.PLow;
        var pHigh = typicalRange.Params?.PHigh;
        var valid =
            pLow.HasValue &&
            pHigh.HasValue &&
            double.IsFinite(pLow.Value) &&
            double.IsFinite(pHigh.Value) &&
            pLow.Value >= 0d &&
            pHigh.Value <= 1d &&
            pLow.Value < pHigh.Value;

        if (valid)
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: "typical_range.method=percentile requires valid params with 0 <= p_low < p_high <= 1.",
            details: new Dictionary<string, object?>
            {
                ["field"] = $"{fieldPrefix}.params",
                ["p_low"] = pLow,
                ["p_high"] = pHigh
            });
    }

    private static void ValidateConstraintModeValue(string? mode, string field)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return;
        }

        if (SupportedConstraintModes.Contains(mode))
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: "mode must be one of: hard, soft.",
            details: new Dictionary<string, object?>
            {
                ["field"] = field,
                ["mode"] = mode
            });
    }

    private static void ValidateSoftModeSupportForCriteria(string criterionName, string? mode, string field)
    {
        if (!string.Equals(mode, "soft", StringComparison.Ordinal))
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: "mode=soft is not supported for criteria in this recorte.",
            details: new Dictionary<string, object?>
            {
                ["field"] = field,
                ["mode"] = mode,
                ["criterion_name"] = criterionName
            });
    }

    private static void ValidateSoftModeSupportForFilter(string filterName, string? mode, string field)
    {
        if (!string.Equals(mode, "soft", StringComparison.Ordinal))
        {
            return;
        }

        if (SupportedSoftFilters.Contains(filterName))
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: "mode=soft is not supported for this filter in this recorte.",
            details: new Dictionary<string, object?>
            {
                ["field"] = field,
                ["mode"] = mode,
                ["filter_name"] = filterName
            });
    }

    private static void ValidateAllowedValues(
        string constraintName,
        IReadOnlyList<double>? values,
        string field)
    {
        if (values is null || values.Count == 0)
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "allowed_values.values must be a non-empty array.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = field
                });
        }

        var requiresInteger = RequiresIntegerConstraint(constraintName);
        foreach (var value in values)
        {
            if (!double.IsFinite(value))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "allowed_values.values must contain finite numbers.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = field
                    });
            }

            if (requiresInteger && !IsInteger(value))
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "allowed_values.values must contain integers for this constraint.",
                    details: new Dictionary<string, object?>
                    {
                        ["field"] = field,
                        ["constraint_name"] = constraintName
                    });
            }
        }
    }

    private static bool RequiresIntegerConstraint(string name)
    {
        return name.Contains("count", StringComparison.Ordinal) ||
               name.Contains("repeat", StringComparison.Ordinal) ||
               name.Contains("pairs", StringComparison.Ordinal) ||
               name.Contains("neighbor", StringComparison.Ordinal) ||
               name.Contains("top10_overlap", StringComparison.Ordinal) ||
               string.Equals(name, "max_consecutive_run", StringComparison.Ordinal) ||
               string.Equals(name, "min_top10_overlap", StringComparison.Ordinal);
    }

    private static bool IsInteger(double value)
    {
        return Math.Abs(value - Math.Round(value)) <= 1e-9d;
    }

    private static string ResolveDefaultSearchMethod(string strategyName, string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return string.Equals(strategyName, "declared_composite_profile", StringComparison.Ordinal)
            ? "sampled"
            : "greedy_topk";
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

            if (!MetricAvailabilityCatalog.IsKnownMetric(aggregate.SourceMetricName))
            {
                throw CreateUnknownMetricForRoute(
                    aggregate.SourceMetricName,
                    "metric name is not listed in the metric catalog.");
            }

            if (!MetricAvailabilityCatalog.IsExposedInSummarizeWindowAggregates(aggregate.SourceMetricName))
            {
                throw CreateUnknownMetricForRoute(
                    aggregate.SourceMetricName,
                    "requested metric is known in the catalog but unavailable in this summarize_window_aggregates build.",
                    MetricAvailabilityCatalog.GetSummarizeWindowAggregatesAllowedSources());
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

    private static ApplicationValidationException CreateUnknownMetricForRoute(
        string metricName,
        string message,
        IReadOnlyList<string>? allowedMetrics = null,
        string? reason = null)
    {
        var details = new Dictionary<string, object?>
        {
            ["metric_name"] = metricName
        };

        if (allowedMetrics is not null)
        {
            details["allowed_metrics"] = allowedMetrics.ToArray();
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            details["reason"] = reason;
        }

        return new ApplicationValidationException(
            code: "UNKNOWN_METRIC",
            message: message,
            details: details);
    }
}

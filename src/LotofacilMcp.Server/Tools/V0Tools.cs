using System.Text.Json.Serialization;
using System.Text.Json;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Application.Windows;
using LotofacilMcp.Domain.Analytics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Normalization;
using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.CanonicalJson;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Hashing;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Server.Tools;

public sealed record MetricRequest([property: JsonPropertyName("name")] string Name);

public sealed record ComputeWindowMetricsRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("metrics")] IReadOnlyList<MetricRequest>? Metrics = null,
    [property: JsonPropertyName("allow_pending")] bool AllowPending = false,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record GetDrawWindowRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record DiscoverCapabilitiesRequest(
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record StabilityIndicatorRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("aggregation")] string? Aggregation);

public sealed record AnalyzeIndicatorStabilityRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("indicators")] IReadOnlyList<StabilityIndicatorRequestDto>? Indicators = null,
    [property: JsonPropertyName("normalization_method")] string? NormalizationMethod = null,
    [property: JsonPropertyName("top_k")] int TopK = 5,
    [property: JsonPropertyName("min_history")] int MinHistory = 20,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record ContractError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("details")] IReadOnlyDictionary<string, object?> Details);

public sealed record ContractErrorEnvelope([property: JsonPropertyName("error")] ContractError Error);

public sealed record WindowEnvelope(
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("start_contest_id")] int StartContestId,
    [property: JsonPropertyName("end_contest_id")] int EndContestId);

public sealed record MetricValueEnvelope(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("shape")] string Shape,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("value")] IReadOnlyList<double> Value,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record ComputeWindowMetricsResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("metrics")] IReadOnlyList<MetricValueEnvelope> Metrics);

public sealed record DrawDto(
    [property: JsonPropertyName("contest_id")] int ContestId,
    [property: JsonPropertyName("draw_date")] string DrawDate,
    [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);

public sealed record GetDrawWindowResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("draws")] IReadOnlyList<DrawDto> Draws);

public sealed record StabilityRankingEntryEnvelope(
    [property: JsonPropertyName("indicator_name")] string IndicatorName,
    [property: JsonPropertyName("aggregation")] string Aggregation,
    [property: JsonPropertyName("component_index")] int? ComponentIndex,
    [property: JsonPropertyName("shape")] string Shape,
    [property: JsonPropertyName("dispersion")] double Dispersion,
    [property: JsonPropertyName("stability_score")] double StabilityScore,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record AnalyzeIndicatorStabilityResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("normalization_method")] string NormalizationMethod,
    [property: JsonPropertyName("ranking")] IReadOnlyList<StabilityRankingEntryEnvelope> Ranking);

public sealed record ComposeIndicatorComponentRequest(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("transform")] string Transform,
    [property: JsonPropertyName("weight")] double Weight);

public sealed record ComposeIndicatorAnalysisRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("target")] string Target = "",
    [property: JsonPropertyName("operator")] string Operator = "",
    [property: JsonPropertyName("components")] IReadOnlyList<ComposeIndicatorComponentRequest>? Components = null,
    [property: JsonPropertyName("top_k")] int TopK = 10,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record WeightedDezenaRankingEntryEnvelope(
    [property: JsonPropertyName("dezena")] int Dezena,
    [property: JsonPropertyName("rank")] int Rank,
    [property: JsonPropertyName("score")] double Score,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record ComposeIndicatorAnalysisResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("target")] string Target,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("ranking")] IReadOnlyList<WeightedDezenaRankingEntryEnvelope> Ranking);

public sealed record AssociationItemRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("aggregation")] string? Aggregation);

public sealed record AnalyzeIndicatorAssociationsRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("items")] IReadOnlyList<AssociationItemRequest>? Items = null,
    [property: JsonPropertyName("method")] string Method = "",
    [property: JsonPropertyName("top_k")] int TopK = 5,
    [property: JsonPropertyName("stability_check")] AssociationStabilityCheckRequest? StabilityCheck = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record AssociationStabilityCheckRequest(
    [property: JsonPropertyName("method")] string Method = "",
    [property: JsonPropertyName("subwindow_size")] int SubwindowSize = 0,
    [property: JsonPropertyName("stride")] int Stride = 0,
    [property: JsonPropertyName("min_subwindows")] int MinSubwindows = 0);

public sealed record AssociationMagnitudeEntryEnvelope(
    [property: JsonPropertyName("indicator_a")] string IndicatorA,
    [property: JsonPropertyName("aggregation_a")] string AggregationA,
    [property: JsonPropertyName("component_index_a")] int? ComponentIndexA,
    [property: JsonPropertyName("indicator_b")] string IndicatorB,
    [property: JsonPropertyName("aggregation_b")] string AggregationB,
    [property: JsonPropertyName("component_index_b")] int? ComponentIndexB,
    [property: JsonPropertyName("association_strength")] double AssociationStrength,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record AssociationMagnitudeEnvelope(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("top_pairs")] IReadOnlyList<AssociationMagnitudeEntryEnvelope> TopPairs);

public sealed record AssociationStabilityEntryEnvelope(
    [property: JsonPropertyName("indicator_a")] string IndicatorA,
    [property: JsonPropertyName("aggregation_a")] string AggregationA,
    [property: JsonPropertyName("component_index_a")] int? ComponentIndexA,
    [property: JsonPropertyName("indicator_b")] string IndicatorB,
    [property: JsonPropertyName("aggregation_b")] string AggregationB,
    [property: JsonPropertyName("component_index_b")] int? ComponentIndexB,
    [property: JsonPropertyName("mean")] double Mean,
    [property: JsonPropertyName("median")] double Median,
    [property: JsonPropertyName("p10")] double P10,
    [property: JsonPropertyName("p90")] double P90,
    [property: JsonPropertyName("min")] double Min,
    [property: JsonPropertyName("max")] double Max,
    [property: JsonPropertyName("stddev")] double StdDev,
    [property: JsonPropertyName("sign_consistency_ratio")] double SignConsistencyRatio);

public sealed record AssociationStabilityEnvelope(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("subwindow_size")] int SubwindowSize,
    [property: JsonPropertyName("stride")] int Stride,
    [property: JsonPropertyName("min_subwindows")] int MinSubwindows,
    [property: JsonPropertyName("subwindows_count")] int SubwindowsCount,
    [property: JsonPropertyName("top_pairs")] IReadOnlyList<AssociationStabilityEntryEnvelope> TopPairs);

public sealed record AnalyzeIndicatorAssociationsResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("association_magnitude")] AssociationMagnitudeEnvelope AssociationMagnitude,
    [property: JsonPropertyName("association_stability"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AssociationStabilityEnvelope? AssociationStability);

public sealed record WindowPatternFeatureRequest(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("aggregation")] string? Aggregation);

public sealed record SummarizeWindowPatternsRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("features")] IReadOnlyList<WindowPatternFeatureRequest>? Features = null,
    [property: JsonPropertyName("coverage_threshold")] double CoverageThreshold = 0.8,
    [property: JsonPropertyName("range_method")] string RangeMethod = "iqr",
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record WindowPatternSummaryEnvelope(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("aggregation")] string Aggregation,
    [property: JsonPropertyName("mode")] double Mode,
    [property: JsonPropertyName("q1")] double Q1,
    [property: JsonPropertyName("median")] double Median,
    [property: JsonPropertyName("q3")] double Q3,
    [property: JsonPropertyName("iqr")] double Iqr,
    [property: JsonPropertyName("coverage_observed")] double CoverageObserved,
    [property: JsonPropertyName("coverage_count")] int CoverageCount,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("outlier_count")] int OutlierCount,
    [property: JsonPropertyName("outlier_lower_fence")] double OutlierLowerFence,
    [property: JsonPropertyName("outlier_upper_fence")] double OutlierUpperFence,
    [property: JsonPropertyName("coverage_threshold_met")] bool CoverageThresholdMet,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record SummarizeWindowPatternsResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("range_method")] string RangeMethod,
    [property: JsonPropertyName("coverage_threshold")] double CoverageThreshold,
    [property: JsonPropertyName("summaries")] IReadOnlyList<WindowPatternSummaryEnvelope> Summaries);

public sealed record WindowAggregateRequestDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("source_metric_name")] string? SourceMetricName,
    [property: JsonPropertyName("aggregate_type")] string? AggregateType,
    [property: JsonPropertyName("params")] JsonElement Params);

public sealed record SummarizeWindowAggregatesRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("aggregates")] IReadOnlyList<WindowAggregateRequestDto>? Aggregates = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record HistogramBucketEnvelope(
    [property: JsonPropertyName("x")] double X,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("ratio"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? Ratio);

public sealed record PatternCountItemEnvelope(
    [property: JsonPropertyName("pattern")] IReadOnlyList<int> Pattern,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("ratio"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? Ratio);

public sealed record WindowAggregateEnvelope(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("source_metric_name")] string SourceMetricName,
    [property: JsonPropertyName("aggregate_type")] string AggregateType,
    [property: JsonPropertyName("buckets"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyList<HistogramBucketEnvelope>? Buckets,
    [property: JsonPropertyName("items"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyList<PatternCountItemEnvelope>? Items,
    [property: JsonPropertyName("matrix"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyList<IReadOnlyList<int>>? Matrix);

public sealed record SummarizeWindowAggregatesResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("aggregates")] IReadOnlyList<WindowAggregateEnvelope> Aggregates);

public sealed record PromptTemplateSummaryEnvelope(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("resource_uri")] string ResourceUri,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("suggested_windows")] string SuggestedWindows);

public sealed record HelpResponse(
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("getting_started_resource_uri"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? GettingStartedResourceUri,
    [property: JsonPropertyName("quick_start_markdown"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? QuickStartMarkdown,
    [property: JsonPropertyName("index_resource_uri")] string IndexResourceUri,
    [property: JsonPropertyName("index_markdown")] string IndexMarkdown,
    [property: JsonPropertyName("templates")] IReadOnlyList<PromptTemplateSummaryEnvelope> Templates);

public sealed record WindowModesByToolEnvelope(
    [property: JsonPropertyName("tool_name")] string ToolName,
    [property: JsonPropertyName("supported_modes")] IReadOnlyList<string> SupportedModes);

public sealed record ToolCapabilityEnvelope(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("supported_parameters")] IReadOnlyDictionary<string, IReadOnlyList<string>> SupportedParameters,
    [property: JsonPropertyName("capabilities")] string Capabilities);

public sealed record MetricsCapabilitiesEnvelope(
    [property: JsonPropertyName("implemented_metric_names")] IReadOnlyList<string> ImplementedMetricNames,
    [property: JsonPropertyName("pending_metric_names")] IReadOnlyList<string> PendingMetricNames,
    [property: JsonPropertyName("compute_window_metrics_allowed")] IReadOnlyList<string> ComputeWindowMetricsAllowed,
    [property: JsonPropertyName("summarize_window_aggregates_allowed_sources")] IReadOnlyList<string> SummarizeWindowAggregatesAllowedSources,
    [property: JsonPropertyName("association_allowed_indicators")] IReadOnlyList<string> AssociationAllowedIndicators);

public sealed record GenerationStrategyEnvelope(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version);

public sealed record SearchMethodSeedPolicyEnvelope(
    [property: JsonPropertyName("search_method")] string SearchMethod,
    [property: JsonPropertyName("seed_required_for_replay_guaranteed")]
    bool SeedRequiredForReplayGuaranteed);

public sealed record GenerationCapabilitiesEnvelope(
    [property: JsonPropertyName("strategies")] IReadOnlyList<GenerationStrategyEnvelope> Strategies,
    [property: JsonPropertyName("search_methods")] IReadOnlyList<string> SearchMethods,
    [property: JsonPropertyName("supported_filters")] IReadOnlyList<string> SupportedFilters,
    [property: JsonPropertyName("max_sum_plan_count_per_request")] int MaxSumPlanCountPerRequest,
    [property: JsonPropertyName("supported_generation_modes")] IReadOnlyList<string> SupportedGenerationModes,
    [property: JsonPropertyName("request_seed_optional")] bool RequestSeedOptional,
    [property: JsonPropertyName("seed_by_search_method")] IReadOnlyList<SearchMethodSeedPolicyEnvelope> SeedBySearchMethod);

public sealed record DiscoverCapabilitiesResponse(
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("build_profile")] string BuildProfile,
    [property: JsonPropertyName("dataset_requirements")] IReadOnlyList<string> DatasetRequirements,
    // ADR 0023 (D2/D5) hotfixes: declare UX invariants for Content channel and knobs usage.
    [property: JsonPropertyName("content_channel_rules")] IReadOnlyList<string> ContentChannelRules,
    [property: JsonPropertyName("knobs_quick_ux")] IReadOnlyList<string> KnobsQuickUx,
    [property: JsonPropertyName("window_modes_supported")] IReadOnlyList<WindowModesByToolEnvelope> WindowModesSupported,
    [property: JsonPropertyName("tools")] IReadOnlyList<ToolCapabilityEnvelope> Tools,
    [property: JsonPropertyName("metrics")] MetricsCapabilitiesEnvelope Metrics,
    [property: JsonPropertyName("generation")] GenerationCapabilitiesEnvelope Generation);

public sealed record GenerateCandidatePlanItemRequest(
    [property: JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("strategy_version")] string? StrategyVersion = null,
    [property: JsonPropertyName("search_method")] string? SearchMethod = null,
    [property: JsonPropertyName("tie_break_rule")] string? TieBreakRule = null,
    [property: JsonPropertyName("criteria")] IReadOnlyList<GenerateCandidateCriterionRequest>? Criteria = null,
    [property: JsonPropertyName("weights")] IReadOnlyList<GenerateCandidateWeightRequest>? Weights = null,
    [property: JsonPropertyName("filters")] IReadOnlyList<GenerateCandidateFilterRequest>? Filters = null);

public sealed record GenerateRangeSpecRequest(
    [property: JsonPropertyName("min")] double Min,
    [property: JsonPropertyName("max")] double Max,
    [property: JsonPropertyName("inclusive")] bool? Inclusive = null);

public sealed record GenerateAllowedValuesSpecRequest(
    [property: JsonPropertyName("values")] IReadOnlyList<double>? Values = null);

public sealed record GenerateTypicalRangeParamsRequest(
    [property: JsonPropertyName("p_low")] double? PLow = null,
    [property: JsonPropertyName("p_high")] double? PHigh = null);

public sealed record GenerateTypicalRangeSpecRequest(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("coverage")] double Coverage,
    [property: JsonPropertyName("params")] GenerateTypicalRangeParamsRequest? Params = null,
    [property: JsonPropertyName("window_ref")] string? WindowRef = null,
    [property: JsonPropertyName("inclusive")] bool? Inclusive = null);

public sealed record GenerateCandidateCriterionRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] double? Value = null,
    [property: JsonPropertyName("range")] GenerateRangeSpecRequest? Range = null,
    [property: JsonPropertyName("allowed_values")] GenerateAllowedValuesSpecRequest? AllowedValues = null,
    [property: JsonPropertyName("typical_range")] GenerateTypicalRangeSpecRequest? TypicalRange = null,
    [property: JsonPropertyName("mode")] string? Mode = null);

public sealed record GenerateCandidateWeightRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("weight")] double Weight);

public sealed record GenerateCandidateFilterRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] double? Value = null,
    [property: JsonPropertyName("min")] double? Min = null,
    [property: JsonPropertyName("max")] double? Max = null,
    [property: JsonPropertyName("range")] GenerateRangeSpecRequest? Range = null,
    [property: JsonPropertyName("allowed_values")] GenerateAllowedValuesSpecRequest? AllowedValues = null,
    [property: JsonPropertyName("typical_range")] GenerateTypicalRangeSpecRequest? TypicalRange = null,
    [property: JsonPropertyName("mode")] string? Mode = null,
    [property: JsonPropertyName("version")] string? Version = null);

public sealed record GenerateGlobalConstraintsRequest(
    [property: JsonPropertyName("unique_games")] bool? UniqueGames = null,
    [property: JsonPropertyName("sorted_numbers")] bool? SortedNumbers = null);

public sealed record GenerateRepeatRangeRequest(
    [property: JsonPropertyName("min")] int? Min = null,
    [property: JsonPropertyName("max")] int? Max = null);

public sealed record GenerateStructuralExclusionsRequest(
    [property: JsonPropertyName("max_consecutive_run")] double? MaxConsecutiveRun = null,
    [property: JsonPropertyName("max_neighbor_count")] double? MaxNeighborCount = null,
    [property: JsonPropertyName("min_row_entropy_norm")] double? MinRowEntropyNorm = null,
    [property: JsonPropertyName("min_column_entropy_norm")] double? MinColumnEntropyNorm = null,
    [property: JsonPropertyName("max_hhi_linha")] double? MaxHhiLinha = null,
    [property: JsonPropertyName("max_hhi_coluna")] double? MaxHhiColuna = null,
    [property: JsonPropertyName("repeat_range")] GenerateRepeatRangeRequest? RepeatRange = null,
    [property: JsonPropertyName("min_slot_alignment")] double? MinSlotAlignment = null,
    [property: JsonPropertyName("max_outlier_score")] double? MaxOutlierScore = null);

public sealed record GenerateGenerationBudgetRequest(
    [property: JsonPropertyName("max_attempts")] int? MaxAttempts = null,
    [property: JsonPropertyName("pool_multiplier")] double? PoolMultiplier = null);

public sealed record GenerateCandidateGamesRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("seed")] ulong? Seed = null,
    [property: JsonPropertyName("plan")] IReadOnlyList<GenerateCandidatePlanItemRequest>? Plan = null,
    [property: JsonPropertyName("global_constraints")] GenerateGlobalConstraintsRequest? GlobalConstraints = null,
    [property: JsonPropertyName("structural_exclusions")] GenerateStructuralExclusionsRequest? StructuralExclusions = null,
    [property: JsonPropertyName("generation_budget")] GenerateGenerationBudgetRequest? GenerationBudget = null,
    [property: JsonPropertyName("generation_mode")] string? GenerationMode = null,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record AppliedConfigurationEnvelope(
    [property: JsonPropertyName("criteria")] IReadOnlyList<GenerateCandidateCriterionRequest> Criteria,
    [property: JsonPropertyName("weights")] IReadOnlyList<GenerateCandidateWeightRequest> Weights,
    [property: JsonPropertyName("filters")] IReadOnlyList<GenerateCandidateFilterRequest> Filters,
    [property: JsonPropertyName("resolved_defaults")] IReadOnlyDictionary<string, object?> ResolvedDefaults);

public sealed record CandidateGameEnvelope(
    [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers,
    [property: JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyName("strategy_version")] string StrategyVersion,
    [property: JsonPropertyName("search_method")] string SearchMethod,
    [property: JsonPropertyName("tie_break_rule")] string TieBreakRule,
    [property: JsonPropertyName("seed_used"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ulong? SeedUsed,
    [property: JsonPropertyName("applied_configuration")] AppliedConfigurationEnvelope AppliedConfiguration);

public sealed record GenerateCandidateGamesResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("replay_guaranteed")] bool ReplayGuaranteed,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("candidate_games")] IReadOnlyList<CandidateGameEnvelope> CandidateGames);

public sealed record ExplainCandidateGamesRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null,
    [property: JsonPropertyName("games")] IReadOnlyList<IReadOnlyList<int>>? Games = null,
    [property: JsonPropertyName("include_metric_breakdown")] bool IncludeMetricBreakdown = true,
    [property: JsonPropertyName("include_exclusion_breakdown")] bool IncludeExclusionBreakdown = true,
    [property: JsonPropertyName("generation_mode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? GenerationMode = null,
    [property: JsonPropertyName("seed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    ulong? Seed = null,
    [property: JsonPropertyName("replay_guaranteed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? ReplayGuaranteed = null,
    [property: JsonPropertyName("page")] int? Page = null,
    [property: JsonPropertyName("page_size")] int? PageSize = null,
    [property: JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Fields = null,
    [property: JsonPropertyName("include_explanations")] bool IncludeExplanations = true,
    [property: JsonPropertyName("verbosity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Verbosity = null);

public sealed record MetricBreakdownEntryEnvelope(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("metric_version")] string MetricVersion,
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("contribution")] double Contribution,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record ExclusionBreakdownEntryEnvelope(
    [property: JsonPropertyName("exclusion_name")] string ExclusionName,
    [property: JsonPropertyName("exclusion_version")] string ExclusionVersion,
    [property: JsonPropertyName("passed")] bool Passed,
    [property: JsonPropertyName("observed_value")] double ObservedValue,
    [property: JsonPropertyName("threshold")] double Threshold,
    [property: JsonPropertyName("explanation")] string Explanation);

public sealed record ConstraintRangeEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("min")] double Min,
    [property: JsonPropertyOrder(2), JsonPropertyName("max")] double Max,
    [property: JsonPropertyOrder(3), JsonPropertyName("inclusive")] bool Inclusive);

public sealed record ConstraintAllowedValuesEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("values")] IReadOnlyList<double> Values);

public sealed record ConstraintTypicalRangeEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyOrder(2), JsonPropertyName("method")] string Method,
    [property: JsonPropertyOrder(3), JsonPropertyName("coverage")] double Coverage,
    [property: JsonPropertyOrder(4), JsonPropertyName("resolved_range")] ConstraintRangeEnvelope ResolvedRange,
    [property: JsonPropertyOrder(5), JsonPropertyName("coverage_observed")] double CoverageObserved,
    [property: JsonPropertyOrder(6), JsonPropertyName("method_version")] string MethodVersion);

public sealed record ConstraintSpecEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("value"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? Value,
    [property: JsonPropertyOrder(2), JsonPropertyName("range"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ConstraintRangeEnvelope? Range,
    [property: JsonPropertyOrder(3), JsonPropertyName("allowed_values"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ConstraintAllowedValuesEnvelope? AllowedValues,
    [property: JsonPropertyOrder(4), JsonPropertyName("typical_range"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ConstraintTypicalRangeEnvelope? TypicalRange);

public sealed record ConstraintResultEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("passed")] bool Passed,
    [property: JsonPropertyOrder(2), JsonPropertyName("penalty")] double Penalty);

public sealed record ConstraintBreakdownEntryEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyOrder(2), JsonPropertyName("name")] string Name,
    [property: JsonPropertyOrder(3), JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyOrder(4), JsonPropertyName("observed_value")] double ObservedValue,
    [property: JsonPropertyOrder(5), JsonPropertyName("applied")] ConstraintSpecEnvelope Applied,
    [property: JsonPropertyOrder(6), JsonPropertyName("result")] ConstraintResultEnvelope Result,
    [property: JsonPropertyOrder(7), JsonPropertyName("explanation")] string Explanation);

public sealed record CandidateStrategyExplanationEnvelope(
    [property: JsonPropertyOrder(1), JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyOrder(2), JsonPropertyName("strategy_version")] string StrategyVersion,
    [property: JsonPropertyOrder(3), JsonPropertyName("search_method")] string SearchMethod,
    [property: JsonPropertyOrder(4), JsonPropertyName("tie_break_rule")] string TieBreakRule,
    [property: JsonPropertyOrder(5), JsonPropertyName("score")] double Score,
    [property: JsonPropertyOrder(6), JsonPropertyName("metric_breakdown")] IReadOnlyList<MetricBreakdownEntryEnvelope> MetricBreakdown,
    [property: JsonPropertyOrder(7), JsonPropertyName("exclusion_breakdown")] IReadOnlyList<ExclusionBreakdownEntryEnvelope> ExclusionBreakdown,
    [property: JsonPropertyOrder(8), JsonPropertyName("constraint_breakdown")] IReadOnlyList<ConstraintBreakdownEntryEnvelope> ConstraintBreakdown);

public sealed record GameExplanationEnvelope(
    [property: JsonPropertyName("game")] IReadOnlyList<int> Game,
    [property: JsonPropertyName("candidate_strategies")] IReadOnlyList<CandidateStrategyExplanationEnvelope> CandidateStrategies);

public sealed record CandidateGenerationAuditEnvelope(
    [property: JsonPropertyName("requested_generation_mode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RequestedGenerationMode,
    [property: JsonPropertyName("effective_generation_mode")] string EffectiveGenerationMode,
    [property: JsonPropertyName("context_supplied")] bool ContextSupplied,
    [property: JsonPropertyName("seed_declared"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? SeedDeclared,
    [property: JsonPropertyName("replay_guaranteed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? ReplayGuaranteed,
    [property: JsonPropertyName("intersection_and_restrictions")] string IntersectionAndRestrictions,
    [property: JsonPropertyName("replay_and_seed_policy")] string ReplayAndSeedPolicy);

public sealed record ExplainCandidateGamesResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("candidate_generation_audit")] CandidateGenerationAuditEnvelope CandidateGenerationAudit,
    [property: JsonPropertyName("explanations")] IReadOnlyList<GameExplanationEnvelope> Explanations);

public sealed class V0Tools
{
    private readonly ComputeWindowMetricsUseCase _computeWindowMetricsUseCase;
    private readonly GetDrawWindowUseCase _getDrawWindowUseCase;
    private readonly AnalyzeIndicatorStabilityUseCase _analyzeIndicatorStabilityUseCase;
    private readonly ComposeIndicatorAnalysisUseCase _composeIndicatorAnalysisUseCase;
    private readonly AnalyzeIndicatorAssociationsUseCase _analyzeIndicatorAssociationsUseCase;
    private readonly SummarizeWindowPatternsUseCase _summarizeWindowPatternsUseCase;
    private readonly SummarizeWindowAggregatesUseCase _summarizeWindowAggregatesUseCase;
    private readonly GenerateCandidateGamesUseCase _generateCandidateGamesUseCase;
    private readonly ExplainCandidateGamesUseCase _explainCandidateGamesUseCase;
    private readonly DeterministicHashService _deterministicHashService;
    private readonly string? _drawsSourceUri;
    private readonly string _contentRootPath;
    private readonly HttpJsonDatasetSnapshotCache? _httpSnapshotCache;
    private const string HelpToolVersion = "1.0.0";
    private const string DiscoverCapabilitiesToolVersion = "1.1.0";
    private const int DefaultPageSizeFull = 200;
    private const int MaxPageSizeFull = 500;

    private readonly record struct PaginationSpec(int Page, int PageSize);
    private static string ResolveVerbosity(string? verbosity)
    {
        var normalized = verbosity?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "minimal" => "minimal",
            "full" => "full",
            _ => "standard"
        };
    }

    public V0Tools(string? drawsSourceUri, string? contentRootPath = null, HttpJsonDatasetSnapshotCache? httpSnapshotCache = null)
    {
        var mapper = new V0RequestMapper(new DrawNormalizer());
        var fixtureProvider = new SyntheticFixtureProvider();
        var datasetVersionService = new DatasetVersionService();
        var validator = new V0CrossFieldValidator();

        var frequencyByDezena = new FrequencyByDezenaMetric();
        var totalDePresencasNaJanelaPorDezena = new TotalDePresencasNaJanelaPorDezenaMetric(frequencyByDezena);
        var windowMetricDispatcher = new WindowMetricDispatcher(
            frequencyByDezena,
            totalDePresencasNaJanelaPorDezena,
            new Top10MaisSorteadosMetric(frequencyByDezena),
            new Top10MenosSorteadosMetric(frequencyByDezena),
            new Top10MaioresTotaisDePresencasNaJanelaMetric(totalDePresencasNaJanelaPorDezena),
            new Top10MenoresTotaisDePresencasNaJanelaMetric(totalDePresencasNaJanelaPorDezena),
            new SequenciaAtualDePresencasPorDezenaMetric(),
            new ParesNoConcursoMetric(),
            new RepeticaoConcursoAnteriorMetric(),
            new QuantidadeVizinhosPorConcursoMetric(),
            new SequenciaMaximaVizinhosPorConcursoMetric(),
            new DistribuicaoLinhaPorConcursoMetric(),
            new DistribuicaoColunaPorConcursoMetric(),
            new EntropiaLinhaPorConcursoMetric(),
            new EntropiaColunaPorConcursoMetric(),
            new HhiLinhaPorConcursoMetric(),
            new HhiColunaPorConcursoMetric(),
            new AtrasoPorDezenaMetric(),
            new AssimetriaBlocosMetric(),
            new MatrizNumeroSlotMetric(),
            new FrequenciaBlocosMetric(),
            new AusenciaBlocosMetric(),
            new EstadoAtualDezenaMetric(),
            new EstabilidadeRankingMetric());
        _computeWindowMetricsUseCase = new ComputeWindowMetricsUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _getDrawWindowUseCase = new GetDrawWindowUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            validator,
            mapper);

        _analyzeIndicatorStabilityUseCase = new AnalyzeIndicatorStabilityUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            validator,
            mapper,
            new IndicatorStabilityAnalyzer());

        _composeIndicatorAnalysisUseCase = new ComposeIndicatorAnalysisUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _analyzeIndicatorAssociationsUseCase = new AnalyzeIndicatorAssociationsUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            validator,
            mapper,
            new IndicatorAssociationAnalyzer());

        _summarizeWindowPatternsUseCase = new SummarizeWindowPatternsUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _summarizeWindowAggregatesUseCase = new SummarizeWindowAggregatesUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _generateCandidateGamesUseCase = new GenerateCandidateGamesUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _explainCandidateGamesUseCase = new ExplainCandidateGamesUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            windowMetricDispatcher,
            validator,
            mapper);

        _deterministicHashService = new DeterministicHashService(
            new CanonicalJsonSerializer(),
            new Sha256Hasher());

        _drawsSourceUri = drawsSourceUri;
        // For distributed (self-contained) builds, hosts may set an arbitrary working directory.
        // Resolve relative dataset paths against the executable base directory by default.
        _contentRootPath = string.IsNullOrWhiteSpace(contentRootPath)
            ? AppContext.BaseDirectory
            : contentRootPath;
        _httpSnapshotCache = httpSnapshotCache;
    }

    public object Help()
    {
        try
        {
            var indexPath = Path.Combine(AppContext.BaseDirectory, "resources", "prompts", "index@1.0.0.md");
            var indexMarkdown = File.ReadAllText(indexPath);

            var templates = LotofacilMcp.Server.Prompting.PromptCatalog.Templates
                .Select(t => new PromptTemplateSummaryEnvelope(
                    t.Id,
                    t.Uri,
                    t.Title,
                    t.Description,
                    t.SuggestedWindows))
                .ToArray();

            string? gettingStartedUri = null;
            try
            {
                var gettingStartedPath = Path.Combine(AppContext.BaseDirectory, "resources", "help", "getting-started@1.0.0.md");
                if (File.Exists(gettingStartedPath))
                {
                    gettingStartedUri = LotofacilMcp.Server.Helping.HelpCatalog.GettingStartedUri;
                }
            }
            catch
            {
                gettingStartedUri = null;
            }

            var quickStartMarkdown =
                "## Comece por aqui\n\n" +
                "1) Chame `help`\n" +
                "2) Escolha um caminho (comece pelo **Painel geral**)\n" +
                "3) Escolha o período (se não souber o último concurso, use `get_draw_window(window_size=1)` para ancorar no mais recente)\n\n" +
                "### Quickstart operacional (sem tentativa/erro)\n\n" +
                "- **Último concurso**: peça `get_draw_window(window_size=1)`.\n" +
                "- **Primeiras métricas**: comece com `compute_window_metrics(window_size=5, metrics=[{\"name\":\"frequencia_por_dezena\"}])` e ajuste a janela conforme o objetivo.\n" +
                "- **Campos mínimos de rastreabilidade** (preserve sempre): `dataset_version`, `tool_version`, `deterministic_hash`, `window`.\n\n" +
                "### Modo econômico vs detalhado (sem tentativa/erro)\n\n" +
                "Quando você disser algo como **\"modo econômico\"**, o cliente/agente deve mapear isso para knobs de economia:\n\n" +
                "- `verbosity`: `minimal` | `standard` | `full`\n" +
                "- `include_explanations`: `false` para omitir explicações, `true` para incluir\n" +
                "- `fields`: lista de campos (projeção) para trazer só o necessário no JSON\n\n" +
                "**Recomendação para chat:** use `verbosity=\"standard\"` como padrão (chat-safe).\n\n" +
                "- Em `standard`, o servidor mantém no canal **Content** o **resultado principal** (um resumo humano útil), sem despejar JSON.\n" +
                "- Em `minimal`, o Content fica econômico (ainda útil), e o JSON completo continua no `StructuredContent`.\n" +
                "- Em `full`, o Content pode ser mais detalhado, mas respostas realmente extensas ainda dependem de **projeção** (`fields`) e/ou **paginação** (quando suportada).\n\n" +
                "**Exemplos prontos para pedir:**\n\n" +
                "- **Econômico / só o essencial**: `verbosity=\"minimal\"`, `include_explanations=false`, e (se a tool suportar) `fields=[\"dataset_version\",\"tool_version\",\"deterministic_hash\",\"window\", ...]`.\n" +
                "- **Humano interativo (recomendado)**: `verbosity=\"standard\"`, `include_explanations=true`.\n" +
                "- **Detalhado**: `verbosity=\"full\"`, `include_explanations=true` e, se o payload ficar grande, use `fields` e/ou paginação (`page`/`page_size`, quando suportado).\n\n" +
                "**Quando pedir projeção/paginação (em vez de esperar um dump no Content):**\n\n" +
                "- Se você quer “só o topo” ou “só o ranking”, peça `fields` reduzindo o JSON.\n" +
                "- Se você quer “todos os itens”, use `verbosity=\"full\"` e paginação determinística quando disponível.\n\n" +
                "Dica: você pode abrir o onboarding em `lotofacil-ia://help/getting-started@1.0.0`.\n";

            return new HelpResponse(
                ToolVersion: HelpToolVersion,
                GettingStartedResourceUri: gettingStartedUri,
                QuickStartMarkdown: quickStartMarkdown,
                IndexResourceUri: LotofacilMcp.Server.Prompting.PromptCatalog.IndexUri,
                IndexMarkdown: indexMarkdown,
                Templates: templates);
        }
        catch (Exception ex)
        {
            return ToContractError(
                "HELP_UNAVAILABLE",
                "Unable to load help index resource.",
                new Dictionary<string, object?>
                {
                    ["reason"] = ex.Message
                });
        }
    }

    public object DiscoverCapabilities(DiscoverCapabilitiesRequest request)
    {
        var effectiveVerbosity = ResolveVerbosity(request.Verbosity);
        var implementedMetricNames = MetricAvailabilityCatalog.GetImplementedMetricNames()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var pendingMetricNames = MetricAvailabilityCatalog.GetPendingMetricNames()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var computeWindowAllowed = MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics(allowPending: false)
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var summarizeAllowedSources = MetricAvailabilityCatalog.GetSummarizeWindowAggregatesAllowedSources()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var associationAllowedIndicators = MetricAvailabilityCatalog.GetAnalyzeIndicatorAssociationsAllowedIndicators()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var composeAllowedComponents = MetricAvailabilityCatalog.GetComposeIndicatorAnalysisAllowedComponents()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();
        var knownMetricNames = MetricAvailabilityCatalog.GetKnownMetricNames()
            .OrderBy(static metric => metric, StringComparer.Ordinal)
            .ToArray();

        var contentChannelRules = new[]
        {
            "Hotfix 23.5.2: O resultado principal da tool não deve ser omitido do Content (mesmo quando StructuredContent existir).",
            "ADR 0023 (D2): Content é um resumo humano útil e não deve duplicar o JSON canônico do StructuredContent.",
            "ADR 0023 (D4): Respostas extensas devem usar projeção (fields) e/ou paginação determinística (quando suportada), em vez de dump completo no Content."
        };

        var knobsQuickUx = effectiveVerbosity switch
        {
            "minimal" => new[]
            {
                "Hotfix 23.5.1: Default recomendado para chat é verbosity=standard (chat-safe).",
                "Modo econômico: verbosity=minimal + include_explanations=false + (quando suportado) fields=[dataset_version, tool_version, deterministic_hash, window, ...]."
            },
            "full" => new[]
            {
                "Hotfix 23.5.1: Default recomendado para chat é verbosity=standard (chat-safe).",
                "Humano interativo: verbosity=standard + include_explanations=true.",
                "Detalhado: verbosity=full + include_explanations=true; para payload grande use fields e/ou paginação (page/page_size quando suportado)."
            },
            _ => new[]
            {
                "Hotfix 23.5.1: Default recomendado para chat é verbosity=standard (chat-safe).",
                "Econômico: verbosity=minimal + include_explanations=false (+ fields quando suportado).",
                "Detalhado: verbosity=full (+ fields/paginação quando suportado)."
            }
        };

        static IReadOnlyDictionary<string, IReadOnlyList<string>> WithWindowOperationalConstraints(
            Dictionary<string, IReadOnlyList<string>> supportedParameters)
        {
            supportedParameters["window_size.constraint"] = ["window_size > 0"];
            supportedParameters["window_size.quickstart"] = ["window_size=1 anchors the latest available contest when end_contest_id is omitted"];
            supportedParameters["start_contest_id.constraint"] = ["start_contest_id requires end_contest_id"];
            supportedParameters["start_end.constraint"] = ["start_contest_id must be <= end_contest_id"];
            supportedParameters["window_size_start_end.coherence"] = ["if start_contest_id/end_contest_id are provided, window_size must be omitted/0 or equal to (end-start+1)"];
            return supportedParameters;
        }

        var response = new DiscoverCapabilitiesResponse(
            ToolVersion: DiscoverCapabilitiesToolVersion,
            DeterministicHash: string.Empty,
            BuildProfile: "v0",
            DatasetRequirements:
            [
                "requires Dataset__DrawsSourceUri configured (path or file://) to load draws dataset"
            ],
            ContentChannelRules: contentChannelRules,
            KnobsQuickUx: knobsQuickUx,
            WindowModesSupported:
            [
                new WindowModesByToolEnvelope("get_draw_window", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("compute_window_metrics", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("analyze_indicator_stability", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("compose_indicator_analysis", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("analyze_indicator_associations", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("summarize_window_patterns", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("summarize_window_aggregates", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("generate_candidate_games", ["window_size+end_contest_id", "start_contest_id+end_contest_id"]),
                new WindowModesByToolEnvelope("explain_candidate_games", ["window_size+end_contest_id", "start_contest_id+end_contest_id"])
            ],
            Tools:
            [
                new ToolCapabilityEnvelope(
                    Name: "discover_capabilities",
                    ToolVersion: DiscoverCapabilitiesToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"]
                    },
                    Capabilities: "Returns deterministic build-surface metadata without executing metrics."),
                new ToolCapabilityEnvelope(
                    Name: "get_draw_window",
                    ToolVersion: GetDrawWindowUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["window_modes"] = ["window_size+end_contest_id", "start_contest_id+end_contest_id"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "draws"]
                    }),
                    Capabilities: "Returns a canonical draw window (ordered, deterministic) for the resolved window."),
                new ToolCapabilityEnvelope(
                    Name: "compute_window_metrics",
                    ToolVersion: ComputeWindowMetricsUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["window_modes"] = ["window_size+end_contest_id", "start_contest_id+end_contest_id"],
                        ["allow_pending"] = ["false", "true"],
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "metrics"]
                    }),
                    Capabilities: "Computes cataloged metrics allowed in this build for a resolved window."),
                new ToolCapabilityEnvelope(
                    Name: "analyze_indicator_stability",
                    ToolVersion: AnalyzeIndicatorStabilityUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["normalization_method"] = ["madn", "coefficient_of_variation"],
                        ["aggregation"] = ["identity", "mean", "max", "l2_norm", "per_component"],
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "normalization_method", "ranking"]
                    }),
                    Capabilities: "Ranks indicator stability using scalarized series and supported normalization methods."),
                new ToolCapabilityEnvelope(
                    Name: "compose_indicator_analysis",
                    ToolVersion: ComposeIndicatorAnalysisUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["target"] = ["dezena"],
                        ["operator"] = ["weighted_rank"],
                        ["transform"] =
                        [
                            "normalize_max",
                            "invert_normalize_max",
                            "rank_percentile",
                            "identity_unit_interval",
                            "one_minus_unit_interval",
                            "shift_scale_unit_interval"
                        ],
                        ["metric_name"] = composeAllowedComponents,
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "target", "operator", "ranking"]
                    }),
                    Capabilities: "Builds deterministic weighted compositions over supported dezena indicators."),
                new ToolCapabilityEnvelope(
                    Name: "analyze_indicator_associations",
                    ToolVersion: AnalyzeIndicatorAssociationsUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["method"] = ["spearman"],
                        ["aggregation"] = ["identity", "mean", "max", "l2_norm", "per_component"],
                        ["indicator_name"] = associationAllowedIndicators,
                        ["stability_check.method"] = ["rolling_window"],
                        ["stability_check.required_fields"] = ["subwindow_size", "stride", "min_subwindows"],
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "method", "association_magnitude", "association_stability"]
                    }),
                    Capabilities: "Computes association magnitude and optional deterministic subwindow stability for compatible scalarized series."),
                new ToolCapabilityEnvelope(
                    Name: "summarize_window_patterns",
                    ToolVersion: SummarizeWindowPatternsUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["range_method"] = ["iqr"],
                        ["feature_metric_name"] = ["pares_no_concurso"],
                        ["aggregation"] = ["identity"],
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "range_method", "coverage_threshold", "summaries"]
                    }),
                    Capabilities: "Summarizes window pattern distributions with deterministic IQR statistics."),
                new ToolCapabilityEnvelope(
                    Name: "summarize_window_aggregates",
                    ToolVersion: SummarizeWindowAggregatesUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["aggregate_type"] =
                        [
                            "histogram_scalar_series",
                            "topk_patterns_count_vector5_series",
                            "histogram_count_vector5_series_per_position_matrix"
                        ],
                        ["source_metric_name"] = summarizeAllowedSources,
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "aggregates"]
                    }),
                    Capabilities: "Builds canonical aggregate payloads over implemented source metrics."),
                new ToolCapabilityEnvelope(
                    Name: "generate_candidate_games",
                    ToolVersion: GenerateCandidateGamesUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["strategy_name"] = ["common_repetition_frequency", "declared_composite_profile"],
                        ["search_method"] = ["exhaustive", "sampled", "greedy_topk"],
                        ["generation_mode"] = [GenerationModes.RandomUnrestricted, GenerationModes.BehaviorFiltered],
                        ["plan.criteria"] = ["name", "value|range|allowed_values|typical_range", "mode"],
                        ["plan.weights"] = ["name", "weight"],
                        ["plan.filters"] = ["name", "value|min|max|range|allowed_values|typical_range", "mode", "version"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "replay_guaranteed", "window", "candidate_games"]
                    }),
                    Capabilities: "Generates candidate games; max sum of plan count per request, generation modes, and seed policy follow generation envelope."),
                new ToolCapabilityEnvelope(
                    Name: "explain_candidate_games",
                    ToolVersion: ExplainCandidateGamesUseCase.ToolVersion,
                    SupportedParameters: WithWindowOperationalConstraints(new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"],
                        ["flags"] = ["include_metric_breakdown", "include_exclusion_breakdown"],
                        ["generation_mode"] = [GenerationModes.RandomUnrestricted, GenerationModes.BehaviorFiltered],
                        ["context_echo"] = ["seed", "replay_guaranteed"],
                        ["include_explanations"] = ["false", "true"],
                        ["include_explanations.default_recommended"] = ["true (standard/full)", "false (minimal)"],
                        ["page"] = ["1.."],
                        ["page_size"] = [$"1..{MaxPageSizeFull}"],
                        ["page_page_size.constraint"] = ["pagination requires verbosity=full"],
                        ["fields"] = ["dataset_version", "tool_version", "deterministic_hash", "window", "candidate_generation_audit", "explanations"]
                    }),
                    Capabilities: "Explains candidate strategies, exclusion/constraint breakdowns, and auditable echo of generation mode, effective restriction composition (intersection when applicable), and seed/replay policy."),
                new ToolCapabilityEnvelope(
                    Name: "help",
                    ToolVersion: HelpToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["verbosity"] = ["minimal", "standard", "full"],
                        ["verbosity.default_recommended"] = ["standard"]
                    },
                    Capabilities: "Provides onboarding resources and prompt catalog references.")
            ],
            Metrics: new MetricsCapabilitiesEnvelope(
                ImplementedMetricNames: implementedMetricNames,
                PendingMetricNames: pendingMetricNames,
                ComputeWindowMetricsAllowed: computeWindowAllowed,
                SummarizeWindowAggregatesAllowedSources: summarizeAllowedSources,
                AssociationAllowedIndicators: associationAllowedIndicators),
            Generation: new GenerationCapabilitiesEnvelope(
                Strategies:
                [
                    new GenerationStrategyEnvelope("common_repetition_frequency", "1.0.0"),
                    new GenerationStrategyEnvelope("declared_composite_profile", "1.0.0")
                ],
                SearchMethods: ["exhaustive", "sampled", "greedy_topk"],
                SupportedFilters:
                [
                    "max_consecutive_run",
                    "max_neighbor_count",
                    "min_row_entropy_norm",
                    "max_hhi_linha",
                    "repeat_range",
                    "min_slot_alignment",
                    "max_outlier_score"
                ],
                MaxSumPlanCountPerRequest: GenerationRequestLimits.MaxSumPlanCountPerRequest,
                SupportedGenerationModes:
                [
                    GenerationModes.RandomUnrestricted,
                    GenerationModes.BehaviorFiltered
                ],
                RequestSeedOptional: true,
                SeedBySearchMethod:
                [
                    new SearchMethodSeedPolicyEnvelope("exhaustive", SeedRequiredForReplayGuaranteed: false),
                    new SearchMethodSeedPolicyEnvelope("greedy_topk", SeedRequiredForReplayGuaranteed: true),
                    new SearchMethodSeedPolicyEnvelope("sampled", SeedRequiredForReplayGuaranteed: true)
                ]));

        var deterministicHash = _deterministicHashService.Compute(
            new
            {
                verbosity = effectiveVerbosity,
                build_profile = response.BuildProfile,
                dataset_requirements = response.DatasetRequirements,
                content_channel_rules = response.ContentChannelRules,
                knobs_quick_ux = response.KnobsQuickUx,
                window_modes_supported = response.WindowModesSupported,
                tools = response.Tools.Select(tool => new
                {
                    name = tool.Name,
                    tool_version = tool.ToolVersion,
                    supported_parameters = tool.SupportedParameters,
                    capabilities = tool.Capabilities
                }).ToArray(),
                metrics = response.Metrics,
                generation = response.Generation,
                known_metric_names = knownMetricNames
            },
            datasetVersion: "build_capabilities_v0",
            toolVersion: DiscoverCapabilitiesToolVersion);

        return response with
        {
            DeterministicHash = deterministicHash
        };
    }

    public object ComputeWindowMetrics(ComputeWindowMetricsRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "metrics",
                "metrics.metric_name",
                "metrics.scope",
                "metrics.shape",
                "metrics.unit",
                "metrics.version",
                "metrics.window",
                "metrics.window.size",
                "metrics.window.start_contest_id",
                "metrics.window.end_contest_id",
                "metrics.value",
                "metrics.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id"
            };

            var fieldsError = ResponseTransforms.ValidateFields("compute_window_metrics", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _computeWindowMetricsUseCase.Execute(new ComputeWindowMetricsInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Metrics: request.Metrics?.Select(metric => new MetricRequestInput(metric.Name)).ToArray(),
                AllowPending: request.AllowPending,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["include_explanations"] = request.IncludeExplanations,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var metrics = result.Metrics.ToArray();
            if (hasPagination && pagination is not null)
            {
                metrics = ApplyPagination(metrics, pagination.Value);
            }

            var response = new ComputeWindowMetricsResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Metrics: metrics
                    .Select(metric => new MetricValueEnvelope(
                        metric.MetricName,
                        metric.Scope,
                        metric.Shape,
                        metric.Unit,
                        metric.Version,
                        new WindowEnvelope(
                            metric.Window.Size,
                            metric.Window.StartContestId,
                            metric.Window.EndContestId),
                        metric.Value.ToArray(),
                        metric.Explanation))
                    .ToArray());

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    public object GetDrawWindow(GetDrawWindowRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "draws"
                ,
                "draws.contest_id",
                "draws.draw_date",
                "draws.numbers"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id"
            };

            var fieldsError = ResponseTransforms.ValidateFields("get_draw_window", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _getDrawWindowUseCase.Execute(new GetDrawWindowInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var draws = result.Draws.ToArray();
            if (hasPagination && pagination is not null)
            {
                draws = ApplyPagination(draws, pagination.Value);
            }

            var response = new GetDrawWindowResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Draws: draws
                    .Select(draw => new DrawDto(draw.ContestId, draw.DrawDate, draw.Numbers.ToArray()))
                    .ToArray());

            if (requestedFields is null)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
            foreach (var f in requestedFields)
            {
                keep.Add(f);
            }
            element = ResponseTransforms.Project(element, keep);
            return element;
        });
    }

    public object AnalyzeIndicatorStability(AnalyzeIndicatorStabilityRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "normalization_method",
                "ranking",
                "ranking.indicator_name",
                "ranking.aggregation",
                "ranking.component_index",
                "ranking.shape",
                "ranking.dispersion",
                "ranking.stability_score",
                "ranking.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "normalization_method"
            };

            var fieldsError = ResponseTransforms.ValidateFields("analyze_indicator_stability", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _analyzeIndicatorStabilityUseCase.Execute(new AnalyzeIndicatorStabilityInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Indicators: request.Indicators?
                    .Select(indicator => new StabilityIndicatorRequestInput(indicator.Name, indicator.Aggregation))
                    .ToArray(),
                NormalizationMethod: request.NormalizationMethod,
                TopK: request.TopK,
                MinHistory: request.MinHistory,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["include_explanations"] = request.IncludeExplanations,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var ranking = result.Ranking.ToArray();
            if (hasPagination && pagination is not null)
            {
                ranking = ApplyPagination(ranking, pagination.Value);
            }

            var response = new AnalyzeIndicatorStabilityResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                NormalizationMethod: result.NormalizationMethod,
                Ranking: ranking
                    .Select(entry => new StabilityRankingEntryEnvelope(
                        entry.IndicatorName,
                        entry.Aggregation,
                        entry.ComponentIndex,
                        entry.Shape,
                        entry.Dispersion,
                        entry.StabilityScore,
                        entry.Explanation))
                    .ToArray());

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    public object ComposeIndicatorAnalysis(ComposeIndicatorAnalysisRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "target",
                "operator",
                "ranking",
                "ranking.dezena",
                "ranking.rank",
                "ranking.score",
                "ranking.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "target",
                "operator"
            };

            var fieldsError = ResponseTransforms.ValidateFields("compose_indicator_analysis", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _composeIndicatorAnalysisUseCase.Execute(new ComposeIndicatorAnalysisInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Target: request.Target,
                Operator: request.Operator,
                Components: (request.Components ?? Array.Empty<ComposeIndicatorComponentRequest>())
                    .Select(component => new CompositionComponentInput(
                        component.MetricName,
                        component.Transform,
                        component.Weight))
                    .ToArray(),
                TopK: request.TopK,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["include_explanations"] = request.IncludeExplanations,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var ranking = result.Ranking.ToArray();
            if (hasPagination && pagination is not null)
            {
                ranking = ApplyPagination(ranking, pagination.Value);
            }

            var response = new ComposeIndicatorAnalysisResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Target: result.Target,
                Operator: result.Operator,
                Ranking: ranking
                    .Select(entry => new WeightedDezenaRankingEntryEnvelope(
                        entry.Dezena,
                        entry.Rank,
                        entry.Score,
                        entry.Explanation))
                    .ToArray());

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    public object AnalyzeIndicatorAssociations(AnalyzeIndicatorAssociationsRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "method",
                "association_magnitude",
                "association_magnitude.method",
                "association_magnitude.top_pairs",
                "association_magnitude.top_pairs.indicator_a",
                "association_magnitude.top_pairs.aggregation_a",
                "association_magnitude.top_pairs.component_index_a",
                "association_magnitude.top_pairs.indicator_b",
                "association_magnitude.top_pairs.aggregation_b",
                "association_magnitude.top_pairs.component_index_b",
                "association_magnitude.top_pairs.association_strength",
                "association_magnitude.top_pairs.explanation",
                "association_stability",
                "association_stability.method",
                "association_stability.subwindow_size",
                "association_stability.stride",
                "association_stability.min_subwindows",
                "association_stability.subwindows_count",
                "association_stability.top_pairs",
                "association_stability.top_pairs.indicator_a",
                "association_stability.top_pairs.aggregation_a",
                "association_stability.top_pairs.component_index_a",
                "association_stability.top_pairs.indicator_b",
                "association_stability.top_pairs.aggregation_b",
                "association_stability.top_pairs.component_index_b",
                "association_stability.top_pairs.association_strength",
                "association_stability.top_pairs.stability_score",
                "association_stability.top_pairs.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "method",
                "association_magnitude"
            };

            var fieldsError = ResponseTransforms.ValidateFields("analyze_indicator_associations", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _analyzeIndicatorAssociationsUseCase.Execute(new AnalyzeIndicatorAssociationsInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Items: (request.Items ?? Array.Empty<AssociationItemRequest>())
                    .Select(item => new StabilityIndicatorRequestInput(item.Name, item.Aggregation))
                    .ToArray(),
                Method: request.Method,
                TopK: request.TopK,
                StabilityCheck: request.StabilityCheck is null
                    ? null
                    : new AssociationStabilityCheckInput(
                        request.StabilityCheck.Method,
                        request.StabilityCheck.SubwindowSize,
                        request.StabilityCheck.Stride,
                        request.StabilityCheck.MinSubwindows),
                FixturePath: fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                new
                {
                    core = result.DeterministicHashInput,
                    verbosity = ResolveVerbosity(request.Verbosity),
                    include_explanations = request.IncludeExplanations,
                    fields = requestedFields
                },
                result.DatasetVersion,
                result.ToolVersion);

            var response = new AnalyzeIndicatorAssociationsResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Method: result.Method,
                AssociationMagnitude: new AssociationMagnitudeEnvelope(
                    result.AssociationMagnitude.Method,
                    result.AssociationMagnitude.TopPairs
                        .Select(entry => new AssociationMagnitudeEntryEnvelope(
                            entry.IndicatorA,
                            entry.AggregationA,
                            entry.ComponentIndexA,
                            entry.IndicatorB,
                            entry.AggregationB,
                            entry.ComponentIndexB,
                            entry.AssociationStrength,
                            entry.Explanation))
                        .ToArray()),
                AssociationStability: result.AssociationStability is null
                    ? null
                    : new AssociationStabilityEnvelope(
                        result.AssociationStability.Method,
                        result.AssociationStability.SubwindowSize,
                        result.AssociationStability.Stride,
                        result.AssociationStability.MinSubwindows,
                        result.AssociationStability.SubwindowsCount,
                        result.AssociationStability.TopPairs
                            .Select(entry => new AssociationStabilityEntryEnvelope(
                                entry.IndicatorA,
                                entry.AggregationA,
                                entry.ComponentIndexA,
                                entry.IndicatorB,
                                entry.AggregationB,
                                entry.ComponentIndexB,
                                entry.Mean,
                                entry.Median,
                                entry.P10,
                                entry.P90,
                                entry.Min,
                                entry.Max,
                                entry.StdDev,
                                entry.SignConsistencyRatio))
                            .ToArray()));

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    public object SummarizeWindowPatterns(SummarizeWindowPatternsRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "range_method",
                "coverage_threshold",
                "summaries",
                "summaries.metric_name",
                "summaries.aggregation",
                "summaries.mode",
                "summaries.q1",
                "summaries.median",
                "summaries.q3",
                "summaries.iqr",
                "summaries.coverage_observed",
                "summaries.coverage_count",
                "summaries.total_count",
                "summaries.outlier_count",
                "summaries.outlier_lower_fence",
                "summaries.outlier_upper_fence",
                "summaries.coverage_threshold_met",
                "summaries.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "range_method",
                "coverage_threshold"
            };

            var fieldsError = ResponseTransforms.ValidateFields("summarize_window_patterns", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _summarizeWindowPatternsUseCase.Execute(new SummarizeWindowPatternsInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Features: (request.Features ?? Array.Empty<WindowPatternFeatureRequest>())
                    .Select(feature => new WindowPatternFeatureInput(feature.MetricName, feature.Aggregation))
                    .ToArray(),
                CoverageThreshold: request.CoverageThreshold,
                RangeMethod: request.RangeMethod,
                FixturePath: fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                new
                {
                    core = result.DeterministicHashInput,
                    verbosity = ResolveVerbosity(request.Verbosity),
                    include_explanations = request.IncludeExplanations,
                    fields = requestedFields
                },
                result.DatasetVersion,
                result.ToolVersion);

            var response = new SummarizeWindowPatternsResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                RangeMethod: result.RangeMethod,
                CoverageThreshold: result.CoverageThreshold,
                Summaries: result.Summaries
                    .Select(summary => new WindowPatternSummaryEnvelope(
                        summary.MetricName,
                        summary.Aggregation,
                        summary.Mode,
                        summary.Q1,
                        summary.Median,
                        summary.Q3,
                        summary.Iqr,
                        summary.CoverageObserved,
                        summary.CoverageCount,
                        summary.TotalCount,
                        summary.OutlierCount,
                        summary.OutlierLowerFence,
                        summary.OutlierUpperFence,
                        summary.CoverageThresholdMet,
                        summary.Explanation))
                    .ToArray());

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    public object SummarizeWindowAggregates(SummarizeWindowAggregatesRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "aggregates",
                "aggregates.id",
                "aggregates.source_metric_name",
                "aggregates.aggregate_type",
                "aggregates.buckets",
                "aggregates.buckets.x",
                "aggregates.buckets.count",
                "aggregates.buckets.ratio",
                "aggregates.items",
                "aggregates.items.pattern",
                "aggregates.items.count",
                "aggregates.items.ratio",
                "aggregates.matrix"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id"
            };

            var fieldsError = ResponseTransforms.ValidateFields("summarize_window_aggregates", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _summarizeWindowAggregatesUseCase.Execute(new SummarizeWindowAggregatesInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Aggregates: request.Aggregates?
                    .Select(aggregate => new WindowAggregateRequestInput(
                        Id: aggregate.Id ?? string.Empty,
                        SourceMetricName: aggregate.SourceMetricName ?? string.Empty,
                        AggregateType: aggregate.AggregateType ?? string.Empty,
                        Params: aggregate.Params.ValueKind is JsonValueKind.Undefined
                            ? JsonSerializer.SerializeToElement(new Dictionary<string, object?>())
                            : aggregate.Params.Clone()))
                    .ToArray(),
                FixturePath: fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                new
                {
                    core = result.DeterministicHashInput,
                    verbosity = ResolveVerbosity(request.Verbosity),
                    fields = requestedFields
                },
                result.DatasetVersion,
                result.ToolVersion);

            var response = new SummarizeWindowAggregatesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Aggregates: result.Aggregates
                    .Select(aggregate => new WindowAggregateEnvelope(
                        Id: aggregate.Id,
                        SourceMetricName: aggregate.SourceMetricName,
                        AggregateType: aggregate.AggregateType,
                        Buckets: aggregate.Buckets?
                            .Select(bucket => new HistogramBucketEnvelope(
                                bucket.X,
                                bucket.Count,
                                bucket.Ratio))
                            .ToArray(),
                        Items: aggregate.Items?
                            .Select(item => new PatternCountItemEnvelope(
                                item.Pattern.ToArray(),
                                item.Count,
                                item.Ratio))
                            .ToArray(),
                        Matrix: aggregate.Matrix))
                    .ToArray());

            if (requestedFields is null)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
            foreach (var f in requestedFields)
            {
                keep.Add(f);
            }
            element = ResponseTransforms.Project(element, keep);
            return element;
        });
    }

    public object GenerateCandidateGames(GenerateCandidateGamesRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "replay_guaranteed",
                "window",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "candidate_games",
                "candidate_games.numbers",
                "candidate_games.strategy_name",
                "candidate_games.strategy_version",
                "candidate_games.search_method",
                "candidate_games.tie_break_rule",
                "candidate_games.seed_used",
                "candidate_games.applied_configuration",
                "candidate_games.applied_configuration.criteria",
                "candidate_games.applied_configuration.criteria.name",
                "candidate_games.applied_configuration.criteria.value",
                "candidate_games.applied_configuration.criteria.range",
                "candidate_games.applied_configuration.criteria.range.min",
                "candidate_games.applied_configuration.criteria.range.max",
                "candidate_games.applied_configuration.criteria.range.inclusive",
                "candidate_games.applied_configuration.criteria.allowed_values",
                "candidate_games.applied_configuration.criteria.allowed_values.values",
                "candidate_games.applied_configuration.criteria.typical_range",
                "candidate_games.applied_configuration.criteria.typical_range.metric_name",
                "candidate_games.applied_configuration.criteria.typical_range.method",
                "candidate_games.applied_configuration.criteria.typical_range.coverage",
                "candidate_games.applied_configuration.criteria.typical_range.params",
                "candidate_games.applied_configuration.criteria.typical_range.window_ref",
                "candidate_games.applied_configuration.criteria.typical_range.inclusive",
                "candidate_games.applied_configuration.criteria.mode",
                "candidate_games.applied_configuration.weights",
                "candidate_games.applied_configuration.weights.name",
                "candidate_games.applied_configuration.weights.weight",
                "candidate_games.applied_configuration.filters",
                "candidate_games.applied_configuration.filters.name",
                "candidate_games.applied_configuration.filters.value",
                "candidate_games.applied_configuration.filters.min",
                "candidate_games.applied_configuration.filters.max",
                "candidate_games.applied_configuration.filters.range",
                "candidate_games.applied_configuration.filters.range.min",
                "candidate_games.applied_configuration.filters.range.max",
                "candidate_games.applied_configuration.filters.range.inclusive",
                "candidate_games.applied_configuration.filters.allowed_values",
                "candidate_games.applied_configuration.filters.allowed_values.values",
                "candidate_games.applied_configuration.filters.typical_range",
                "candidate_games.applied_configuration.filters.typical_range.metric_name",
                "candidate_games.applied_configuration.filters.typical_range.method",
                "candidate_games.applied_configuration.filters.typical_range.coverage",
                "candidate_games.applied_configuration.filters.typical_range.params",
                "candidate_games.applied_configuration.filters.typical_range.window_ref",
                "candidate_games.applied_configuration.filters.typical_range.inclusive",
                "candidate_games.applied_configuration.filters.mode",
                "candidate_games.applied_configuration.filters.version",
                "candidate_games.applied_configuration.resolved_defaults"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "replay_guaranteed",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id"
            };

            var fieldsError = ResponseTransforms.ValidateFields("generate_candidate_games", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _generateCandidateGamesUseCase.Execute(new GenerateCandidateGamesInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Seed: request.Seed,
                Plan: (request.Plan ?? Array.Empty<GenerateCandidatePlanItemRequest>())
                    .Select(planItem => new GenerateCandidatePlanItemInput(
                        planItem.StrategyName,
                        planItem.Count,
                        planItem.StrategyVersion,
                        planItem.SearchMethod,
                        planItem.TieBreakRule,
                        planItem.Criteria?
                            .Select(criterion => new GenerateCandidateCriteriaInput(
                                criterion.Name,
                                criterion.Value,
                                criterion.Range is null
                                    ? null
                                    : new GenerateRangeSpecInput(
                                        criterion.Range.Min,
                                        criterion.Range.Max,
                                        criterion.Range.Inclusive),
                                criterion.AllowedValues is null
                                    ? null
                                    : new GenerateAllowedValuesSpecInput(criterion.AllowedValues.Values),
                                criterion.TypicalRange is null
                                    ? null
                                    : new GenerateTypicalRangeSpecInput(
                                        criterion.TypicalRange.MetricName,
                                        criterion.TypicalRange.Method,
                                        criterion.TypicalRange.Coverage,
                                        criterion.TypicalRange.Params is null
                                            ? null
                                            : new GenerateTypicalRangeParamsInput(
                                                criterion.TypicalRange.Params.PLow,
                                                criterion.TypicalRange.Params.PHigh),
                                        criterion.TypicalRange.WindowRef,
                                        criterion.TypicalRange.Inclusive),
                                criterion.Mode))
                            .ToArray(),
                        planItem.Weights?
                            .Select(weight => new GenerateCandidateWeightInput(
                                weight.Name,
                                weight.Weight))
                            .ToArray(),
                        planItem.Filters?
                            .Select(filter => new GenerateCandidateFilterInput(
                                filter.Name,
                                filter.Value,
                                filter.Min,
                                filter.Max,
                                filter.Range is null
                                    ? null
                                    : new GenerateRangeSpecInput(
                                        filter.Range.Min,
                                        filter.Range.Max,
                                        filter.Range.Inclusive),
                                filter.AllowedValues is null
                                    ? null
                                    : new GenerateAllowedValuesSpecInput(filter.AllowedValues.Values),
                                filter.TypicalRange is null
                                    ? null
                                    : new GenerateTypicalRangeSpecInput(
                                        filter.TypicalRange.MetricName,
                                        filter.TypicalRange.Method,
                                        filter.TypicalRange.Coverage,
                                        filter.TypicalRange.Params is null
                                            ? null
                                            : new GenerateTypicalRangeParamsInput(
                                                filter.TypicalRange.Params.PLow,
                                                filter.TypicalRange.Params.PHigh),
                                        filter.TypicalRange.WindowRef,
                                        filter.TypicalRange.Inclusive),
                                filter.Mode,
                                filter.Version))
                            .ToArray()))
                    .ToArray(),
                GlobalConstraints: request.GlobalConstraints is null
                    ? null
                    : new GenerateGlobalConstraintsInput(
                        request.GlobalConstraints.UniqueGames,
                        request.GlobalConstraints.SortedNumbers),
                StructuralExclusions: request.StructuralExclusions is null
                    ? null
                    : new GenerateStructuralExclusionsInput(
                        request.StructuralExclusions.MaxConsecutiveRun,
                        request.StructuralExclusions.MaxNeighborCount,
                        request.StructuralExclusions.MinRowEntropyNorm,
                        request.StructuralExclusions.MinColumnEntropyNorm,
                        request.StructuralExclusions.MaxHhiLinha,
                        request.StructuralExclusions.MaxHhiColuna,
                        request.StructuralExclusions.RepeatRange is null
                            ? null
                            : new GenerateRepeatRangeInput(
                                request.StructuralExclusions.RepeatRange.Min,
                                request.StructuralExclusions.RepeatRange.Max),
                        request.StructuralExclusions.MinSlotAlignment,
                        request.StructuralExclusions.MaxOutlierScore),
                GenerationBudget: request.GenerationBudget is null
                    ? null
                    : new GenerateGenerationBudgetInput(
                        request.GenerationBudget.MaxAttempts,
                        request.GenerationBudget.PoolMultiplier),
                GenerationMode: request.GenerationMode,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var candidateGames = result.CandidateGames.ToArray();
            if (hasPagination && pagination is not null)
            {
                candidateGames = ApplyPagination(candidateGames, pagination.Value);
            }

            var response = new GenerateCandidateGamesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                ReplayGuaranteed: result.ReplayGuaranteed,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                CandidateGames: candidateGames
                    .Select(game => new CandidateGameEnvelope(
                        game.Numbers.ToArray(),
                        game.StrategyName,
                        game.StrategyVersion,
                        game.SearchMethod,
                        game.TieBreakRule,
                        game.SeedUsed,
                        new AppliedConfigurationEnvelope(
                            Criteria: game.AppliedConfiguration.Criteria
                                .Select(criterion => new GenerateCandidateCriterionRequest(
                                    criterion.Name,
                                    criterion.Value,
                                    criterion.Range is null
                                        ? null
                                        : new GenerateRangeSpecRequest(
                                            criterion.Range.Min,
                                            criterion.Range.Max,
                                            criterion.Range.Inclusive),
                                    criterion.AllowedValues is null
                                        ? null
                                        : new GenerateAllowedValuesSpecRequest(criterion.AllowedValues.Values),
                                    criterion.TypicalRange is null
                                        ? null
                                        : new GenerateTypicalRangeSpecRequest(
                                            criterion.TypicalRange.MetricName,
                                            criterion.TypicalRange.Method,
                                            criterion.TypicalRange.Coverage,
                                            criterion.TypicalRange.Params is null
                                                ? null
                                                : new GenerateTypicalRangeParamsRequest(
                                                    criterion.TypicalRange.Params.PLow,
                                                    criterion.TypicalRange.Params.PHigh),
                                            criterion.TypicalRange.WindowRef,
                                            criterion.TypicalRange.Inclusive),
                                    criterion.Mode))
                                .ToArray(),
                            Weights: game.AppliedConfiguration.Weights
                                .Select(weight => new GenerateCandidateWeightRequest(
                                    weight.Name,
                                    weight.Weight))
                                .ToArray(),
                            Filters: game.AppliedConfiguration.Filters
                                .Select(filter => new GenerateCandidateFilterRequest(
                                    filter.Name,
                                    filter.Value,
                                    filter.Min,
                                    filter.Max,
                                    filter.Range is null
                                        ? null
                                        : new GenerateRangeSpecRequest(
                                            filter.Range.Min,
                                            filter.Range.Max,
                                            filter.Range.Inclusive),
                                    filter.AllowedValues is null
                                        ? null
                                        : new GenerateAllowedValuesSpecRequest(filter.AllowedValues.Values),
                                    filter.TypicalRange is null
                                        ? null
                                        : new GenerateTypicalRangeSpecRequest(
                                            filter.TypicalRange.MetricName,
                                            filter.TypicalRange.Method,
                                            filter.TypicalRange.Coverage,
                                            filter.TypicalRange.Params is null
                                                ? null
                                                : new GenerateTypicalRangeParamsRequest(
                                                    filter.TypicalRange.Params.PLow,
                                                    filter.TypicalRange.Params.PHigh),
                                            filter.TypicalRange.WindowRef,
                                            filter.TypicalRange.Inclusive),
                                    filter.Mode,
                                    filter.Version))
                                .ToArray(),
                            ResolvedDefaults: game.AppliedConfiguration.ResolvedDefaults)))
                    .ToArray());

            if (requestedFields is null)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
            foreach (var f in requestedFields)
            {
                keep.Add(f);
            }
            element = ResponseTransforms.Project(element, keep);
            return element;
        });
    }

    public object ExplainCandidateGames(ExplainCandidateGamesRequest request)
    {
        return ExecuteWithDatasetHandling(() =>
        {
            var requestedFields = ResponseTransforms.NormalizeFields(request.Fields);
            var allowedFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window",
                "candidate_generation_audit",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id",
                "candidate_generation_audit.requested_generation_mode",
                "candidate_generation_audit.effective_generation_mode",
                "candidate_generation_audit.context_supplied",
                "candidate_generation_audit.seed_declared",
                "candidate_generation_audit.replay_guaranteed",
                "candidate_generation_audit.intersection_and_restrictions",
                "candidate_generation_audit.replay_and_seed_policy",
                "explanations",
                "explanations.game",
                "explanations.candidate_strategies",
                "explanations.candidate_strategies.strategy_name",
                "explanations.candidate_strategies.strategy_version",
                "explanations.candidate_strategies.search_method",
                "explanations.candidate_strategies.tie_break_rule",
                "explanations.candidate_strategies.score",
                "explanations.candidate_strategies.metric_breakdown",
                "explanations.candidate_strategies.metric_breakdown.metric_name",
                "explanations.candidate_strategies.metric_breakdown.metric_version",
                "explanations.candidate_strategies.metric_breakdown.value",
                "explanations.candidate_strategies.metric_breakdown.contribution",
                "explanations.candidate_strategies.metric_breakdown.explanation",
                "explanations.candidate_strategies.exclusion_breakdown",
                "explanations.candidate_strategies.exclusion_breakdown.exclusion_name",
                "explanations.candidate_strategies.exclusion_breakdown.exclusion_version",
                "explanations.candidate_strategies.exclusion_breakdown.passed",
                "explanations.candidate_strategies.exclusion_breakdown.observed_value",
                "explanations.candidate_strategies.exclusion_breakdown.threshold",
                "explanations.candidate_strategies.exclusion_breakdown.explanation",
                "explanations.candidate_strategies.constraint_breakdown",
                "explanations.candidate_strategies.constraint_breakdown.kind",
                "explanations.candidate_strategies.constraint_breakdown.name",
                "explanations.candidate_strategies.constraint_breakdown.mode",
                "explanations.candidate_strategies.constraint_breakdown.observed_value",
                "explanations.candidate_strategies.constraint_breakdown.applied",
                "explanations.candidate_strategies.constraint_breakdown.applied.value",
                "explanations.candidate_strategies.constraint_breakdown.applied.range",
                "explanations.candidate_strategies.constraint_breakdown.applied.range.min",
                "explanations.candidate_strategies.constraint_breakdown.applied.range.max",
                "explanations.candidate_strategies.constraint_breakdown.applied.range.inclusive",
                "explanations.candidate_strategies.constraint_breakdown.applied.allowed_values",
                "explanations.candidate_strategies.constraint_breakdown.applied.allowed_values.values",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.metric_name",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.method",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.coverage",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.resolved_range",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.resolved_range.min",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.resolved_range.max",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.resolved_range.inclusive",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.coverage_observed",
                "explanations.candidate_strategies.constraint_breakdown.applied.typical_range.method_version",
                "explanations.candidate_strategies.constraint_breakdown.result",
                "explanations.candidate_strategies.constraint_breakdown.result.passed",
                "explanations.candidate_strategies.constraint_breakdown.result.penalty",
                "explanations.candidate_strategies.constraint_breakdown.explanation",
                "explanations.candidate_strategies.explanation"
            };
            var requiredFields = new HashSet<string>(StringComparer.Ordinal)
            {
                "dataset_version",
                "tool_version",
                "deterministic_hash",
                "window.size",
                "window.start_contest_id",
                "window.end_contest_id"
            };

            var fieldsError = ResponseTransforms.ValidateFields("explain_candidate_games", requestedFields, allowedFields);
            if (fieldsError is not null)
            {
                return fieldsError;
            }

            if (!TryGetFixturePath(out var fixturePath, out var datasetError))
            {
                return datasetError!;
            }

            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _explainCandidateGamesUseCase.Execute(new ExplainCandidateGamesInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Games: request.Games ?? Array.Empty<IReadOnlyList<int>>(),
                IncludeMetricBreakdown: request.IncludeMetricBreakdown,
                IncludeExclusionBreakdown: request.IncludeExclusionBreakdown,
                GenerationMode: request.GenerationMode,
                Seed: request.Seed,
                ReplayGuaranteed: request.ReplayGuaranteed,
                FixturePath: fixturePath));

            var hasPagination = TryResolvePagination(request.Verbosity, request.Page, request.PageSize, out var pagination, out var paginationError);
            if (paginationError is not null)
            {
                return paginationError;
            }

            var hashInput = new Dictionary<string, object?>
            {
                ["core"] = result.DeterministicHashInput,
                ["include_explanations"] = request.IncludeExplanations,
                ["fields"] = requestedFields,
                ["verbosity"] = ResolveVerbosity(request.Verbosity)
            };
            if (hasPagination && pagination is not null)
            {
                hashInput["pagination"] = new { page = pagination.Value.Page, page_size = pagination.Value.PageSize };
            }

            var deterministicHash = _deterministicHashService.Compute(
                hashInput,
                result.DatasetVersion,
                result.ToolVersion);

            var explanations = result.Explanations.ToArray();
            if (hasPagination && pagination is not null)
            {
                explanations = ApplyPagination(explanations, pagination.Value);
            }

            var generationAudit = result.GenerationAudit;
            var response = new ExplainCandidateGamesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                CandidateGenerationAudit: new CandidateGenerationAuditEnvelope(
                    generationAudit.RequestedGenerationMode,
                    generationAudit.EffectiveGenerationMode,
                    generationAudit.ContextSupplied,
                    generationAudit.SeedDeclared,
                    generationAudit.ReplayGuaranteed,
                    generationAudit.IntersectionAndRestrictions,
                    generationAudit.ReplayAndSeedPolicy),
                Explanations: explanations
                    .Select(game => new GameExplanationEnvelope(
                        Game: game.Game.ToArray(),
                        CandidateStrategies: game.CandidateStrategies
                            .Select(strategy => new CandidateStrategyExplanationEnvelope(
                                StrategyName: strategy.StrategyName,
                                StrategyVersion: strategy.StrategyVersion,
                                SearchMethod: strategy.SearchMethod,
                                TieBreakRule: strategy.TieBreakRule,
                                Score: strategy.Score,
                                MetricBreakdown: strategy.MetricBreakdown
                                    .Select(metric => new MetricBreakdownEntryEnvelope(
                                        metric.MetricName,
                                        metric.MetricVersion,
                                        metric.Value,
                                        metric.Contribution,
                                        metric.Explanation))
                                    .ToArray(),
                                ExclusionBreakdown: strategy.ExclusionBreakdown
                                    .Select(exclusion => new ExclusionBreakdownEntryEnvelope(
                                        exclusion.ExclusionName,
                                        exclusion.ExclusionVersion,
                                        exclusion.Passed,
                                        exclusion.ObservedValue,
                                        exclusion.Threshold,
                                        exclusion.Explanation))
                                    .ToArray(),
                                ConstraintBreakdown: strategy.ConstraintBreakdown
                                    .Select(constraint => new ConstraintBreakdownEntryEnvelope(
                                        constraint.Kind,
                                        constraint.Name,
                                        constraint.Mode,
                                        constraint.ObservedValue,
                                        new ConstraintSpecEnvelope(
                                            constraint.Applied.Value,
                                            constraint.Applied.Range is null
                                                ? null
                                                : new ConstraintRangeEnvelope(
                                                    constraint.Applied.Range.Min,
                                                    constraint.Applied.Range.Max,
                                                    constraint.Applied.Range.Inclusive),
                                            constraint.Applied.AllowedValues is null
                                                ? null
                                                : new ConstraintAllowedValuesEnvelope(
                                                    constraint.Applied.AllowedValues.Values.ToArray()),
                                            constraint.Applied.TypicalRange is null
                                                ? null
                                                : new ConstraintTypicalRangeEnvelope(
                                                    constraint.Applied.TypicalRange.MetricName,
                                                    constraint.Applied.TypicalRange.Method,
                                                    constraint.Applied.TypicalRange.Coverage,
                                                    new ConstraintRangeEnvelope(
                                                        constraint.Applied.TypicalRange.ResolvedRange.Min,
                                                        constraint.Applied.TypicalRange.ResolvedRange.Max,
                                                        constraint.Applied.TypicalRange.ResolvedRange.Inclusive),
                                                    constraint.Applied.TypicalRange.CoverageObserved,
                                                    constraint.Applied.TypicalRange.MethodVersion)),
                                        new ConstraintResultEnvelope(
                                            constraint.Result.Passed,
                                            constraint.Result.Penalty),
                                        constraint.Explanation))
                                    .ToArray()))
                            .ToArray()))
                    .ToArray());

            if (requestedFields is null && request.IncludeExplanations)
            {
                return response;
            }

            var element = JsonSerializer.SerializeToElement(response);
            if (!request.IncludeExplanations)
            {
                element = ResponseTransforms.StripExplanations(element);
            }

            if (requestedFields is not null)
            {
                var keep = new HashSet<string>(requiredFields, StringComparer.Ordinal);
                foreach (var f in requestedFields)
                {
                    keep.Add(f);
                }
                element = ResponseTransforms.Project(element, keep);
            }

            return element;
        });
    }

    private bool TryGetFixturePath(out string fixturePath, out ContractErrorEnvelope? error)
    {
        error = null;
        fixturePath = string.Empty;

        if (string.IsNullOrWhiteSpace(_drawsSourceUri))
        {
            error = ToContractError(
                "DATASET_UNAVAILABLE",
                "Dataset source is not configured.",
                new Dictionary<string, object?>
                {
                    ["reason"] = "missing_env",
                    ["missing_env"] = "Dataset__DrawsSourceUri",
                    ["accepted_schemes"] = new[] { "file", "http", "https" },
                    ["accepted_formats"] = new[] { "csv", "json" },
                    ["examples"] = new[]
                    {
                        @"tests/fixtures/synthetic_min_window.json",
                        @"file:///C:/_projeto/Lotofacil-IA/tests/fixtures/synthetic_min_window.json"
                    }
                });
            return false;
        }

        var trimmed = _drawsSourceUri.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
        {
            if (string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                if (_httpSnapshotCache is null)
                {
                    error = ToDatasetUnavailable(
                        reason: "unreachable",
                        message: "HTTP dataset source is not available in this runtime configuration.",
                        source: trimmed);
                    return false;
                }

                var snapshot = _httpSnapshotCache.GetOrCreateSnapshot(trimmed);
                if (!snapshot.Success || string.IsNullOrWhiteSpace(snapshot.SnapshotPath))
                {
                    error = ToDatasetUnavailable(
                        reason: snapshot.FailureReason ?? "unreachable",
                        message: "Unable to load dataset from remote HTTP source.",
                        source: trimmed);
                    return false;
                }

                fixturePath = snapshot.SnapshotPath;
                return true;
            }

            if (absoluteUri.IsFile)
            {
                trimmed = absoluteUri.LocalPath;
            }
        }

        var fullPath = Path.IsPathRooted(trimmed)
            ? Path.GetFullPath(trimmed)
            : Path.GetFullPath(Path.Combine(_contentRootPath, trimmed));

        if (!File.Exists(fullPath))
        {
            error = ToContractError(
                "DATASET_UNAVAILABLE",
                "Dataset source is unreachable.",
                new Dictionary<string, object?>
                {
                    ["reason"] = "unreachable",
                    ["source"] = _drawsSourceUri,
                    ["accepted_schemes"] = new[] { "file", "http", "https" },
                    ["accepted_formats"] = new[] { "csv", "json" },
                    ["examples"] = new[]
                    {
                        @"tests/fixtures/synthetic_min_window.json",
                        @"file:///C:/_projeto/Lotofacil-IA/tests/fixtures/synthetic_min_window.json"
                    }
                });
            return false;
        }

        fixturePath = fullPath;
        return true;
    }

    private object ExecuteWithDatasetHandling(Func<object> action)
    {
        try
        {
            return action();
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
        catch (System.Text.Json.JsonException)
        {
            return ToDatasetUnavailable(
                reason: "invalid_format",
                message: "Dataset JSON is invalid.",
                source: _drawsSourceUri);
        }
        catch (DomainInvariantViolationException ex)
        {
            return ToDatasetUnavailable(
                reason: "invalid_data",
                message: ex.Message,
                source: _drawsSourceUri);
        }
        catch (InvalidOperationException ex)
        {
            // SyntheticFixtureProvider uses InvalidOperationException for schema issues (e.g., missing/empty draws)
            return ToDatasetUnavailable(
                reason: "invalid_data",
                message: ex.Message,
                source: _drawsSourceUri);
        }
        catch (IOException ex)
        {
            return ToDatasetUnavailable(
                reason: "unreachable",
                message: ex.Message,
                source: _drawsSourceUri);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToDatasetUnavailable(
                reason: "unreachable",
                message: ex.Message,
                source: _drawsSourceUri);
        }
    }

    private static bool TryResolvePagination(
        string? verbosity,
        int? page,
        int? pageSize,
        out PaginationSpec? spec,
        out ContractErrorEnvelope? error)
    {
        spec = null;
        error = null;

        if (page is null && pageSize is null)
        {
            return false;
        }

        // ADR 0023 / mcp-tool-contract: paginação é suportada apenas para respostas grandes em verbosity="full".
        if (!string.Equals(ResolveVerbosity(verbosity), "full", StringComparison.Ordinal))
        {
            error = ToContractError(
                code: "INVALID_REQUEST",
                message: "Pagination requires verbosity=full.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "page",
                    ["constraint"] = "page/page_size are only allowed when verbosity=full",
                    ["verbosity"] = ResolveVerbosity(verbosity)
                });
            return false;
        }

        var resolvedPage = page ?? 1;
        var resolvedPageSize = pageSize ?? DefaultPageSizeFull;

        if (resolvedPage < 1)
        {
            error = ToContractError(
                code: "INVALID_REQUEST",
                message: "Invalid pagination parameter.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "page",
                    ["constraint"] = "page >= 1",
                    ["value"] = resolvedPage
                });
            return false;
        }

        if (resolvedPageSize < 1 || resolvedPageSize > MaxPageSizeFull)
        {
            error = ToContractError(
                code: "INVALID_REQUEST",
                message: "Invalid pagination parameter.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "page_size",
                    ["constraint"] = $"1 <= page_size <= {MaxPageSizeFull}",
                    ["value"] = resolvedPageSize
                });
            return false;
        }

        spec = new PaginationSpec(resolvedPage, resolvedPageSize);
        return true;
    }

    private static T[] ApplyPagination<T>(IReadOnlyList<T> items, PaginationSpec spec)
    {
        if (items.Count == 0)
        {
            return Array.Empty<T>();
        }

        var offset = (long)(spec.Page - 1) * spec.PageSize;
        if (offset >= items.Count)
        {
            return Array.Empty<T>();
        }

        var take = Math.Min(spec.PageSize, items.Count - (int)offset);
        var pageItems = new T[take];
        for (var i = 0; i < take; i++)
        {
            pageItems[i] = items[(int)offset + i];
        }
        return pageItems;
    }

    private static ContractErrorEnvelope ToDatasetUnavailable(string reason, string message, string? source)
    {
        var details = new Dictionary<string, object?>
        {
            ["reason"] = reason,
            ["source"] = source ?? string.Empty,
            ["accepted_schemes"] = new[] { "file", "http", "https" },
            ["accepted_formats"] = new[] { "csv", "json" },
            ["examples"] = new[]
            {
                @"tests/fixtures/synthetic_min_window.json",
                @"file:///C:/_projeto/Lotofacil-IA/tests/fixtures/synthetic_min_window.json"
            }
        };

        return ToContractError("DATASET_UNAVAILABLE", message, details);
    }

    private static ContractErrorEnvelope ToContractError(
        string code,
        string message,
        IReadOnlyDictionary<string, object?> details)
    {
        return new ContractErrorEnvelope(new ContractError(code, message, details));
    }
}

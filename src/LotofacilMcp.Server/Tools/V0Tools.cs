using System.Text.Json.Serialization;
using System.Text.Json;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Application.Windows;
using LotofacilMcp.Domain.Analytics;
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
    [property: JsonPropertyName("allow_pending")] bool AllowPending = false);

public sealed record GetDrawWindowRequest(
    [property: JsonPropertyName("window_size")] int? WindowSize = null,
    [property: JsonPropertyName("start_contest_id")] int? StartContestId = null,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId = null);

public sealed record DiscoverCapabilitiesRequest();

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
    [property: JsonPropertyName("min_history")] int MinHistory = 20);

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
    [property: JsonPropertyName("top_k")] int TopK = 10);

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
    [property: JsonPropertyName("stability_check")] AssociationStabilityCheckRequest? StabilityCheck = null);

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
    [property: JsonPropertyName("range_method")] string RangeMethod = "iqr");

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
    [property: JsonPropertyName("aggregates")] IReadOnlyList<WindowAggregateRequestDto>? Aggregates = null);

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
    [property: JsonPropertyName("generation_mode")] string? GenerationMode = null);

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
    [property: JsonPropertyName("include_exclusion_breakdown")] bool IncludeExclusionBreakdown = true);

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

public sealed record ExplainCandidateGamesResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
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
    private readonly string _fixturePath;
    private const string HelpToolVersion = "1.0.0";
    private const string DiscoverCapabilitiesToolVersion = "1.1.0";

    public V0Tools(string? fixturePath = null)
    {
        var mapper = new V0RequestMapper(new DrawNormalizer());
        var fixtureProvider = new SyntheticFixtureProvider();
        var datasetVersionService = new DatasetVersionService();
        var validator = new V0CrossFieldValidator();

        var frequencyByDezena = new FrequencyByDezenaMetric();
        var windowMetricDispatcher = new WindowMetricDispatcher(
            frequencyByDezena,
            new Top10MaisSorteadosMetric(frequencyByDezena),
            new Top10MenosSorteadosMetric(frequencyByDezena),
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

        _fixturePath = fixturePath ?? GetDefaultFixturePath();
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
                "3) Escolha o período (se não souber o último concurso, peça para ancorar no mais recente)\n\n" +
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
        var _ = request;
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

        var response = new DiscoverCapabilitiesResponse(
            ToolVersion: DiscoverCapabilitiesToolVersion,
            DeterministicHash: string.Empty,
            BuildProfile: "v0",
            DatasetRequirements:
            [
                "requires synthetic fixture json path configured in server build/runtime"
            ],
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
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>(),
                    Capabilities: "Returns deterministic build-surface metadata without executing metrics."),
                new ToolCapabilityEnvelope(
                    Name: "compute_window_metrics",
                    ToolVersion: ComputeWindowMetricsUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["window_modes"] = ["window_size+end_contest_id", "start_contest_id+end_contest_id"],
                        ["allow_pending"] = ["false", "true"],
                        ["metric_names"] = computeWindowAllowed
                    },
                    Capabilities: "Computes cataloged metrics allowed in this build for a resolved window."),
                new ToolCapabilityEnvelope(
                    Name: "analyze_indicator_stability",
                    ToolVersion: AnalyzeIndicatorStabilityUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["normalization_method"] = ["madn", "coefficient_of_variation"],
                        ["aggregation"] = ["identity", "mean", "max", "l2_norm", "per_component"]
                    },
                    Capabilities: "Ranks indicator stability using scalarized series and supported normalization methods."),
                new ToolCapabilityEnvelope(
                    Name: "compose_indicator_analysis",
                    ToolVersion: ComposeIndicatorAnalysisUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
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
                        ["metric_name"] = composeAllowedComponents
                    },
                    Capabilities: "Builds deterministic weighted compositions over supported dezena indicators."),
                new ToolCapabilityEnvelope(
                    Name: "analyze_indicator_associations",
                    ToolVersion: AnalyzeIndicatorAssociationsUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["method"] = ["spearman"],
                        ["aggregation"] = ["identity", "mean", "max", "l2_norm", "per_component"],
                        ["indicator_name"] = associationAllowedIndicators,
                        ["stability_check.method"] = ["rolling_window"],
                        ["stability_check.required_fields"] = ["subwindow_size", "stride", "min_subwindows"]
                    },
                    Capabilities: "Computes association magnitude and optional deterministic subwindow stability for compatible scalarized series."),
                new ToolCapabilityEnvelope(
                    Name: "summarize_window_patterns",
                    ToolVersion: SummarizeWindowPatternsUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["range_method"] = ["iqr"],
                        ["feature_metric_name"] = ["pares_no_concurso"],
                        ["aggregation"] = ["identity"]
                    },
                    Capabilities: "Summarizes window pattern distributions with deterministic IQR statistics."),
                new ToolCapabilityEnvelope(
                    Name: "summarize_window_aggregates",
                    ToolVersion: SummarizeWindowAggregatesUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["aggregate_type"] =
                        [
                            "histogram_scalar_series",
                            "topk_patterns_count_vector5_series",
                            "histogram_count_vector5_series_per_position_matrix"
                        ],
                        ["source_metric_name"] = summarizeAllowedSources
                    },
                    Capabilities: "Builds canonical aggregate payloads over implemented source metrics."),
                new ToolCapabilityEnvelope(
                    Name: "generate_candidate_games",
                    ToolVersion: GenerateCandidateGamesUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["strategy_name"] = ["common_repetition_frequency", "declared_composite_profile"],
                        ["search_method"] = ["exhaustive", "sampled", "greedy_topk"],
                        ["generation_mode"] = [GenerationModes.RandomUnrestricted, GenerationModes.BehaviorFiltered],
                        ["plan.criteria"] = ["name", "value|range|allowed_values|typical_range", "mode"],
                        ["plan.weights"] = ["name", "weight"],
                        ["plan.filters"] = ["name", "value|min|max|range|allowed_values|typical_range", "mode", "version"]
                    },
                    Capabilities: "Generates candidate games; max sum of plan count per request, generation modes, and seed policy follow generation envelope."),
                new ToolCapabilityEnvelope(
                    Name: "explain_candidate_games",
                    ToolVersion: ExplainCandidateGamesUseCase.ToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["flags"] = ["include_metric_breakdown", "include_exclusion_breakdown"]
                    },
                    Capabilities: "Explains candidate strategies and exclusion breakdowns for provided games."),
                new ToolCapabilityEnvelope(
                    Name: "help",
                    ToolVersion: HelpToolVersion,
                    SupportedParameters: new Dictionary<string, IReadOnlyList<string>>(),
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
                build_profile = response.BuildProfile,
                dataset_requirements = response.DatasetRequirements,
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
        try
        {
            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _computeWindowMetricsUseCase.Execute(new ComputeWindowMetricsInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                Metrics: request.Metrics?.Select(metric => new MetricRequestInput(metric.Name)).ToArray(),
                AllowPending: request.AllowPending,
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new ComputeWindowMetricsResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Metrics: result.Metrics
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object GetDrawWindow(GetDrawWindowRequest request)
    {
        try
        {
            var (windowSize, endContestId) = WindowRequestResolver.Resolve(
                request.WindowSize,
                request.StartContestId,
                request.EndContestId);

            var result = _getDrawWindowUseCase.Execute(new GetDrawWindowInput(
                WindowSize: windowSize,
                EndContestId: endContestId,
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new GetDrawWindowResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Draws: result.Draws
                    .Select(draw => new DrawDto(draw.ContestId, draw.DrawDate, draw.Numbers.ToArray()))
                    .ToArray());
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object AnalyzeIndicatorStability(AnalyzeIndicatorStabilityRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new AnalyzeIndicatorStabilityResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                NormalizationMethod: result.NormalizationMethod,
                Ranking: result.Ranking
                    .Select(entry => new StabilityRankingEntryEnvelope(
                        entry.IndicatorName,
                        entry.Aggregation,
                        entry.ComponentIndex,
                        entry.Shape,
                        entry.Dispersion,
                        entry.StabilityScore,
                        entry.Explanation))
                    .ToArray());
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object ComposeIndicatorAnalysis(ComposeIndicatorAnalysisRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new ComposeIndicatorAnalysisResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Target: result.Target,
                Operator: result.Operator,
                Ranking: result.Ranking
                    .Select(entry => new WeightedDezenaRankingEntryEnvelope(
                        entry.Dezena,
                        entry.Rank,
                        entry.Score,
                        entry.Explanation))
                    .ToArray());
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object AnalyzeIndicatorAssociations(AnalyzeIndicatorAssociationsRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new AnalyzeIndicatorAssociationsResponse(
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object SummarizeWindowPatterns(SummarizeWindowPatternsRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new SummarizeWindowPatternsResponse(
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object SummarizeWindowAggregates(SummarizeWindowAggregatesRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new SummarizeWindowAggregatesResponse(
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object GenerateCandidateGames(GenerateCandidateGamesRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new GenerateCandidateGamesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                ReplayGuaranteed: result.ReplayGuaranteed,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                CandidateGames: result.CandidateGames
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    public object ExplainCandidateGames(ExplainCandidateGamesRequest request)
    {
        try
        {
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
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new ExplainCandidateGamesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
                Window: new WindowEnvelope(
                    result.Window.Size,
                    result.Window.StartContestId,
                    result.Window.EndContestId),
                Explanations: result.Explanations
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
        }
        catch (ApplicationValidationException ex)
        {
            return ToContractError(ex.Code, ex.Message, ex.Details);
        }
    }

    private static ContractErrorEnvelope ToContractError(
        string code,
        string message,
        IReadOnlyDictionary<string, object?> details)
    {
        return new ContractErrorEnvelope(new ContractError(code, message, details));
    }

    private static string GetDefaultFixturePath()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
    }
}

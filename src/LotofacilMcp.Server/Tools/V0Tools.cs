using System.Text.Json.Serialization;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Analytics;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Normalization;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.CanonicalJson;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Hashing;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Server.Tools;

public sealed record MetricRequest([property: JsonPropertyName("name")] string Name);

public sealed record ComputeWindowMetricsRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("metrics")] IReadOnlyList<MetricRequest>? Metrics,
    [property: JsonPropertyName("allow_pending")] bool AllowPending = false);

public sealed record GetDrawWindowRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId);

public sealed record StabilityIndicatorRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("aggregation")] string? Aggregation);

public sealed record AnalyzeIndicatorStabilityRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("indicators")] IReadOnlyList<StabilityIndicatorRequestDto>? Indicators,
    [property: JsonPropertyName("normalization_method")] string? NormalizationMethod,
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
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("target")] string Target,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("components")] IReadOnlyList<ComposeIndicatorComponentRequest> Components,
    [property: JsonPropertyName("top_k")] int TopK);

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
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("items")] IReadOnlyList<AssociationItemRequest>? Items,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("top_k")] int TopK = 5,
    [property: JsonPropertyName("stability_check")] object? StabilityCheck = null);

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

public sealed record AnalyzeIndicatorAssociationsResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("association_magnitude")] AssociationMagnitudeEnvelope AssociationMagnitude,
    [property: JsonPropertyName("association_stability")] object? AssociationStability);

public sealed record WindowPatternFeatureRequest(
    [property: JsonPropertyName("metric_name")] string MetricName,
    [property: JsonPropertyName("aggregation")] string? Aggregation);

public sealed record SummarizeWindowPatternsRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("features")] IReadOnlyList<WindowPatternFeatureRequest>? Features,
    [property: JsonPropertyName("coverage_threshold")] double CoverageThreshold,
    [property: JsonPropertyName("range_method")] string RangeMethod);

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

public sealed record GenerateCandidatePlanItemRequest(
    [property: JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("search_method")] string? SearchMethod);

public sealed record GenerateCandidateGamesRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("seed")] ulong? Seed,
    [property: JsonPropertyName("plan")] IReadOnlyList<GenerateCandidatePlanItemRequest>? Plan);

public sealed record CandidateGameEnvelope(
    [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers,
    [property: JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyName("strategy_version")] string StrategyVersion,
    [property: JsonPropertyName("search_method")] string SearchMethod,
    [property: JsonPropertyName("tie_break_rule")] string TieBreakRule,
    [property: JsonPropertyName("seed_used"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ulong? SeedUsed);

public sealed record GenerateCandidateGamesResponse(
    [property: JsonPropertyName("dataset_version")] string DatasetVersion,
    [property: JsonPropertyName("tool_version")] string ToolVersion,
    [property: JsonPropertyName("deterministic_hash")] string DeterministicHash,
    [property: JsonPropertyName("window")] WindowEnvelope Window,
    [property: JsonPropertyName("candidate_games")] IReadOnlyList<CandidateGameEnvelope> CandidateGames);

public sealed record ExplainCandidateGamesRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId,
    [property: JsonPropertyName("games")] IReadOnlyList<IReadOnlyList<int>>? Games,
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

public sealed record CandidateStrategyExplanationEnvelope(
    [property: JsonPropertyName("strategy_name")] string StrategyName,
    [property: JsonPropertyName("strategy_version")] string StrategyVersion,
    [property: JsonPropertyName("search_method")] string SearchMethod,
    [property: JsonPropertyName("tie_break_rule")] string TieBreakRule,
    [property: JsonPropertyName("score")] double Score,
    [property: JsonPropertyName("metric_breakdown")] IReadOnlyList<MetricBreakdownEntryEnvelope> MetricBreakdown,
    [property: JsonPropertyName("exclusion_breakdown")] IReadOnlyList<ExclusionBreakdownEntryEnvelope> ExclusionBreakdown);

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
    private readonly GenerateCandidateGamesUseCase _generateCandidateGamesUseCase;
    private readonly ExplainCandidateGamesUseCase _explainCandidateGamesUseCase;
    private readonly DeterministicHashService _deterministicHashService;
    private readonly string _fixturePath;

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
            new AssimetriaBlocosMetric());
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

    public object ComputeWindowMetrics(ComputeWindowMetricsRequest request)
    {
        try
        {
            var result = _computeWindowMetricsUseCase.Execute(new ComputeWindowMetricsInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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
            var result = _getDrawWindowUseCase.Execute(new GetDrawWindowInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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
            var result = _analyzeIndicatorStabilityUseCase.Execute(new AnalyzeIndicatorStabilityInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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
            var result = _composeIndicatorAnalysisUseCase.Execute(new ComposeIndicatorAnalysisInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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
        if (request.StabilityCheck is not null)
        {
            return ToContractError(
                "UNSUPPORTED_STABILITY_CHECK",
                "stability_check subwindow computation is not supported in this build.",
                new Dictionary<string, object?>
                {
                    ["field"] = "stability_check"
                });
        }

        try
        {
            var result = _analyzeIndicatorAssociationsUseCase.Execute(new AnalyzeIndicatorAssociationsInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
                Items: (request.Items ?? Array.Empty<AssociationItemRequest>())
                    .Select(item => new StabilityIndicatorRequestInput(item.Name, item.Aggregation))
                    .ToArray(),
                Method: request.Method,
                TopK: request.TopK,
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
                AssociationStability: result.AssociationStability);
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
            var result = _summarizeWindowPatternsUseCase.Execute(new SummarizeWindowPatternsInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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

    public object GenerateCandidateGames(GenerateCandidateGamesRequest request)
    {
        try
        {
            var result = _generateCandidateGamesUseCase.Execute(new GenerateCandidateGamesInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
                Seed: request.Seed,
                Plan: (request.Plan ?? Array.Empty<GenerateCandidatePlanItemRequest>())
                    .Select(planItem => new GenerateCandidatePlanItemInput(
                        planItem.StrategyName,
                        planItem.Count,
                        planItem.SearchMethod))
                    .ToArray(),
                FixturePath: _fixturePath));

            var deterministicHash = _deterministicHashService.Compute(
                result.DeterministicHashInput,
                result.DatasetVersion,
                result.ToolVersion);

            return new GenerateCandidateGamesResponse(
                DatasetVersion: result.DatasetVersion,
                ToolVersion: result.ToolVersion,
                DeterministicHash: deterministicHash,
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
                        game.SeedUsed))
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
            var result = _explainCandidateGamesUseCase.Execute(new ExplainCandidateGamesInput(
                WindowSize: request.WindowSize,
                EndContestId: request.EndContestId,
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
                        game.Game.ToArray(),
                        game.CandidateStrategies
                            .Select(strategy => new CandidateStrategyExplanationEnvelope(
                                strategy.StrategyName,
                                strategy.StrategyVersion,
                                strategy.SearchMethod,
                                strategy.TieBreakRule,
                                strategy.Score,
                                strategy.MetricBreakdown
                                    .Select(metric => new MetricBreakdownEntryEnvelope(
                                        metric.MetricName,
                                        metric.MetricVersion,
                                        metric.Value,
                                        metric.Contribution,
                                        metric.Explanation))
                                    .ToArray(),
                                strategy.ExclusionBreakdown
                                    .Select(exclusion => new ExclusionBreakdownEntryEnvelope(
                                        exclusion.ExclusionName,
                                        exclusion.ExclusionVersion,
                                        exclusion.Passed,
                                        exclusion.ObservedValue,
                                        exclusion.Threshold,
                                        exclusion.Explanation))
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

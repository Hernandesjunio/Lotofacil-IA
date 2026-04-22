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

public sealed class V0Tools
{
    private readonly ComputeWindowMetricsUseCase _computeWindowMetricsUseCase;
    private readonly GetDrawWindowUseCase _getDrawWindowUseCase;
    private readonly AnalyzeIndicatorStabilityUseCase _analyzeIndicatorStabilityUseCase;
    private readonly ComposeIndicatorAnalysisUseCase _composeIndicatorAnalysisUseCase;
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

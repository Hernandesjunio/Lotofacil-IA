using System.Text.Json.Serialization;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
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
    [property: JsonPropertyName("metrics")] IReadOnlyList<MetricRequest>? Metrics);

public sealed record GetDrawWindowRequest(
    [property: JsonPropertyName("window_size")] int WindowSize,
    [property: JsonPropertyName("end_contest_id")] int? EndContestId);

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
    [property: JsonPropertyName("value")] IReadOnlyList<int> Value,
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

public sealed class V0Tools
{
    private readonly ComputeWindowMetricsUseCase _computeWindowMetricsUseCase;
    private readonly GetDrawWindowUseCase _getDrawWindowUseCase;
    private readonly DeterministicHashService _deterministicHashService;
    private readonly string _fixturePath;

    public V0Tools(string? fixturePath = null)
    {
        var mapper = new V0RequestMapper(new DrawNormalizer());
        var fixtureProvider = new SyntheticFixtureProvider();
        var datasetVersionService = new DatasetVersionService();
        var validator = new V0CrossFieldValidator();

        _computeWindowMetricsUseCase = new ComputeWindowMetricsUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
            new FrequencyByDezenaMetric(),
            validator,
            mapper);

        _getDrawWindowUseCase = new GetDrawWindowUseCase(
            fixtureProvider,
            datasetVersionService,
            new WindowResolver(),
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

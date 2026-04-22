using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record ComputeWindowMetricsInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<MetricRequestInput>? Metrics,
    bool AllowPending = false,
    string FixturePath = "");

public sealed record ComputeWindowMetricsDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    bool AllowPending,
    IReadOnlyList<MetricRequestInput> Metrics);

public sealed record ComputeWindowMetricsResult(
    string DatasetVersion,
    string ToolVersion,
    ComputeWindowMetricsDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    IReadOnlyList<FrequencyMetricValueView> Metrics);

public sealed class ComputeWindowMetricsUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public ComputeWindowMetricsUseCase(
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

    public ComputeWindowMetricsResult Execute(ComputeWindowMetricsInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateComputeWindowMetrics(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var metricValues = BuildMetricValues(window, windowView, input.Metrics!);

            return new ComputeWindowMetricsResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new ComputeWindowMetricsDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.AllowPending,
                    input.Metrics!.ToArray()),
                Window: windowView,
                Metrics: metricValues);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private IReadOnlyList<FrequencyMetricValueView> BuildMetricValues(
        DrawWindow window,
        WindowDescriptor windowView,
        IReadOnlyList<MetricRequestInput> metricRequests)
    {
        var results = new List<FrequencyMetricValueView>(metricRequests.Count);

        foreach (var metricRequest in metricRequests)
        {
            var value = _windowMetricDispatcher.Dispatch(metricRequest.Name, window);
            results.Add(new FrequencyMetricValueView(
                MetricName: value.MetricName,
                Scope: value.Scope,
                Shape: value.Shape,
                Unit: value.Unit,
                Version: value.Version,
                Window: windowView,
                Value: value.Value.ToArray(),
                Explanation: ExplanationFor(value.MetricName)));
        }

        return results;
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

    private static string ExplanationFor(string metricName) => metricName switch
    {
        "frequencia_por_dezena" => "Contagem de ocorrencias por dezena na janela resolvida.",
        "top10_mais_sorteados" =>
            "Dez dezenas com maior frequencia na janela; empates resolvidos por dezena ascendente.",
        "top10_menos_sorteados" =>
            "Dez dezenas com menor frequencia na janela; empates resolvidos por dezena ascendente.",
        "pares_no_concurso" =>
            "Série da quantidade de dezenas pares em cada concurso da janela (ordem cronologica).",
        "repeticao_concurso_anterior" =>
            "Série |J_t ∩ J_{t-1}| na janela; comprimento N ou N-1 conforme ADR 0001 D18 (predecessor fora da janela quando existir).",
        "quantidade_vizinhos_por_concurso" =>
            "Série da contagem de pares consecutivos com diferença 1 (vizinhos) em cada concurso da janela (ordem crescente canônica).",
        "sequencia_maxima_vizinhos_por_concurso" =>
            "Série do maior bloco consecutivo com diferença 1 (vizinhos) em cada concurso da janela (ordem crescente canônica).",
        _ => "Metrica de janela."
    };
}

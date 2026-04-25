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
    public const string ToolVersion = "1.1.0";

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
            var details = new Dictionary<string, object?>
            {
                ["metric_name"] = metricName
            };

            if (MetricAvailabilityCatalog.IsKnownMetric(metricName))
            {
                details["allowed_metrics"] = MetricAvailabilityCatalog.GetComputeWindowMetricsAllowedMetrics().ToArray();
            }

            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is not available in V0.",
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

    private static string ExplanationFor(string metricName) => metricName switch
    {
        "frequencia_por_dezena" => "Contagem de ocorrencias por dezena na janela resolvida.",
        "total_de_presencas_na_janela_por_dezena" =>
            "Total de presencas por dezena na janela resolvida (equivalente a frequencia_por_dezena no mesmo recorte).",
        "sequencia_atual_de_presencas_por_dezena" =>
            "Sequencia atual (streak) de presencas por dezena ao fim da janela; reinicia em 0 quando a dezena nao aparece no concurso.",
        "top10_mais_sorteados" =>
            "Dez dezenas com maior frequencia na janela; empates resolvidos por dezena ascendente.",
        "top10_menos_sorteados" =>
            "Dez dezenas com menor frequencia na janela; empates resolvidos por dezena ascendente.",
        "top10_maiores_totais_de_presencas_na_janela" =>
            "Dez dezenas com maior total de presencas na janela; empates resolvidos por dezena ascendente.",
        "top10_menores_totais_de_presencas_na_janela" =>
            "Dez dezenas com menor total de presencas na janela; empates resolvidos por dezena ascendente.",
        "pares_no_concurso" =>
            "Série da quantidade de dezenas pares em cada concurso da janela (ordem cronologica).",
        "repeticao_concurso_anterior" =>
            "Série |J_t ∩ J_{t-1}| na janela; comprimento N ou N-1 conforme ADR 0001 D18 (predecessor fora da janela quando existir).",
        "quantidade_vizinhos_por_concurso" =>
            "Série da contagem de pares consecutivos com diferença 1 (vizinhos) em cada concurso da janela (ordem crescente canônica).",
        "sequencia_maxima_vizinhos_por_concurso" =>
            "Série do maior bloco consecutivo com diferença 1 (vizinhos) em cada concurso da janela (ordem crescente canônica).",
        "distribuicao_linha_por_concurso" =>
            "Série (por concurso) da contagem de dezenas em cada linha do volante 5x5; cinco inteiros por ponto, somando 15, em blocos consecutivos na ordem das linhas 1 a 5.",
        "distribuicao_coluna_por_concurso" =>
            "Série (por concurso) da contagem de dezenas em cada coluna do volante 5x5; cinco inteiros por ponto, somando 15, em blocos consecutivos na ordem das colunas 1 a 5.",
        "entropia_linha_por_concurso" =>
            "Série da entropia de Shannon (bits) da distribuição das 15 dezenas pelas 5 linhas do volante, por concurso da janela (p_i = contagem na linha i / 15).",
        "entropia_coluna_por_concurso" =>
            "Série da entropia de Shannon (bits) da distribuição das 15 dezenas pelas 5 colunas do volante, por concurso da janela (p_i = contagem na coluna i / 15).",
        "hhi_linha_por_concurso" =>
            "Série do índice Herfindahl-Hirschman (HHI) da distribuição das dezenas pelas 5 linhas do volante, por concurso (HHI = soma (contagem_i/15)^2).",
        "hhi_coluna_por_concurso" =>
            "Série do índice Herfindahl-Hirschman (HHI) da distribuição das dezenas pelas 5 colunas do volante, por concurso (HHI = soma (contagem_i/15)^2).",
        "matriz_numero_slot" =>
            "Matriz dezena x slot (25x15), achatada por dezena e depois slot, com contagem de ocorrência após ordenação crescente canônica das dezenas em cada concurso.",
        "frequencia_blocos" =>
            "Lista de blocos consecutivos de presença por dezena, codificada de forma determinística como [dezena, quantidade_blocos, blocos...].",
        "ausencia_blocos" =>
            "Lista de blocos consecutivos de ausência por dezena, codificada de forma determinística como [dezena, quantidade_blocos, blocos...].",
        "assimetria_blocos" =>
            "Por dezena, razão (presenças - ausências)/(presenças + ausências) em blocos; desequilíbrio entre períodos de presença e ausência no recorte analisado.",
        "atraso_por_dezena" =>
            "Quantos concursos à frente do fim desta janela desde a última vez em que a dezena saiu; 0 = saiu no último sorteio do recorte.",
        "estado_atual_dezena" =>
            "Estado corrente por dezena ao fim da janela: 0 quando saiu no último concurso da janela, ou atraso atual quando não saiu.",
        "estabilidade_ranking" =>
            "Persistência de ranking entre sub-janelas contíguas via Spearman normalizado em [0,1], com empates por average rank e partição determinística.",
        _ => "Metrica de janela."
    };
}

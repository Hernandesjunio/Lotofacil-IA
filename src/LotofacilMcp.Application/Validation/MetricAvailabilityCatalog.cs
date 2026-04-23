namespace LotofacilMcp.Application.Validation;

public static class MetricAvailabilityCatalog
{
    private static readonly string[] KnownMetricNames =
    [
        "frequencia_por_dezena",
        "top10_mais_sorteados",
        "top10_menos_sorteados",
        "repeticao_concurso_anterior",
        "intersecoes_multiplas",
        "atraso_por_dezena",
        "frequencia_blocos",
        "ausencia_blocos",
        "estado_atual_dezena",
        "pares_impares",
        "pares_no_concurso",
        "quantidade_vizinhos",
        "quantidade_vizinhos_por_concurso",
        "sequencia_maxima_vizinhos",
        "sequencia_maxima_vizinhos_por_concurso",
        "distribuicao_linha",
        "distribuicao_linha_por_concurso",
        "distribuicao_coluna",
        "distribuicao_coluna_por_concurso",
        "entropia_linha",
        "entropia_linha_por_concurso",
        "entropia_coluna",
        "entropia_coluna_por_concurso",
        "hhi_concentracao",
        "hhi_linha_por_concurso",
        "hhi_coluna_por_concurso",
        "matriz_numero_slot",
        "analise_slot",
        "surpresa_slot",
        "intersecao_conjunto_referencia",
        "media_janela",
        "desvio_padrao_janela",
        "coeficiente_variacao",
        "madn_janela",
        "mad_janela",
        "tendencia_linear",
        "estabilidade_ranking",
        "divergencia_kl",
        "zscore_repeticao",
        "persistencia_atraso_extremo",
        "assimetria_blocos",
        "estatistica_runs",
        "outlier_score"
    ];

    private static readonly string[] ComputeWindowMetricsExposedNames =
    [
        "frequencia_por_dezena",
        "top10_mais_sorteados",
        "top10_menos_sorteados",
        "pares_no_concurso",
        "quantidade_vizinhos_por_concurso",
        "sequencia_maxima_vizinhos_por_concurso",
        "distribuicao_linha_por_concurso",
        "distribuicao_coluna_por_concurso",
        "entropia_linha_por_concurso",
        "entropia_coluna_por_concurso",
        "hhi_linha_por_concurso",
        "hhi_coluna_por_concurso",
        "atraso_por_dezena",
        "assimetria_blocos"
    ];

    private static readonly HashSet<string> KnownMetricNameSet = new(KnownMetricNames, StringComparer.Ordinal);
    private static readonly HashSet<string> ComputeWindowMetricsExposedSet =
        new(ComputeWindowMetricsExposedNames, StringComparer.Ordinal);

    public static bool IsKnownMetric(string metricName)
    {
        return KnownMetricNameSet.Contains(metricName);
    }

    public static bool IsExposedInComputeWindowMetrics(string metricName)
    {
        return ComputeWindowMetricsExposedSet.Contains(metricName);
    }

    public static IReadOnlyList<string> GetComputeWindowMetricsAllowedMetrics()
    {
        return ComputeWindowMetricsExposedNames;
    }
}

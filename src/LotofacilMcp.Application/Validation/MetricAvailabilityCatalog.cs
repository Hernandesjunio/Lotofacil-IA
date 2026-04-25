namespace LotofacilMcp.Application.Validation;

public static class MetricAvailabilityCatalog
{
    private static readonly MetricCapability[] Registry =
    [
        // Implemented in this build
        new("frequencia_por_dezena", "1.0.0", "window", "vector_by_dezena", "stable", true, true, true, true, true),
        new("total_de_presencas_na_janela_por_dezena", "1.0.0", "window", "vector_by_dezena", "stable", true, true, true, true, true),
        new("sequencia_atual_de_presencas_por_dezena", "1.0.0", "window", "vector_by_dezena", "stable", true, true, true, false, true),
        new("top10_mais_sorteados", "1.0.0", "window", "dezena_list[10]", "stable", true, true, true, false, false),
        new("top10_menos_sorteados", "1.0.0", "window", "dezena_list[10]", "stable", true, true, true, false, false),
        new("top10_maiores_totais_de_presencas_na_janela", "1.0.0", "window", "dezena_list[10]", "stable", true, true, true, false, false),
        new("top10_menores_totais_de_presencas_na_janela", "1.0.0", "window", "dezena_list[10]", "stable", true, true, true, false, false),
        new("repeticao_concurso_anterior", "1.0.0", "series", "series", "pending", true, true, true, true, false),
        new("atraso_por_dezena", "1.0.0", "window", "vector_by_dezena", "stable", true, true, true, false, true),
        new("pares_no_concurso", "1.0.0", "series", "series", "stable", true, true, true, true, false),
        new("quantidade_vizinhos_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, true, false),
        new("sequencia_maxima_vizinhos_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, true, false),
        new("distribuicao_linha_por_concurso", "1.0.0", "series", "series_of_count_vector[5]", "stable", true, true, true, true, false),
        new("distribuicao_coluna_por_concurso", "1.0.0", "series", "series_of_count_vector[5]", "stable", true, true, true, true, false),
        new("entropia_linha_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, true, false),
        new("entropia_coluna_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, false, false),
        new("hhi_linha_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, false, false),
        new("hhi_coluna_por_concurso", "1.0.0", "series", "series", "stable", true, true, true, false, false),
        new("assimetria_blocos", "1.0.0", "window", "vector_by_dezena", "stable", true, true, true, false, true),

        // Known by normative catalog but not implemented in this build
        new("intersecoes_multiplas", "1.0.0", "window", "count_list_by_dezena", "stable", false, false, false, false, false),
        new("frequencia_blocos", "1.0.0", "window", "count_list_by_dezena", "stable", true, true, false, false, false),
        new("ausencia_blocos", "1.0.0", "window", "count_list_by_dezena", "stable", true, true, false, false, false),
        new("estado_atual_dezena", "1.0.0", "window", "vector_by_dezena", "stable", true, true, false, false, false),
        new("pares_impares", "1.0.0", "candidate_game", "count_pair", "stable", true, false, false, false, false),
        new("quantidade_vizinhos", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("sequencia_maxima_vizinhos", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("distribuicao_linha", "1.0.0", "window", "count_vector[5]", "stable", false, false, false, false, false),
        new("distribuicao_coluna", "1.0.0", "window", "count_vector[5]", "stable", false, false, false, false, false),
        new("entropia_linha", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("entropia_coluna", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("hhi_concentracao", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("matriz_numero_slot", "1.0.0", "window", "count_matrix[25x15]", "stable", true, true, false, false, false),
        new("analise_slot", "1.0.0", "candidate_game", "scalar", "stable", true, false, false, false, false),
        new("surpresa_slot", "1.0.0", "candidate_game", "scalar", "stable", true, false, false, false, false),
        new("intersecao_conjunto_referencia", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("media_janela", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("desvio_padrao_janela", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("coeficiente_variacao", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("madn_janela", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("mad_janela", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("tendencia_linear", "1.0.0", "window", "scalar", "stable", false, false, false, false, false),
        new("estabilidade_ranking", "1.0.0", "window", "scalar", "stable", true, true, false, false, false),
        new("divergencia_kl", "1.0.0", "window", "scalar", "stable", true, false, false, false, false),
        new("zscore_repeticao", "1.0.0", "window", "series", "stable", false, false, false, false, false),
        new("persistencia_atraso_extremo", "2.0.0", "window", "scalar", "stable", true, false, false, false, false),
        new("estatistica_runs", "1.0.0", "candidate_game", "count_pair", "stable", true, false, false, false, false),
        new("outlier_score", "1.0.0", "candidate_game", "scalar", "stable", true, false, false, false, false)
    ];

    private static readonly IReadOnlyDictionary<string, MetricCapability> RegistryByName =
        Registry.ToDictionary(static entry => entry.MetricName, StringComparer.Ordinal);

    private static readonly string[] KnownMetricNames = Registry
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] ImplementedMetricNames = Registry
        .Where(static entry => entry.Implemented)
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] ComputeWindowMetricsAllowedMetrics = Registry
        .Where(static entry => entry.ComputeWindowMetrics)
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] PendingMetricNames = Registry
        .Where(static entry => string.Equals(entry.Status, "pending", StringComparison.Ordinal))
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] SummarizeWindowAggregatesAllowedSourceMetrics = Registry
        .Where(static entry => entry.SummarizeWindowAggregatesSource)
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] AssociationsAllowedIndicators = Registry
        .Where(static entry => entry.Associations)
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] CompositionAllowedComponents = Registry
        .Where(static entry => entry.ComposeIndicatorAnalysisComponent)
        .Select(static entry => entry.MetricName)
        .OrderBy(static name => name, StringComparer.Ordinal)
        .ToArray();

    public static bool IsKnownMetric(string metricName)
    {
        return RegistryByName.ContainsKey(metricName);
    }

    public static bool IsImplementedMetric(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) && entry.Implemented;
    }

    public static bool IsExposedInComputeWindowMetrics(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) && entry.ComputeWindowMetrics;
    }

    public static bool IsPendingMetric(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) &&
               string.Equals(entry.Status, "pending", StringComparison.Ordinal);
    }

    public static bool IsExposedInSummarizeWindowAggregates(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) && entry.SummarizeWindowAggregatesSource;
    }

    public static bool IsExposedInAnalyzeIndicatorAssociations(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) && entry.Associations;
    }

    public static bool IsExposedInComposeIndicatorAnalysis(string metricName)
    {
        return RegistryByName.TryGetValue(metricName, out var entry) && entry.ComposeIndicatorAnalysisComponent;
    }

    public static IReadOnlyList<string> GetComputeWindowMetricsAllowedMetrics()
    {
        return ComputeWindowMetricsAllowedMetrics;
    }

    public static IReadOnlyList<string> GetComputeWindowMetricsAllowedMetrics(bool allowPending)
    {
        if (allowPending)
        {
            return ComputeWindowMetricsAllowedMetrics;
        }

        return Registry
            .Where(static entry => entry.ComputeWindowMetrics &&
                                   !string.Equals(entry.Status, "pending", StringComparison.Ordinal))
            .Select(static entry => entry.MetricName)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<string> GetSummarizeWindowAggregatesAllowedSources()
    {
        return SummarizeWindowAggregatesAllowedSourceMetrics;
    }

    public static IReadOnlyList<string> GetAnalyzeIndicatorAssociationsAllowedIndicators()
    {
        return AssociationsAllowedIndicators;
    }

    public static IReadOnlyList<string> GetComposeIndicatorAnalysisAllowedComponents()
    {
        return CompositionAllowedComponents;
    }

    public static IReadOnlyList<string> GetKnownMetricNames()
    {
        return KnownMetricNames;
    }

    public static IReadOnlyList<string> GetImplementedMetricNames()
    {
        return ImplementedMetricNames;
    }

    public static IReadOnlyList<string> GetPendingMetricNames()
    {
        return PendingMetricNames;
    }

    public static IReadOnlyList<MetricCapability> GetRegistryEntries()
    {
        return Registry;
    }

    public sealed record MetricCapability(
        string MetricName,
        string Version,
        string Scope,
        string Shape,
        string Status,
        bool Implemented,
        bool ComputeWindowMetrics,
        bool SummarizeWindowAggregatesSource,
        bool Associations,
        bool ComposeIndicatorAnalysisComponent);
}

using LotofacilMcp.Application.Composition;
using LotofacilMcp.Application.UseCases;

namespace LotofacilMcp.Application.Validation;

public sealed class V0CrossFieldValidator
{
    private static readonly HashSet<string> SupportedComputeWindowMetrics =
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

            if (!SupportedComputeWindowMetrics.Contains(metric.Name))
            {
                throw new ApplicationValidationException(
                    code: "UNKNOWN_METRIC",
                    message: "requested metric is not available in V0.",
                    details: new Dictionary<string, object?>
                    {
                        ["metric_name"] = metric.Name
                    });
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
}

using LotofacilMcp.Application.UseCases;

namespace LotofacilMcp.Application.Validation;

public sealed class V0CrossFieldValidator
{
    private static readonly HashSet<string> SupportedComputeWindowMetrics =
    [
        "frequencia_por_dezena",
        "top10_mais_sorteados",
        "top10_menos_sorteados",
        "pares_no_concurso"
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
}

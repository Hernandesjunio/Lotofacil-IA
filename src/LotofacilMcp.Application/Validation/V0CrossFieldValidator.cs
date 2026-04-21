using LotofacilMcp.Application.UseCases;

namespace LotofacilMcp.Application.Validation;

public sealed class V0CrossFieldValidator
{
    private const string SupportedMetricName = "frequencia_por_dezena";

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

            if (!string.Equals(metric.Name, SupportedMetricName, StringComparison.Ordinal))
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
}

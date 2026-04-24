using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Generation;

public sealed record TypicalRangePercentileParams(
    double? PLow,
    double? PHigh);

public sealed record TypicalRangeSpec(
    string MetricName,
    string Method,
    double Coverage,
    TypicalRangePercentileParams? Params,
    string? WindowRef,
    bool? Inclusive);

public sealed record ResolvedRange(
    double Min,
    double Max,
    bool Inclusive);

public sealed record TypicalRangeResolution(
    ResolvedRange ResolvedRange,
    double CoverageObserved,
    string MethodVersion);

public sealed class TypicalRangeResolver
{
    public const string MethodVersion = "1.0.0";

    public TypicalRangeResolution Resolve(
        TypicalRangeSpec spec,
        IReadOnlyList<double> series)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(series);

        if (spec.Coverage is < 0d or > 1d)
        {
            throw new DomainInvariantViolationException(
                "typical_range.coverage must be in the inclusive range [0,1].");
        }

        if (series.Count == 0)
        {
            throw new DomainInvariantViolationException("typical_range cannot be resolved from an empty series.");
        }

        foreach (var value in series)
        {
            if (!double.IsFinite(value))
            {
                throw new DomainInvariantViolationException("typical_range series must contain only finite values.");
            }
        }

        var sorted = series.OrderBy(static value => value).ToArray();
        var method = spec.Method ?? string.Empty;
        var inclusive = spec.Inclusive ?? true;

        var (min, max) = method switch
        {
            "iqr" => ResolveIqr(sorted),
            "percentile" => ResolvePercentile(spec.Params, sorted),
            _ => throw new DomainInvariantViolationException($"typical_range.method is not supported: {method}.")
        };

        var countInRange = series.Count(value => inclusive
            ? value >= min && value <= max
            : value > min && value < max);
        var coverageObserved = countInRange / (double)series.Count;

        return new TypicalRangeResolution(
            ResolvedRange: new ResolvedRange(min, max, inclusive),
            CoverageObserved: coverageObserved,
            MethodVersion: MethodVersion);
    }

    private static (double Min, double Max) ResolveIqr(IReadOnlyList<double> sorted)
    {
        var q1 = ComputePercentile(sorted, 0.25d);
        var q3 = ComputePercentile(sorted, 0.75d);
        return (q1, q3);
    }

    private static (double Min, double Max) ResolvePercentile(
        TypicalRangePercentileParams? parameters,
        IReadOnlyList<double> sorted)
    {
        var pLow = parameters?.PLow;
        var pHigh = parameters?.PHigh;

        if (!pLow.HasValue || !pHigh.HasValue)
        {
            throw new DomainInvariantViolationException(
                "typical_range.method=percentile requires params.p_low and params.p_high.");
        }

        if (!double.IsFinite(pLow.Value) || !double.IsFinite(pHigh.Value))
        {
            throw new DomainInvariantViolationException(
                "typical_range percentile params must be finite numbers.");
        }

        if (pLow.Value < 0d || pHigh.Value > 1d || pLow.Value >= pHigh.Value)
        {
            throw new DomainInvariantViolationException(
                "typical_range percentile params must satisfy 0 <= p_low < p_high <= 1.");
        }

        return (
            Min: ComputePercentile(sorted, pLow.Value),
            Max: ComputePercentile(sorted, pHigh.Value));
    }

    private static double ComputePercentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        var position = (sortedValues.Count - 1) * percentile;
        var left = (int)Math.Floor(position);
        var right = (int)Math.Ceiling(position);
        if (left == right)
        {
            return sortedValues[left];
        }

        var fraction = position - left;
        return sortedValues[left] + ((sortedValues[right] - sortedValues[left]) * fraction);
    }
}

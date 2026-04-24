using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Analytics;

public sealed record AssociationMagnitudeEntry(
    string IndicatorA,
    string AggregationA,
    int? ComponentIndexA,
    string IndicatorB,
    string AggregationB,
    int? ComponentIndexB,
    double AssociationStrength,
    string Explanation);

public sealed record AssociationMagnitude(
    string Method,
    IReadOnlyList<AssociationMagnitudeEntry> TopPairs);

public sealed record AssociationStabilityCheck(
    int SubwindowSize,
    int Stride,
    int MinSubwindows);

public sealed record AssociationStabilityEntry(
    string IndicatorA,
    string AggregationA,
    int? ComponentIndexA,
    string IndicatorB,
    string AggregationB,
    int? ComponentIndexB,
    double Mean,
    double Median,
    double P10,
    double P90,
    double Min,
    double Max,
    double StdDev,
    double SignConsistencyRatio);

public sealed record AssociationStability(
    string Method,
    int SubwindowSize,
    int Stride,
    int MinSubwindows,
    int SubwindowsCount,
    IReadOnlyList<AssociationStabilityEntry> TopPairs);

public sealed record AssociationAnalysisResult(
    AssociationMagnitude Magnitude,
    AssociationStability? Stability);

public sealed class IndicatorAssociationAnalyzer
{
    public AssociationAnalysisResult AnalyzeSpearman(
        DrawWindow window,
        IReadOnlyList<StabilityIndicatorRequest> items,
        int topK,
        AssociationStabilityCheck? stabilityCheck)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(items);

        var resolved = ResolveSeries(window, items);
        var pairScores = ComputePairScores(resolved);
        var ordered = pairScores
            .OrderByDescending(pair => Math.Abs(pair.AssociationStrength))
            .ThenBy(pair => pair.A.IndicatorName, StringComparer.Ordinal)
            .ThenBy(pair => pair.A.ComponentIndex ?? -1)
            .ThenBy(pair => pair.B.IndicatorName, StringComparer.Ordinal)
            .ThenBy(pair => pair.B.ComponentIndex ?? -1)
            .Take(topK)
            .ToArray();

        var magnitude = new AssociationMagnitude(
            Method: "spearman",
            TopPairs: ordered
                .Select(pair => new AssociationMagnitudeEntry(
                    IndicatorA: pair.A.IndicatorName,
                    AggregationA: pair.A.Aggregation,
                    ComponentIndexA: pair.A.ComponentIndex,
                    IndicatorB: pair.B.IndicatorName,
                    AggregationB: pair.B.Aggregation,
                    ComponentIndexB: pair.B.ComponentIndex,
                    AssociationStrength: pair.AssociationStrength,
                    Explanation:
                    "Co-movimento monotônico (Spearman) na janela; não implica relação causal entre indicadores."))
                .ToArray());

        var stability = stabilityCheck is null
            ? null
            : ComputeStability(ordered, stabilityCheck);

        return new AssociationAnalysisResult(magnitude, stability);
    }

    private static IReadOnlyList<ResolvedIndicatorScalarSeries> ResolveSeries(
        DrawWindow window,
        IReadOnlyList<StabilityIndicatorRequest> items)
    {
        var resolved = new List<ResolvedIndicatorScalarSeries>();
        foreach (var item in items)
        {
            foreach (var series in IndicatorStabilityAnalyzer.ResolveScalarSeriesForAssociation(
                         window,
                         new StabilityIndicatorRequest(item.Name, item.Aggregation)))
            {
                resolved.Add(series);
            }
        }

        if (resolved.Count < 2)
        {
            throw new DomainInvariantViolationException("at least two resolved scalar series are required for association.");
        }

        for (var i = 0; i < resolved.Count; i++)
        {
            var series = resolved[i];
            if (series.Values.Count != window.Draws.Count)
            {
                throw new DomainInvariantViolationException("resolved series are not time-aligned to the draw window.");
            }
        }

        return resolved;
    }

    private static IReadOnlyList<AssociationPairScore> ComputePairScores(IReadOnlyList<ResolvedIndicatorScalarSeries> resolved)
    {
        var pairScores = new List<AssociationPairScore>();
        for (var i = 0; i < resolved.Count; i++)
        {
            for (var j = i + 1; j < resolved.Count; j++)
            {
                var a = resolved[i];
                var b = resolved[j];
                var rho = SpearmanCorrelation.Compute(a.Values, b.Values);
                pairScores.Add(new AssociationPairScore(a, b, rho));
            }
        }

        return pairScores;
    }

    private static AssociationStability ComputeStability(
        IReadOnlyList<AssociationPairScore> orderedPairs,
        AssociationStabilityCheck stabilityCheck)
    {
        if (orderedPairs.Count == 0)
        {
            throw new DomainInvariantViolationException("at least one pair is required for stability_check.");
        }

        var seriesLength = orderedPairs[0].A.Values.Count;
        if (stabilityCheck.SubwindowSize < 2)
        {
            throw new DomainInvariantViolationException("stability_check.subwindow_size must be greater than 1.");
        }

        if (stabilityCheck.Stride < 1)
        {
            throw new DomainInvariantViolationException("stability_check.stride must be greater than or equal to 1.");
        }

        if (stabilityCheck.MinSubwindows < 2)
        {
            throw new DomainInvariantViolationException("stability_check.min_subwindows must be greater than or equal to 2.");
        }

        if (stabilityCheck.SubwindowSize > seriesLength)
        {
            throw new DomainInvariantViolationException("stability_check.subwindow_size must be less than or equal to window_size.");
        }

        var starts = BuildSubwindowStarts(seriesLength, stabilityCheck.SubwindowSize, stabilityCheck.Stride);
        if (starts.Count < stabilityCheck.MinSubwindows)
        {
            throw new DomainInvariantViolationException("stability_check generated fewer subwindows than min_subwindows.");
        }

        var entries = orderedPairs
            .Select(pair =>
            {
                var correlations = new List<double>(starts.Count);
                foreach (var start in starts)
                {
                    var x = Slice(pair.A.Values, start, stabilityCheck.SubwindowSize);
                    var y = Slice(pair.B.Values, start, stabilityCheck.SubwindowSize);
                    correlations.Add(SpearmanCorrelation.Compute(x, y));
                }

                var stats = ComputeStatistics(correlations);
                var signRatio = ComputeSignConsistencyRatio(correlations, pair.AssociationStrength);
                return new AssociationStabilityEntry(
                    IndicatorA: pair.A.IndicatorName,
                    AggregationA: pair.A.Aggregation,
                    ComponentIndexA: pair.A.ComponentIndex,
                    IndicatorB: pair.B.IndicatorName,
                    AggregationB: pair.B.Aggregation,
                    ComponentIndexB: pair.B.ComponentIndex,
                    Mean: stats.Mean,
                    Median: stats.Median,
                    P10: stats.P10,
                    P90: stats.P90,
                    Min: stats.Min,
                    Max: stats.Max,
                    StdDev: stats.StdDev,
                    SignConsistencyRatio: signRatio);
            })
            .ToArray();

        return new AssociationStability(
            Method: "spearman",
            SubwindowSize: stabilityCheck.SubwindowSize,
            Stride: stabilityCheck.Stride,
            MinSubwindows: stabilityCheck.MinSubwindows,
            SubwindowsCount: starts.Count,
            TopPairs: entries);
    }

    private static IReadOnlyList<int> BuildSubwindowStarts(int seriesLength, int subwindowSize, int stride)
    {
        var starts = new List<int>();
        for (var start = 0; start + subwindowSize <= seriesLength; start += stride)
        {
            starts.Add(start);
        }

        return starts;
    }

    private static IReadOnlyList<double> Slice(IReadOnlyList<double> source, int start, int length)
    {
        var values = new double[length];
        for (var i = 0; i < length; i++)
        {
            values[i] = source[start + i];
        }

        return values;
    }

    private static AssociationStatistics ComputeStatistics(IReadOnlyList<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        var mean = values.Average();
        var variance = 0d;
        for (var i = 0; i < values.Count; i++)
        {
            var delta = values[i] - mean;
            variance += delta * delta;
        }

        variance /= values.Count;
        var stdDev = Math.Sqrt(variance);

        return new AssociationStatistics(
            Mean: mean,
            Median: Percentile(sorted, 0.5),
            P10: Percentile(sorted, 0.1),
            P90: Percentile(sorted, 0.9),
            Min: sorted[0],
            Max: sorted[^1],
            StdDev: stdDev);
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double p)
    {
        if (sortedValues.Count == 1)
        {
            return sortedValues[0];
        }

        var position = (sortedValues.Count - 1) * p;
        var lowerIndex = (int)Math.Floor(position);
        var upperIndex = (int)Math.Ceiling(position);
        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }

        var weight = position - lowerIndex;
        return sortedValues[lowerIndex] * (1d - weight) + (sortedValues[upperIndex] * weight);
    }

    private static double ComputeSignConsistencyRatio(IReadOnlyList<double> values, double globalAssociation)
    {
        var targetSign = Sign(globalAssociation);
        var sameSignCount = 0;
        for (var i = 0; i < values.Count; i++)
        {
            if (Sign(values[i]) == targetSign)
            {
                sameSignCount++;
            }
        }

        return sameSignCount / (double)values.Count;
    }

    private static int Sign(double value)
    {
        const double epsilon = 1e-12;
        if (Math.Abs(value) < epsilon)
        {
            return 0;
        }

        return value > 0d ? 1 : -1;
    }

    private sealed record AssociationPairScore(
        ResolvedIndicatorScalarSeries A,
        ResolvedIndicatorScalarSeries B,
        double AssociationStrength);

    private sealed record AssociationStatistics(
        double Mean,
        double Median,
        double P10,
        double P90,
        double Min,
        double Max,
        double StdDev);
}

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

public sealed class IndicatorAssociationAnalyzer
{
    public AssociationMagnitude ComputeSpearmanMagnitude(
        DrawWindow window,
        IReadOnlyList<StabilityIndicatorRequest> items,
        int topK)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(items);

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

        foreach (var series in resolved)
        {
            if (series.Values.Count != window.Draws.Count)
            {
                throw new DomainInvariantViolationException("resolved series are not time-aligned to the draw window.");
            }
        }

        var pairScores = new List<AssociationMagnitudeEntry>();
        for (var i = 0; i < resolved.Count; i++)
        {
            for (var j = i + 1; j < resolved.Count; j++)
            {
                var a = resolved[i];
                var b = resolved[j];
                var rho = SpearmanCorrelation.Compute(a.Values, b.Values);
                pairScores.Add(new AssociationMagnitudeEntry(
                    IndicatorA: a.IndicatorName,
                    AggregationA: a.Aggregation,
                    ComponentIndexA: a.ComponentIndex,
                    IndicatorB: b.IndicatorName,
                    AggregationB: b.Aggregation,
                    ComponentIndexB: b.ComponentIndex,
                    AssociationStrength: rho,
                    Explanation:
                    "Co-movimento monotônico (Spearman) na janela; não implica relação causal entre indicadores."));
            }
        }

        var ordered = pairScores
            .OrderByDescending(entry => Math.Abs(entry.AssociationStrength))
            .ThenBy(entry => entry.IndicatorA, StringComparer.Ordinal)
            .ThenBy(entry => entry.ComponentIndexA ?? -1)
            .ThenBy(entry => entry.IndicatorB, StringComparer.Ordinal)
            .ThenBy(entry => entry.ComponentIndexB ?? -1)
            .Take(topK)
            .ToArray();

        return new AssociationMagnitude("spearman", ordered);
    }
}

using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Metrics;

public sealed class EstatisticaRunsMetric
{
    public WindowMetricValue Compute(IReadOnlyList<int> candidateGame)
    {
        if (candidateGame is null || candidateGame.Count != 15)
        {
            throw new DomainInvariantViolationException("candidate_game must contain exactly 15 dezenas.");
        }

        var maxRun = VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(candidateGame);
        var neighborCount = CountAdjacentPairs(candidateGame);

        return new WindowMetricValue(
            MetricName: "estatistica_runs",
            Scope: "candidate_game",
            Shape: "count_pair",
            Unit: "count",
            Version: "1.0.0",
            Value: [maxRun, neighborCount]);
    }

    private static int CountAdjacentPairs(IReadOnlyList<int> sortedNumbers)
    {
        var count = 0;
        for (var i = 1; i < sortedNumbers.Count; i++)
        {
            if (sortedNumbers[i] - sortedNumbers[i - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }
}

using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Metrics;

public sealed class ParesImparesMetric
{
    public WindowMetricValue Compute(IReadOnlyList<int> candidateGame)
    {
        ValidateCandidateGame(candidateGame);

        var pares = candidateGame.Count(number => number % 2 == 0);
        var impares = candidateGame.Count - pares;

        return new WindowMetricValue(
            MetricName: "pares_impares",
            Scope: "candidate_game",
            Shape: "count_pair",
            Unit: "count",
            Version: "1.0.0",
            Value: [pares, impares]);
    }

    private static void ValidateCandidateGame(IReadOnlyList<int> candidateGame)
    {
        if (candidateGame is null || candidateGame.Count != 15)
        {
            throw new DomainInvariantViolationException("candidate_game must contain exactly 15 dezenas.");
        }
    }
}

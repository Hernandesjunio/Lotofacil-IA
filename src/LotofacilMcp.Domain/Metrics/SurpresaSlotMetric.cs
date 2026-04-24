using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class SurpresaSlotMetric
{
    public WindowMetricValue Compute(DrawWindow window, IReadOnlyList<int> candidateGame)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        if (candidateGame is null || candidateGame.Count != 15)
        {
            throw new DomainInvariantViolationException("candidate_game must contain exactly 15 dezenas.");
        }

        var matrix = BuildSlotFrequencyMatrix(window);
        var smoothingAlpha = 1d / 25d;
        var denominator = window.Size + (25d * smoothingAlpha);

        double score = 0d;
        for (var slot = 0; slot < 15; slot++)
        {
            var dezena = candidateGame[slot];
            var count = matrix[dezena - 1, slot];
            var p = (count + smoothingAlpha) / denominator;
            score += -Math.Log2(p);
        }

        return new WindowMetricValue(
            MetricName: "surpresa_slot",
            Scope: "candidate_game",
            Shape: "scalar",
            Unit: "bits",
            Version: "1.0.0",
            Value: [score]);
    }

    private static int[,] BuildSlotFrequencyMatrix(DrawWindow window)
    {
        var matrix = new int[25, 15];
        foreach (var draw in window.Draws)
        {
            for (var slot = 0; slot < draw.Numbers.Count; slot++)
            {
                matrix[draw.Numbers[slot] - 1, slot]++;
            }
        }

        return matrix;
    }
}

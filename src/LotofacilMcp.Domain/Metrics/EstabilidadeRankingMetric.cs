using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class EstabilidadeRankingMetric
{
    private const int DefaultBlocks = 4;

    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        if (window.Size < 2)
        {
            throw new DomainInvariantViolationException("estabilidade_ranking requires at least two concursos in the window.");
        }

        var blockCount = Math.Min(DefaultBlocks, window.Size);
        var blockSizes = BuildBalancedBlockSizes(window.Size, blockCount);
        var blockRankVectors = BuildRankVectors(window, blockSizes);

        var normalizedScores = new double[blockRankVectors.Count - 1];
        for (var i = 0; i < blockRankVectors.Count - 1; i++)
        {
            var rho = ComputePearsonWithEdgeRules(blockRankVectors[i], blockRankVectors[i + 1]);
            normalizedScores[i] = (rho + 1d) / 2d;
        }

        var score = normalizedScores.Average();
        return new WindowMetricValue(
            MetricName: "estabilidade_ranking",
            Scope: "window",
            Shape: "scalar",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: [score]);
    }

    private static List<int> BuildBalancedBlockSizes(int windowSize, int blockCount)
    {
        var baseSize = windowSize / blockCount;
        var remainder = windowSize % blockCount;
        var sizes = new List<int>(blockCount);
        for (var i = 0; i < blockCount; i++)
        {
            sizes.Add(baseSize + (i < remainder ? 1 : 0));
        }

        return sizes;
    }

    private static List<double[]> BuildRankVectors(DrawWindow window, IReadOnlyList<int> blockSizes)
    {
        var vectors = new List<double[]>(blockSizes.Count);
        var cursor = 0;
        foreach (var blockSize in blockSizes)
        {
            var frequencies = new int[25];
            for (var offset = 0; offset < blockSize; offset++)
            {
                var draw = window.Draws[cursor + offset];
                foreach (var dezena in draw.Numbers)
                {
                    frequencies[dezena - 1]++;
                }
            }

            vectors.Add(BuildAverageRanksDescending(frequencies));
            cursor += blockSize;
        }

        return vectors;
    }

    private static double[] BuildAverageRanksDescending(int[] frequencies)
    {
        var pairs = Enumerable.Range(0, frequencies.Length)
            .Select(i => (Index: i, Frequency: frequencies[i]))
            .OrderByDescending(p => p.Frequency)
            .ThenBy(p => p.Index)
            .ToArray();

        var ranks = new double[frequencies.Length];
        var position = 1;
        var cursor = 0;
        while (cursor < pairs.Length)
        {
            var tieStart = cursor;
            var frequency = pairs[cursor].Frequency;
            while (cursor < pairs.Length && pairs[cursor].Frequency == frequency)
            {
                cursor++;
            }

            var tieEnd = cursor; // exclusive
            var averageRank = ((position) + (position + (tieEnd - tieStart) - 1)) / 2d;
            for (var i = tieStart; i < tieEnd; i++)
            {
                ranks[pairs[i].Index] = averageRank;
            }

            position += (tieEnd - tieStart);
        }

        return ranks;
    }

    private static double ComputePearsonWithEdgeRules(double[] left, double[] right)
    {
        var meanLeft = left.Average();
        var meanRight = right.Average();

        var sumCov = 0d;
        var sumSqLeft = 0d;
        var sumSqRight = 0d;
        for (var i = 0; i < left.Length; i++)
        {
            var dl = left[i] - meanLeft;
            var dr = right[i] - meanRight;
            sumCov += dl * dr;
            sumSqLeft += dl * dl;
            sumSqRight += dr * dr;
        }

        var leftZeroVariance = sumSqLeft == 0d;
        var rightZeroVariance = sumSqRight == 0d;
        if (leftZeroVariance && rightZeroVariance)
        {
            return AreEqualVectors(left, right) ? 1d : 0d;
        }

        if (leftZeroVariance || rightZeroVariance)
        {
            return 0d;
        }

        return sumCov / Math.Sqrt(sumSqLeft * sumSqRight);
    }

    private static bool AreEqualVectors(double[] left, double[] right)
    {
        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }
}

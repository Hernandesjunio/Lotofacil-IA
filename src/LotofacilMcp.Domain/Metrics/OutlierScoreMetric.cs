using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class OutlierScoreMetric
{
    private readonly AnaliseSlotMetric _analiseSlotMetric = new();
    private readonly ParesImparesMetric _paresImparesMetric = new();

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

        var feature = BuildFeatureVector(window, candidateGame);
        var center = BuildCenter(window);
        var covariance = BuildCovariance(window, center);
        Regularize(covariance, 1e-6);
        var inverse = Invert5x5(covariance);
        var distance = MahalanobisDistance(feature, center, inverse);

        return new WindowMetricValue(
            MetricName: "outlier_score",
            Scope: "candidate_game",
            Shape: "scalar",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: [distance]);
    }

    private double[] BuildFeatureVector(DrawWindow window, IReadOnlyList<int> candidateGame)
    {
        var frequency = new FrequencyByDezenaMetric().Compute(window).Value;
        var maxFrequency = frequency.Max();
        var freqAlignment = candidateGame.Average(number => frequency[number - 1] / maxFrequency);

        var slotAlignment = _analiseSlotMetric.Compute(window, candidateGame).Value[0];

        var rowEntropyNorm = Math.Clamp(
            ShannonEntropyBits.FromNonNegativeCounts(BuildRowCounts(candidateGame)) / Math.Log2(5d),
            0d,
            1d);

        var pairs = _paresImparesMetric.Compute(candidateGame).Value[0];
        var pairsRatio = pairs / 15d;

        var lastDraw = window.Draws[^1].Numbers;
        var repetition = candidateGame.Count(number => lastDraw.Contains(number)) / 15d;

        return
        [
            freqAlignment,
            slotAlignment,
            rowEntropyNorm,
            pairsRatio,
            repetition
        ];
    }

    private static double[] BuildCenter(DrawWindow window)
    {
        var frequency = new FrequencyByDezenaMetric().Compute(window).Value;
        var maxFrequency = frequency.Max();

        var center = new double[5];
        foreach (var draw in window.Draws)
        {
            var freqAlignment = draw.Numbers.Average(number => frequency[number - 1] / maxFrequency);
            var slotAlignment = new AnaliseSlotMetric().Compute(window, draw.Numbers).Value[0];
            var rowEntropyNorm = Math.Clamp(
                ShannonEntropyBits.FromNonNegativeCounts(BuildRowCounts(draw.Numbers)) / Math.Log2(5d),
                0d,
                1d);
            var pairsRatio = draw.Numbers.Count(number => number % 2 == 0) / 15d;

            center[0] += freqAlignment;
            center[1] += slotAlignment;
            center[2] += rowEntropyNorm;
            center[3] += pairsRatio;
            center[4] += 1d; // draw self-intersection ratio in its own context.
        }

        for (var i = 0; i < center.Length; i++)
        {
            center[i] /= window.Draws.Count;
        }

        return center;
    }

    private static double[,] BuildCovariance(DrawWindow window, IReadOnlyList<double> center)
    {
        var frequency = new FrequencyByDezenaMetric().Compute(window).Value;
        var maxFrequency = frequency.Max();
        var slotMetric = new AnaliseSlotMetric();

        var vectors = new List<double[]>(window.Draws.Count);
        foreach (var draw in window.Draws)
        {
            vectors.Add(
            [
                draw.Numbers.Average(number => frequency[number - 1] / maxFrequency),
                slotMetric.Compute(window, draw.Numbers).Value[0],
                Math.Clamp(ShannonEntropyBits.FromNonNegativeCounts(BuildRowCounts(draw.Numbers)) / Math.Log2(5d), 0d, 1d),
                draw.Numbers.Count(number => number % 2 == 0) / 15d,
                1d
            ]);
        }

        var covariance = new double[5, 5];
        var divisor = Math.Max(1, vectors.Count - 1);
        foreach (var vector in vectors)
        {
            for (var i = 0; i < 5; i++)
            {
                var di = vector[i] - center[i];
                for (var j = 0; j < 5; j++)
                {
                    covariance[i, j] += di * (vector[j] - center[j]);
                }
            }
        }

        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                covariance[i, j] /= divisor;
            }
        }

        return covariance;
    }

    private static void Regularize(double[,] matrix, double lambda)
    {
        for (var i = 0; i < 5; i++)
        {
            matrix[i, i] += lambda;
        }
    }

    private static double[,] Invert5x5(double[,] matrix)
    {
        var n = 5;
        var augmented = new double[n, n * 2];
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                augmented[i, j] = matrix[i, j];
            }

            augmented[i, n + i] = 1d;
        }

        for (var pivot = 0; pivot < n; pivot++)
        {
            var pivotValue = augmented[pivot, pivot];
            if (Math.Abs(pivotValue) < 1e-12)
            {
                throw new DomainInvariantViolationException("outlier_score covariance matrix is singular after regularization.");
            }

            for (var col = 0; col < n * 2; col++)
            {
                augmented[pivot, col] /= pivotValue;
            }

            for (var row = 0; row < n; row++)
            {
                if (row == pivot)
                {
                    continue;
                }

                var factor = augmented[row, pivot];
                if (factor == 0d)
                {
                    continue;
                }

                for (var col = 0; col < n * 2; col++)
                {
                    augmented[row, col] -= factor * augmented[pivot, col];
                }
            }
        }

        var inverse = new double[n, n];
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                inverse[i, j] = augmented[i, n + j];
            }
        }

        return inverse;
    }

    private static double MahalanobisDistance(
        IReadOnlyList<double> feature,
        IReadOnlyList<double> center,
        double[,] inverseCovariance)
    {
        var delta = new double[5];
        for (var i = 0; i < 5; i++)
        {
            delta[i] = feature[i] - center[i];
        }

        double quadraticForm = 0d;
        for (var i = 0; i < 5; i++)
        {
            double inner = 0d;
            for (var j = 0; j < 5; j++)
            {
                inner += inverseCovariance[i, j] * delta[j];
            }

            quadraticForm += delta[i] * inner;
        }

        return Math.Sqrt(Math.Max(0d, quadraticForm));
    }

    private static int[] BuildRowCounts(IReadOnlyList<int> numbers)
    {
        var counts = new int[5];
        foreach (var number in numbers)
        {
            counts[(number - 1) / 5]++;
        }

        return counts;
    }
}

using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Analytics;

public sealed record StabilityIndicatorRequest(
    string Name,
    string? Aggregation);

public sealed record StabilityRankingEntry(
    string IndicatorName,
    string Aggregation,
    int? ComponentIndex,
    string Shape,
    double Dispersion,
    double StabilityScore,
    string Explanation);

public sealed record StabilityAnalysis(
    string NormalizationMethod,
    IReadOnlyList<StabilityRankingEntry> Ranking);

public sealed class IndicatorStabilityAnalyzer
{
    private const double Epsilon = 1e-9;

    public StabilityAnalysis Analyze(
        DrawWindow window,
        IReadOnlyList<StabilityIndicatorRequest> indicators,
        string normalizationMethod,
        int topK,
        int minHistory)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(indicators);

        if (window.Draws.Count < minHistory)
        {
            throw new DomainInvariantViolationException("insufficient history for requested min_history.");
        }

        var entries = new List<StabilityRankingEntry>();
        foreach (var indicator in indicators)
        {
            ArgumentNullException.ThrowIfNull(indicator);

            var scalarSeries = BuildScalarSeries(window, indicator);
            foreach (var series in scalarSeries)
            {
                var dispersion = ComputeDispersion(normalizationMethod, series.Values);
                var stabilityScore = 1d / (1d + dispersion);
                entries.Add(new StabilityRankingEntry(
                    IndicatorName: indicator.Name,
                    Aggregation: series.Aggregation,
                    ComponentIndex: series.ComponentIndex,
                    Shape: series.Shape,
                    Dispersion: dispersion,
                    StabilityScore: stabilityScore,
                    Explanation: $"Estabilidade calculada via {normalizationMethod}."));
            }
        }

        var ordered = entries
            .OrderByDescending(entry => entry.StabilityScore)
            .ThenBy(entry => entry.IndicatorName, StringComparer.Ordinal)
            .ThenBy(entry => entry.ComponentIndex ?? -1)
            .Take(topK)
            .ToArray();

        return new StabilityAnalysis(normalizationMethod, ordered);
    }

    private static IReadOnlyList<ScalarSeries> BuildScalarSeries(
        DrawWindow window,
        StabilityIndicatorRequest indicator)
    {
        var seriesData = indicator.Name switch
        {
            "repeticao_concurso_anterior" => IndicatorSeriesData.FromScalarSeries(
                "series",
                RepeticaoConcursoAnteriorSeries.Build(window)),
            "pares_no_concurso" => IndicatorSeriesData.FromScalarSeries(
                "series",
                window.Draws.Select(CountEvenNumbers).ToArray()),
            "quantidade_vizinhos_por_concurso" => IndicatorSeriesData.FromScalarSeries(
                "series",
                window.Draws.Select(CountNeighborPairs).ToArray()),
            "sequencia_maxima_vizinhos_por_concurso" => IndicatorSeriesData.FromScalarSeries(
                "series",
                window.Draws.Select(ComputeMaxNeighborRun).ToArray()),
            "frequencia_por_dezena" => IndicatorSeriesData.FromVectorSeries(
                "vector_by_dezena",
                window.Draws.Select(draw => ExpandToPresenceVector(draw.Numbers)).ToArray()),
            "distribuicao_linha_por_concurso" => IndicatorSeriesData.FromVectorSeries(
                "series_of_count_vector[5]",
                window.Draws.Select(draw => ComputeRowDistribution(draw.Numbers)).ToArray()),
            _ => throw new DomainInvariantViolationException($"unknown indicator requested: {indicator.Name}.")
        };

        return ReduceSeriesData(seriesData, indicator);
    }

    private static IReadOnlyList<ScalarSeries> ReduceSeriesData(
        IndicatorSeriesData seriesData,
        StabilityIndicatorRequest indicator)
    {
        if (seriesData.ScalarSeries is not null)
        {
            var aggregation = string.IsNullOrWhiteSpace(indicator.Aggregation) ? "identity" : indicator.Aggregation!;
            if (!string.Equals(aggregation, "identity", StringComparison.Ordinal) &&
                !string.Equals(aggregation, "mean", StringComparison.Ordinal))
            {
                throw new DomainInvariantViolationException("unsupported aggregation for scalar indicator.");
            }

            return
            [
                new ScalarSeries(
                    Aggregation: "identity",
                    ComponentIndex: null,
                    Shape: seriesData.Shape,
                    Values: seriesData.ScalarSeries)
            ];
        }

        if (string.IsNullOrWhiteSpace(indicator.Aggregation))
        {
            throw new DomainInvariantViolationException("aggregation is required for non-scalar indicator.");
        }

        var vectorSeries = seriesData.VectorSeries!;
        return indicator.Aggregation switch
        {
            "mean" => [new ScalarSeries("mean", null, seriesData.Shape, vectorSeries.Select(values => values.Average()).ToArray())],
            "max" => [new ScalarSeries("max", null, seriesData.Shape, vectorSeries.Select(values => values.Max()).ToArray())],
            "l2_norm" => [new ScalarSeries("l2_norm", null, seriesData.Shape, vectorSeries.Select(ComputeL2Norm).ToArray())],
            "per_component" => BuildPerComponentSeries(seriesData.Shape, vectorSeries),
            _ => throw new DomainInvariantViolationException("unsupported aggregation for indicator stability.")
        };
    }

    private static ScalarSeries[] BuildPerComponentSeries(string shape, IReadOnlyList<double[]> vectorSeries)
    {
        var dimensions = vectorSeries[0].Length;
        var result = new ScalarSeries[dimensions];

        for (var componentIndex = 0; componentIndex < dimensions; componentIndex++)
        {
            var componentSeries = vectorSeries.Select(values => values[componentIndex]).ToArray();
            result[componentIndex] = new ScalarSeries(
                Aggregation: "per_component",
                ComponentIndex: componentIndex,
                Shape: shape,
                Values: componentSeries);
        }

        return result;
    }

    private static double ComputeDispersion(string normalizationMethod, IReadOnlyList<double> values)
    {
        if (values.Count < 2)
        {
            throw new DomainInvariantViolationException("insufficient history for requested indicator series.");
        }

        return normalizationMethod switch
        {
            "madn" => ComputeMadn(values),
            "coefficient_of_variation" => ComputeCoefficientOfVariation(values),
            _ => throw new DomainInvariantViolationException("unsupported normalization_method.")
        };
    }

    private static double ComputeMadn(IReadOnlyList<double> values)
    {
        var median = ComputeMedian(values);
        var absoluteDeviations = values
            .Select(value => Math.Abs(value - median))
            .ToArray();
        var mad = ComputeMedian(absoluteDeviations);

        if (Math.Abs(median) > Epsilon)
        {
            return mad / Math.Abs(median);
        }

        var sorted = values.OrderBy(value => value).ToArray();
        var q1 = ComputePercentile(sorted, 0.25);
        var q3 = ComputePercentile(sorted, 0.75);
        var iqr = q3 - q1;
        return iqr / Math.Abs(median + Epsilon);
    }

    private static double ComputeCoefficientOfVariation(IReadOnlyList<double> values)
    {
        if (values.Any(value => value <= 0d))
        {
            throw new DomainInvariantViolationException("coefficient_of_variation requires strictly positive series.");
        }

        var mean = values.Average();
        if (mean <= Epsilon)
        {
            throw new DomainInvariantViolationException("coefficient_of_variation requires mean greater than epsilon.");
        }

        var variance = values
            .Select(value => Math.Pow(value - mean, 2d))
            .Sum() / (values.Count - 1);
        var stdDev = Math.Sqrt(variance);
        return stdDev / mean;
    }

    private static int CountEvenNumbers(Draw draw)
    {
        return draw.Numbers.Count(number => number % 2 == 0);
    }

    private static int CountNeighborPairs(Draw draw)
    {
        var count = 0;
        for (var index = 1; index < draw.Numbers.Count; index++)
        {
            if (draw.Numbers[index] - draw.Numbers[index - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }

    private static int ComputeMaxNeighborRun(Draw draw) =>
        VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(draw.Numbers);

    private static double[] ExpandToPresenceVector(IReadOnlyList<int> numbers)
    {
        var values = new double[25];
        foreach (var number in numbers)
        {
            values[number - 1] = 1d;
        }

        return values;
    }

    private static double[] ComputeRowDistribution(IReadOnlyList<int> numbers)
    {
        var values = new double[5];
        foreach (var number in numbers)
        {
            var rowIndex = (number - 1) / 5;
            values[rowIndex]++;
        }

        return values;
    }

    private static double ComputeL2Norm(IReadOnlyList<double> values)
    {
        return Math.Sqrt(values.Sum(value => value * value));
    }

    private static double ComputeMedian(IReadOnlyList<double> values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        return ComputePercentile(sorted, 0.5);
    }

    private static double ComputePercentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0d;
        }

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

    private sealed record ScalarSeries(
        string Aggregation,
        int? ComponentIndex,
        string Shape,
        IReadOnlyList<double> Values);

    private sealed record IndicatorSeriesData(
        string Shape,
        IReadOnlyList<double>? ScalarSeries,
        IReadOnlyList<double[]>? VectorSeries)
    {
        public static IndicatorSeriesData FromScalarSeries(string shape, IReadOnlyList<double> values)
        {
            return new IndicatorSeriesData(shape, values, null);
        }

        public static IndicatorSeriesData FromScalarSeries(string shape, IReadOnlyList<int> values)
        {
            return new IndicatorSeriesData(shape, values.Select(value => (double)value).ToArray(), null);
        }

        public static IndicatorSeriesData FromVectorSeries(string shape, IReadOnlyList<double[]> values)
        {
            if (values.Count == 0)
            {
                throw new DomainInvariantViolationException("vector series cannot be empty.");
            }

            var expectedDimensions = values[0].Length;
            if (values.Any(vector => vector.Length != expectedDimensions))
            {
                throw new DomainInvariantViolationException("vector series must have fixed dimensions.");
            }

            return new IndicatorSeriesData(shape, null, values);
        }
    }
}

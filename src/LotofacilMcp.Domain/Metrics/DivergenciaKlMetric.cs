using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class DivergenciaKlMetric
{
    public WindowMetricValue Compute(DrawWindow baselineWindow, DrawWindow comparisonWindow)
    {
        if (baselineWindow is null || comparisonWindow is null)
        {
            throw new DomainInvariantViolationException("both baseline and comparison windows are required.");
        }

        var p = BuildDistribution(baselineWindow);
        var q = BuildDistribution(comparisonWindow);
        var alpha = 1d / 25d;

        double divergence = 0d;
        for (var i = 0; i < 25; i++)
        {
            var pSmooth = (p[i] + alpha) / (1d + (25d * alpha));
            var qSmooth = (q[i] + alpha) / (1d + (25d * alpha));
            divergence += pSmooth * Math.Log2(pSmooth / qSmooth);
        }

        return new WindowMetricValue(
            MetricName: "divergencia_kl",
            Scope: "window",
            Shape: "scalar",
            Unit: "bits",
            Version: "1.0.0",
            Value: [divergence]);
    }

    private static double[] BuildDistribution(DrawWindow window)
    {
        var counts = new double[25];
        foreach (var draw in window.Draws)
        {
            foreach (var dezena in draw.Numbers)
            {
                counts[dezena - 1]++;
            }
        }

        var total = counts.Sum();
        if (total == 0d)
        {
            throw new DomainInvariantViolationException("window distribution cannot be empty.");
        }

        for (var i = 0; i < counts.Length; i++)
        {
            counts[i] /= total;
        }

        return counts;
    }
}

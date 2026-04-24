using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class AusenciaBlocosMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        return new WindowMetricValue(
            MetricName: "ausencia_blocos",
            Scope: "window",
            Shape: "count_list_by_dezena",
            Unit: "count",
            Version: "1.0.0",
            Value: EncodeBlocks(window));
    }

    private static IReadOnlyList<double> EncodeBlocks(DrawWindow window)
    {
        var encoded = new List<double>(256);
        for (var dezena = 1; dezena <= 25; dezena++)
        {
            var blocks = new List<int>();
            var current = 0;

            foreach (var draw in window.Draws)
            {
                var isAbsent = !draw.Numbers.Contains(dezena);
                if (isAbsent)
                {
                    current++;
                    continue;
                }

                if (current > 0)
                {
                    blocks.Add(current);
                    current = 0;
                }
            }

            if (current > 0)
            {
                blocks.Add(current);
            }

            encoded.Add(dezena);
            encoded.Add(blocks.Count);
            foreach (var block in blocks)
            {
                encoded.Add(block);
            }
        }

        return encoded;
    }
}

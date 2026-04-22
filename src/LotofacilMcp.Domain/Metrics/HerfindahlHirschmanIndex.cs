using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Metrics;

/// <summary>HHI = Σ p_i² with p_i = c_i / sum(c) for non-negative count histograms.</summary>
public static class HerfindahlHirschmanIndex
{
    public static double FromNonNegativeCounts(ReadOnlySpan<int> counts)
    {
        var total = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            var c = counts[i];
            if (c < 0)
            {
                throw new DomainInvariantViolationException("HHI counts must be non-negative.");
            }

            total += c;
        }

        if (total == 0)
        {
            return 0d;
        }

        double hhi = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            var p = (double)counts[i] / total;
            hhi += p * p;
        }

        return hhi;
    }
}

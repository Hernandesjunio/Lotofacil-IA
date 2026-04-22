using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Metrics;

/// <summary>Shannon entropy in bits for a histogram of counts (p_i = c_i / sum c).</summary>
public static class ShannonEntropyBits
{
    public static double FromNonNegativeCounts(ReadOnlySpan<int> counts)
    {
        var total = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            var c = counts[i];
            if (c < 0)
            {
                throw new DomainInvariantViolationException("entropy counts must be non-negative.");
            }

            total += c;
        }

        if (total == 0)
        {
            return 0d;
        }

        double h = 0;
        for (var i = 0; i < counts.Length; i++)
        {
            var c = counts[i];
            if (c == 0)
            {
                continue;
            }

            var p = (double)c / total;
            h -= p * Math.Log2(p);
        }

        return h;
    }
}

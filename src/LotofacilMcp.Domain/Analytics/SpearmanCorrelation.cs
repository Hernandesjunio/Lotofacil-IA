namespace LotofacilMcp.Domain.Analytics;

public static class SpearmanCorrelation
{
    public static double Compute(IReadOnlyList<double> x, IReadOnlyList<double> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        if (x.Count != y.Count)
        {
            throw new InvalidOperationException("series must have the same length for spearman.");
        }

        if (x.Count < 2)
        {
            return 0d;
        }

        var rankX = AverageRanks(x);
        var rankY = AverageRanks(y);
        return Pearson(rankX, rankY);
    }

    private static IReadOnlyList<double> AverageRanks(IReadOnlyList<double> values)
    {
        var n = values.Count;
        var order = new int[n];
        for (var i = 0; i < n; i++)
        {
            order[i] = i;
        }

        Array.Sort(order, (a, b) =>
        {
            var cmp = values[a].CompareTo(values[b]);
            return cmp != 0 ? cmp : a.CompareTo(b);
        });

        var ranks = new double[n];
        var iSort = 0;
        while (iSort < n)
        {
            var j = iSort;
            var val = values[order[iSort]];
            while (j < n && Math.Abs(values[order[j]] - val) < 1e-15)
            {
                j++;
            }

            var averageRank = ((iSort + 1) + (j + 1)) / 2d;
            for (var k = iSort; k < j; k++)
            {
                ranks[order[k]] = averageRank;
            }

            iSort = j;
        }

        return ranks;
    }

    private static double Pearson(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        var n = a.Count;
        if (n == 0)
        {
            return 0d;
        }

        var meanA = 0d;
        var meanB = 0d;
        for (var i = 0; i < n; i++)
        {
            meanA += a[i];
            meanB += b[i];
        }

        meanA /= n;
        meanB /= n;

        var num = 0d;
        var denA = 0d;
        var denB = 0d;
        for (var i = 0; i < n; i++)
        {
            var dA = a[i] - meanA;
            var dB = b[i] - meanB;
            num += dA * dB;
            denA += dA * dA;
            denB += dB * dB;
        }

        var den = Math.Sqrt(denA * denB);
        if (den < 1e-15)
        {
            return 0d;
        }

        return num / den;
    }
}

using LotofacilMcp.Application.Validation;

namespace LotofacilMcp.Application.Composition;

public static class IndicatorTransformFunctions
{
    public const double WeightSumTolerance = 1e-9;

    public static IReadOnlyList<double> Apply(string transform, IReadOnlyList<double> values)
    {
        if (string.IsNullOrWhiteSpace(transform))
        {
            throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "transform is required for each component.",
                details: new Dictionary<string, object?> { ["field"] = "components[].transform" });
        }

        return transform switch
        {
            "normalize_max" => NormalizeMax(values),
            "invert_normalize_max" => InvertNormalizeMax(values),
            "rank_percentile" => RankPercentile(values),
            "identity_unit_interval" => IdentityUnitInterval(values),
            "one_minus_unit_interval" => OneMinusUnitInterval(values),
            "shift_scale_unit_interval" => ShiftScaleUnitInterval(values),
            _ => throw new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "transform is not a supported value.",
                details: new Dictionary<string, object?>
                {
                    ["field"] = "components[].transform",
                    ["transform"] = transform
                })
        };
    }

    private static IReadOnlyList<double> NormalizeMax(IReadOnlyList<double> x)
    {
        var n = x.Count;
        if (n == 0)
        {
            return Array.Empty<double>();
        }

        var max = 0.0;
        for (var i = 0; i < n; i++)
        {
            if (x[i] > max)
            {
                max = x[i];
            }
        }

        if (max == 0)
        {
            return Enumerable.Repeat(0.0, n).ToArray();
        }

        var r = new double[n];
        for (var i = 0; i < n; i++)
        {
            r[i] = x[i] / max;
        }

        return r;
    }

    private static IReadOnlyList<double> InvertNormalizeMax(IReadOnlyList<double> x)
    {
        var n = NormalizeMax(x);
        var r = new double[n.Count];
        for (var i = 0; i < n.Count; i++)
        {
            r[i] = 1.0 - n[i];
        }

        return r;
    }

    private static IReadOnlyList<double> RankPercentile(IReadOnlyList<double> x)
    {
        var n = x.Count;
        if (n == 0)
        {
            return Array.Empty<double>();
        }

        if (n == 1)
        {
            return [0.5];
        }

        var ord = new int[n];
        for (var i = 0; i < n; i++)
        {
            ord[i] = i;
        }

        Array.Sort(ord, (a, b) =>
        {
            var cmp = x[a].CompareTo(x[b]);
            if (cmp != 0)
            {
                return cmp;
            }

            return a.CompareTo(b);
        });

        var rankOfIndex = new int[n];
        for (var r = 0; r < n; r++)
        {
            rankOfIndex[ord[r]] = r;
        }

        var denom = n - 1.0;
        var o = new double[n];
        for (var i = 0; i < n; i++)
        {
            o[i] = rankOfIndex[i] / denom;
        }

        return o;
    }

    private static IReadOnlyList<double> IdentityUnitInterval(IReadOnlyList<double> x)
    {
        var n = x.Count;
        var r = new double[n];
        for (var i = 0; i < n; i++)
        {
            var v = x[i];
            if (v is < 0.0 or > 1.0)
            {
                throw new ApplicationValidationException(
                    code: "INVALID_REQUEST",
                    message: "identity_unit_interval requires all values in [0,1].",
                    details: new Dictionary<string, object?> { ["index"] = i, ["value"] = v });
            }

            r[i] = v;
        }

        return r;
    }

    private static IReadOnlyList<double> OneMinusUnitInterval(IReadOnlyList<double> x)
    {
        _ = IdentityUnitInterval(x);
        var n = x.Count;
        var r = new double[n];
        for (var i = 0; i < n; i++)
        {
            r[i] = 1.0 - x[i];
        }

        return r;
    }

    private static IReadOnlyList<double> ShiftScaleUnitInterval(IReadOnlyList<double> x)
    {
        var n = x.Count;
        if (n == 0)
        {
            return Array.Empty<double>();
        }

        var min = x[0];
        var max = x[0];
        for (var i = 1; i < n; i++)
        {
            if (x[i] < min)
            {
                min = x[i];
            }

            if (x[i] > max)
            {
                max = x[i];
            }
        }

        if (min == max)
        {
            return Enumerable.Repeat(0.5, n).ToArray();
        }

        var span = max - min;
        var r = new double[n];
        for (var i = 0; i < n; i++)
        {
            r[i] = (x[i] - min) / span;
        }

        return r;
    }
}

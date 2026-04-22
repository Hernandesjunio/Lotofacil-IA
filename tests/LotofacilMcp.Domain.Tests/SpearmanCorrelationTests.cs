using LotofacilMcp.Domain.Analytics;

namespace LotofacilMcp.Domain.Tests;

public sealed class SpearmanCorrelationTests
{
    [Fact]
    public void PerfectNegativeMonotonic_IsMinusOne()
    {
        var x = new double[] { 1, 2, 3, 4, 5 };
        var y = new double[] { 5, 4, 3, 2, 1 };
        var rho = SpearmanCorrelation.Compute(x, y);
        Assert.Equal(-1.0, rho, precision: 12);
    }

    [Fact]
    public void IdenticalSeries_IsOne()
    {
        var v = new double[] { 2, 7, 1, 9, 3 };
        var rho = SpearmanCorrelation.Compute(v, v);
        Assert.Equal(1.0, rho, precision: 12);
    }
}

using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Tests;

public sealed class TypicalRangeResolverTests
{
    [Fact]
    public void Resolve_Iqr_UsesDeterministicQuartilesAndReportsCoverageObserved()
    {
        var resolver = new TypicalRangeResolver();
        var spec = new TypicalRangeSpec(
            MetricName: "repeticao_concurso_anterior",
            Method: "iqr",
            Coverage: 0.8d,
            Params: null,
            WindowRef: null,
            Inclusive: null);
        var series = new[] { 1d, 2d, 3d, 4d, 100d };

        var result = resolver.Resolve(spec, series);

        Assert.Equal(2d, result.ResolvedRange.Min);
        Assert.Equal(4d, result.ResolvedRange.Max);
        Assert.True(result.ResolvedRange.Inclusive);
        Assert.Equal(0.6d, result.CoverageObserved, 10);
        Assert.Equal("1.0.0", result.MethodVersion);
    }

    [Fact]
    public void Resolve_Percentile_UsesDeclaredParamsAndIsStableForSameInput()
    {
        var resolver = new TypicalRangeResolver();
        var spec = new TypicalRangeSpec(
            MetricName: "quantidade_vizinhos_por_concurso",
            Method: "percentile",
            Coverage: 0.8d,
            Params: new TypicalRangePercentileParams(0.1d, 0.9d),
            WindowRef: null,
            Inclusive: true);
        var series = new[] { 1d, 2d, 3d, 4d, 100d };

        var first = resolver.Resolve(spec, series);
        var second = resolver.Resolve(spec, series);

        Assert.Equal(first, second);
        Assert.Equal(1.4d, first.ResolvedRange.Min, 10);
        Assert.Equal(61.6d, first.ResolvedRange.Max, 10);
        Assert.Equal(0.6d, first.CoverageObserved, 10);
    }

    [Theory]
    [InlineData(-0.01d)]
    [InlineData(1.01d)]
    public void Resolve_WithCoverageOutsideRange_ThrowsDomainInvariantViolation(double coverage)
    {
        var resolver = new TypicalRangeResolver();
        var spec = new TypicalRangeSpec(
            MetricName: "repeticao_concurso_anterior",
            Method: "iqr",
            Coverage: coverage,
            Params: null,
            WindowRef: null,
            Inclusive: true);

        Assert.Throws<DomainInvariantViolationException>(() => resolver.Resolve(spec, new[] { 1d, 2d, 3d }));
    }

    [Fact]
    public void Resolve_PercentileWithInvalidParams_ThrowsDomainInvariantViolation()
    {
        var resolver = new TypicalRangeResolver();
        var spec = new TypicalRangeSpec(
            MetricName: "repeticao_concurso_anterior",
            Method: "percentile",
            Coverage: 0.5d,
            Params: new TypicalRangePercentileParams(0.9d, 0.1d),
            WindowRef: null,
            Inclusive: true);

        Assert.Throws<DomainInvariantViolationException>(() => resolver.Resolve(spec, new[] { 1d, 2d, 3d }));
    }
}

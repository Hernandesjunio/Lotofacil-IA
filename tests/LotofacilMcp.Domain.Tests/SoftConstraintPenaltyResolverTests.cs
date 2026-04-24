using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Tests;

public sealed class SoftConstraintPenaltyResolverTests
{
    [Theory]
    [InlineData("max_neighbor_count", true)]
    [InlineData("max_consecutive_run", true)]
    [InlineData("repeat_range", false)]
    public void Supports_ReturnsExpectedResult(string constraintName, bool expected)
    {
        var resolver = new SoftConstraintPenaltyResolver();

        Assert.Equal(expected, resolver.Supports(constraintName));
    }

    [Fact]
    public void Resolve_UsesDeterministicVersionedLinearPenalty()
    {
        var resolver = new SoftConstraintPenaltyResolver();

        var penalty = resolver.Resolve("max_neighbor_count", violationDistance: 2d);

        Assert.Equal("max_neighbor_count", penalty.ConstraintName);
        Assert.Equal(2d, penalty.ViolationDistance);
        Assert.Equal(14d, penalty.Scale);
        Assert.Equal(2d / 14d, penalty.Penalty, 10);
        Assert.Equal(SoftConstraintPenaltyResolver.PenaltyVersion, penalty.Version);
    }

    [Fact]
    public void Resolve_ClampsPenaltyToOne()
    {
        var resolver = new SoftConstraintPenaltyResolver();

        var penalty = resolver.Resolve("max_consecutive_run", violationDistance: 999d);

        Assert.Equal(1d, penalty.Penalty);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(-1d)]
    public void Resolve_WithInvalidViolationDistance_ThrowsDomainInvariantViolation(double distance)
    {
        var resolver = new SoftConstraintPenaltyResolver();

        Assert.Throws<DomainInvariantViolationException>(() => resolver.Resolve("max_neighbor_count", distance));
    }
}

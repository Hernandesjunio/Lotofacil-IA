using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Tests;

public sealed class GenerationBudgetResolverTests
{
    [Fact]
    public void Resolve_WithExplicitMaxAttempts_PreservesDeterministicBudget()
    {
        var resolver = new GenerationBudgetResolver();

        var resolved = resolver.Resolve(
            new GenerationBudgetSpec(MaxAttempts: 500, PoolMultiplier: 3.5d),
            basePoolSize: 140);

        Assert.Equal(500, resolved.MaxAttempts);
        Assert.Equal(3.5d, resolved.PoolMultiplier);
    }

    [Fact]
    public void Resolve_WithoutMaxAttempts_UsesPoolMultiplierAgainstBasePool()
    {
        var resolver = new GenerationBudgetResolver();

        var resolved = resolver.Resolve(
            new GenerationBudgetSpec(MaxAttempts: null, PoolMultiplier: 2.25d),
            basePoolSize: 140);

        Assert.Equal(315, resolved.MaxAttempts);
        Assert.Equal(2.25d, resolved.PoolMultiplier);
    }

    [Fact]
    public void Resolve_DefaultBudget_UsesDeterministicMultiplierOne()
    {
        var resolver = new GenerationBudgetResolver();

        var resolved = resolver.Resolve(null, basePoolSize: 180);

        Assert.Equal(180, resolved.MaxAttempts);
        Assert.Equal(1d, resolved.PoolMultiplier);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Resolve_WithInvalidMaxAttempts_ThrowsDomainInvariantViolation(int maxAttempts)
    {
        var resolver = new GenerationBudgetResolver();

        Assert.Throws<DomainInvariantViolationException>(() =>
            resolver.Resolve(new GenerationBudgetSpec(MaxAttempts: maxAttempts, PoolMultiplier: 1d), basePoolSize: 100));
    }
}

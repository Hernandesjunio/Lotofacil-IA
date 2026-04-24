using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Generation;

public sealed record GenerationBudgetSpec(
    int? MaxAttempts,
    double? PoolMultiplier);

public sealed record ResolvedGenerationBudget(
    int MaxAttempts,
    double PoolMultiplier);

public sealed class GenerationBudgetResolver
{
    public ResolvedGenerationBudget Resolve(GenerationBudgetSpec? budget, int basePoolSize)
    {
        if (basePoolSize <= 0)
        {
            throw new DomainInvariantViolationException("generation budget base pool size must be greater than zero.");
        }

        var multiplier = budget?.PoolMultiplier ?? 1d;
        if (!double.IsFinite(multiplier) || multiplier <= 0d)
        {
            throw new DomainInvariantViolationException("generation_budget.pool_multiplier must be a finite number greater than zero.");
        }

        int maxAttempts;
        if (budget?.MaxAttempts is int explicitMaxAttempts)
        {
            if (explicitMaxAttempts <= 0)
            {
                throw new DomainInvariantViolationException("generation_budget.max_attempts must be greater than zero.");
            }

            maxAttempts = explicitMaxAttempts;
        }
        else
        {
            maxAttempts = (int)Math.Ceiling(basePoolSize * multiplier);
        }

        return new ResolvedGenerationBudget(
            MaxAttempts: maxAttempts,
            PoolMultiplier: multiplier);
    }
}

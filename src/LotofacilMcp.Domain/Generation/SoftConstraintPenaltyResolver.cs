using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Generation;

public sealed record SoftConstraintPenalty(
    string ConstraintName,
    double ViolationDistance,
    double Scale,
    double Penalty,
    string Version);

public sealed class SoftConstraintPenaltyResolver
{
    public const string PenaltyVersion = "1.0.0";

    public bool Supports(string constraintName)
    {
        return string.Equals(constraintName, "max_neighbor_count", StringComparison.Ordinal) ||
               string.Equals(constraintName, "max_consecutive_run", StringComparison.Ordinal);
    }

    public double ResolveScale(string constraintName)
    {
        return constraintName switch
        {
            "max_neighbor_count" => 14d,
            "max_consecutive_run" => 15d,
            _ => throw new DomainInvariantViolationException($"mode=soft is not supported for constraint '{constraintName}'.")
        };
    }

    public SoftConstraintPenalty Resolve(string constraintName, double violationDistance)
    {
        if (!double.IsFinite(violationDistance) || violationDistance < 0d)
        {
            throw new DomainInvariantViolationException("soft penalty violation distance must be a finite number >= 0.");
        }

        var scale = ResolveScale(constraintName);
        var normalized = Math.Min(1d, violationDistance / scale);
        return new SoftConstraintPenalty(
            ConstraintName: constraintName,
            ViolationDistance: violationDistance,
            Scale: scale,
            Penalty: normalized,
            Version: PenaltyVersion);
    }
}

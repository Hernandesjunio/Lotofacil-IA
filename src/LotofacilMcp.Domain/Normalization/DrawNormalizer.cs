using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Normalization;

public sealed class DrawNormalizer
{
    public Draw Normalize(RawDraw rawDraw)
    {
        if (rawDraw is null)
        {
            throw new DomainInvariantViolationException("raw draw cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(rawDraw.DrawDate))
        {
            throw new DomainInvariantViolationException("draw_date cannot be null or empty.");
        }

        if (!DateOnly.TryParse(rawDraw.DrawDate, out var parsedDate))
        {
            throw new DomainInvariantViolationException("draw_date must be a valid date.");
        }

        if (rawDraw.Numbers is null)
        {
            throw new DomainInvariantViolationException("numbers cannot be null.");
        }

        var canonicalNumbers = rawDraw.Numbers
            .OrderBy(value => value)
            .ToArray();

        return new Draw(
            rawDraw.ContestId,
            parsedDate,
            canonicalNumbers);
    }
}

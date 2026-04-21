using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Windows;

public sealed class WindowResolver
{
    public DrawWindow Resolve(
        IReadOnlyList<Draw> orderedDraws,
        int windowSize,
        int? endContestId)
    {
        if (orderedDraws is null)
        {
            throw new DomainInvariantViolationException("draw collection cannot be null.");
        }

        if (windowSize <= 0)
        {
            throw new DomainInvariantViolationException("window_size must be positive.");
        }

        var draws = orderedDraws
            .OrderBy(draw => draw.ContestId)
            .ToArray();

        if (draws.Length == 0)
        {
            throw new DomainInvariantViolationException("cannot resolve window from empty draw collection.");
        }

        var effectiveEndContestId = endContestId ?? draws[^1].ContestId;
        var endIndex = Array.FindIndex(draws, draw => draw.ContestId == effectiveEndContestId);
        if (endIndex < 0)
        {
            throw new DomainInvariantViolationException("requested end_contest_id was not found.");
        }

        var startIndex = endIndex - windowSize + 1;
        if (startIndex < 0)
        {
            throw new DomainInvariantViolationException("insufficient history for requested window.");
        }

        var windowDraws = draws
            .Skip(startIndex)
            .Take(windowSize)
            .ToArray();

        return new DrawWindow(
            Size: windowSize,
            StartContestId: windowDraws[0].ContestId,
            EndContestId: windowDraws[^1].ContestId,
            Draws: windowDraws);
    }
}

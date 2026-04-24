using LotofacilMcp.Application.Validation;

namespace LotofacilMcp.Application.Windows;

/// <summary>
/// Resolves a single auditable <c>window_size</c> + <c>end_contest_id</c> from the request
/// (ADR 0008 D2), rejecting ambiguous or incomplete combinations.
/// </summary>
public static class WindowRequestResolver
{
    public static (int WindowSize, int? EndContestId) Resolve(
        int? windowSize,
        int? startContestId,
        int? endContestId)
    {
        if (startContestId.HasValue && !endContestId.HasValue)
        {
            throw new ApplicationValidationException(
                "INVALID_REQUEST",
                "end_contest_id is required when start_contest_id is provided.",
                new Dictionary<string, object?>
                {
                    ["field"] = "end_contest_id"
                });
        }

        if (startContestId.HasValue && endContestId.HasValue)
        {
            return ResolveFromInclusiveExtremes(windowSize, startContestId.Value, endContestId.Value);
        }

        if (windowSize is null or <= 0)
        {
            throw new ApplicationValidationException(
                "INVALID_WINDOW_SIZE",
                "window_size must be greater than zero.",
                new Dictionary<string, object?>
                {
                    ["window_size"] = windowSize
                });
        }

        return (windowSize.Value, endContestId);
    }

    private static (int WindowSize, int? EndContestId) ResolveFromInclusiveExtremes(
        int? windowSize,
        int startContestId,
        int endContestId)
    {
        if (startContestId > endContestId)
        {
            throw new ApplicationValidationException(
                "INVALID_REQUEST",
                "start_contest_id must be less than or equal to end_contest_id.",
                new Dictionary<string, object?>
                {
                    ["start_contest_id"] = startContestId,
                    ["end_contest_id"] = endContestId
                });
        }

        var implied = endContestId - startContestId + 1;

        if (windowSize is null || windowSize == 0)
        {
            return (implied, endContestId);
        }

        if (windowSize != implied)
        {
            throw new ApplicationValidationException(
                "INVALID_REQUEST",
                "window_size is incompatible with start_contest_id and end_contest_id.",
                new Dictionary<string, object?>
                {
                    ["window_size"] = windowSize,
                    ["start_contest_id"] = startContestId,
                    ["end_contest_id"] = endContestId
                });
        }

        return (windowSize.Value, endContestId);
    }
}

using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Windows;

public sealed class WindowResolver
{
    public DrawWindow Resolve(
        IReadOnlyList<Draw> orderedDraws,
        int windowSize,
        int? endContestId)
    {
        throw new NotImplementedException("V0 window resolution is implemented in Phase 3.");
    }
}

using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Windows;

public sealed record DrawWindow(
    int Size,
    int StartContestId,
    int EndContestId,
    IReadOnlyList<Draw> Draws);

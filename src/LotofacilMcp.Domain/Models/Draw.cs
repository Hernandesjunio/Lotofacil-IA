namespace LotofacilMcp.Domain.Models;

public sealed record RawDraw(
    int ContestId,
    string DrawDate,
    IReadOnlyList<int> Numbers);

public sealed record Draw(
    int ContestId,
    DateOnly DrawDate,
    IReadOnlyList<int> Numbers);

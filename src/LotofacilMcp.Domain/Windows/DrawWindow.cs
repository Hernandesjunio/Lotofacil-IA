using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Domain.Windows;

public sealed record DrawWindow
{
    public DrawWindow(
        int Size,
        int StartContestId,
        int EndContestId,
        IReadOnlyList<Draw> Draws)
    {
        if (Size <= 0)
        {
            throw new DomainInvariantViolationException("window size must be positive.");
        }

        if (Draws is null)
        {
            throw new DomainInvariantViolationException("window draws cannot be null.");
        }

        if (Draws.Count != Size)
        {
            throw new DomainInvariantViolationException("window draws count must match size.");
        }

        if (StartContestId > EndContestId)
        {
            throw new DomainInvariantViolationException("start_contest_id cannot be greater than end_contest_id.");
        }

        if (Draws[0].ContestId != StartContestId || Draws[^1].ContestId != EndContestId)
        {
            throw new DomainInvariantViolationException("window boundaries must match first and last draw contest_id.");
        }

        this.Size = Size;
        this.StartContestId = StartContestId;
        this.EndContestId = EndContestId;
        this.Draws = Draws.ToArray();
    }

    public int Size { get; }

    public int StartContestId { get; }

    public int EndContestId { get; }

    public IReadOnlyList<Draw> Draws { get; }
}

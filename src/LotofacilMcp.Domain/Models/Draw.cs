namespace LotofacilMcp.Domain.Models;

public sealed record RawDraw(
    int ContestId,
    string DrawDate,
    IReadOnlyList<int> Numbers);

public sealed record Draw
{
    public Draw(int contestId, DateOnly drawDate, IReadOnlyList<int> numbers)
    {
        if (contestId <= 0)
        {
            throw new DomainInvariantViolationException("contest_id must be positive.");
        }

        ContestId = contestId;
        DrawDate = drawDate;
        Numbers = ValidateCanonicalNumbers(numbers);
    }

    public int ContestId { get; }

    public DateOnly DrawDate { get; }

    public IReadOnlyList<int> Numbers { get; }

    private static IReadOnlyList<int> ValidateCanonicalNumbers(IReadOnlyList<int> numbers)
    {
        if (numbers is null)
        {
            throw new DomainInvariantViolationException("numbers cannot be null.");
        }

        if (numbers.Count != 15)
        {
            throw new DomainInvariantViolationException("numbers must contain exactly 15 dezenas.");
        }

        var seen = new HashSet<int>();
        var previous = 0;

        for (var index = 0; index < numbers.Count; index++)
        {
            var value = numbers[index];
            if (value is < 1 or > 25)
            {
                throw new DomainInvariantViolationException("numbers must be within [1, 25].");
            }

            if (!seen.Add(value))
            {
                throw new DomainInvariantViolationException("numbers must be unique.");
            }

            if (index > 0 && value <= previous)
            {
                throw new DomainInvariantViolationException("numbers must be strictly increasing.");
            }

            previous = value;
        }

        return numbers.ToArray();
    }
}

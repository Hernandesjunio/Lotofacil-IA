namespace LotofacilMcp.Domain.Models;

public sealed class DomainInvariantViolationException : Exception
{
    public DomainInvariantViolationException(string message)
        : base(message)
    {
    }
}

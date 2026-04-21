namespace LotofacilMcp.Application.Validation;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?> details)
        : base(message)
    {
        Code = code;
        Details = details;
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Details { get; }
}

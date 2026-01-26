using FluentResults;

namespace TestApi.Reasons;

public sealed class ValidationError : Error
{
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

    public ValidationError(
        IDictionary<string, string[]> errors,
        string message = "One or more validation errors occurred.")
        : base(message)
    {
        Errors = errors;
    }
}


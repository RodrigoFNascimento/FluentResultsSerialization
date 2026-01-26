using FluentResults;

namespace TestApi.Reasons;

public sealed class NotFoundError : Error
{
    public NotFoundError(string resource)
        : base($"{resource} was not found")
    {
    }
}

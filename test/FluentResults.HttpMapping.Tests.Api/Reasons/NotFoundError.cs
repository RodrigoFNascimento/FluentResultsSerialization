using FluentResults;

namespace FluentResults.HttpMapping.Tests.Api.Reasons;

public sealed class NotFoundError : Error
{
    public NotFoundError(string resource)
        : base($"{resource} was not found")
    {
    }
}

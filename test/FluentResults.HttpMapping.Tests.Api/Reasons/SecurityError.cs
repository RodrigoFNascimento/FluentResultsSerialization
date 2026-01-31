using FluentResults;

namespace FluentResults.HttpMapping.Tests.Api.Reasons;

public class SecurityError(string message) : Error(message)
{
}

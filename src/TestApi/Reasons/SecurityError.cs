using FluentResults;

namespace TestApi.Reasons;

public class SecurityError(string message) : Error(message)
{
}

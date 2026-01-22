using FluentResults.HttpMapping.Context;
using System.Net;

internal sealed class ProblemRuleDefinition
{
    public HttpStatusCode Status { get; init; }
    public Func<HttpResultMappingContext, string?>? Title { get; init; }
    public Func<HttpResultMappingContext, string?>? Detail { get; init; }
    public IReadOnlyList<HeaderDescriptor> Headers { get; init; }
}

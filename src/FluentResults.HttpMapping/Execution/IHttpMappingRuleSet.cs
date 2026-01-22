using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Represents a collection of HTTP mapping rules
/// that can execute a mapping operation for a result.
/// </summary>
public interface IHttpMappingRuleSet
{
    /// <summary>
    /// Executes the rule set against the provided mapping context.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>The mapped HTTP result.</returns>
    IResult Execute(HttpResultMappingContext context);
}

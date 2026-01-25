using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Represents an ordered collection of HTTP mapping rules
/// capable of mapping a FluentResults result to an ASP.NET
/// <see cref="IResult"/>.
/// </summary>
/// <remarks>
/// Implementations are responsible for evaluating rules in order,
/// selecting the first matching rule, and producing the corresponding
/// HTTP response. If no rule matches, execution is expected to fail
/// with an exception, as this indicates a configuration error.
/// </remarks>
public interface IHttpMappingRuleSet
{
    /// <summary>
    /// Executes the rule set against the provided mapping context.
    /// </summary>
    /// <param name="context">
    /// The mapping context created from a FluentResults result.
    /// </param>
    /// <returns>
    /// The mapped ASP.NET <see cref="IResult"/> representing the HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no rule in the set matches the provided context.
    /// </exception>
    IResult Execute(HttpResultMappingContext context);
}

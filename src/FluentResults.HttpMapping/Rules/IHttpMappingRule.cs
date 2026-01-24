using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// Represents a single HTTP mapping rule.
/// 
/// A rule decides whether it applies to a given
/// <see cref="HttpResultMappingContext"/> and, if so,
/// produces an ASP.NET <see cref="IResult"/>.
/// 
/// Rules must be:
/// - Pure (no side effects)
/// - Order-dependent (first match wins)
/// - Free of dependency injection
/// </summary>
public interface IHttpMappingRule
{
    /// <summary>
    /// Headers produced by this rule.
    /// </summary>
    IReadOnlyList<HeaderDescriptor> Headers { get; }

    /// <summary>
    /// The name of this rule.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Determines whether this rule applies to the given mapping context.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>
    /// <c>true</c> if the rule should be applied; otherwise, <c>false</c>.
    /// </returns>
    bool Matches(HttpResultMappingContext context);

    /// <summary>
    /// Maps the given context to an HTTP result.
    /// 
    /// This method is only called if <see cref="Matches"/> returned <c>true</c>.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>An ASP.NET HTTP result.</returns>
    IResult Map(HttpResultMappingContext context);
}

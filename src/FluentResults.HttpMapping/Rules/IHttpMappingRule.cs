using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// Represents a single HTTP mapping rule.
/// </summary>
/// <remarks>
/// An HTTP mapping rule determines whether it applies to a given
/// <see cref="HttpResultMappingContext"/> and, if so, produces
/// an ASP.NET <see cref="IResult"/>.
///
/// Rules are evaluated in order, and the first matching rule
/// is used to generate the response.
///
/// Implementations of this interface must adhere to the following constraints:
/// <list type="bullet">
/// <item>
/// <description>
/// <b>Pure</b>: Rules must not produce side effects or depend on ambient state.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>Order-dependent</b>: Rule precedence is determined by registration order.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>DI-free</b>: Rules must not rely on dependency injection.
/// </description>
/// </item>
/// </list>
/// </remarks>
public interface IHttpMappingRule
{
    /// <summary>
    /// Gets the headers produced by this rule.
    /// </summary>
    /// <remarks>
    /// Headers are applied to the HTTP response after the rule
    /// has produced its <see cref="IResult"/>.
    /// </remarks>
    IReadOnlyList<HeaderDescriptor> Headers { get; }

    /// <summary>
    /// Gets the optional name of the rule.
    /// </summary>
    /// <remarks>
    /// Rule names are intended for diagnostics, debugging,
    /// and observability purposes only.
    /// They do not affect rule matching or execution.
    /// </remarks>
    string? Name { get; }

    /// <summary>
    /// Determines whether this rule applies to the given mapping context.
    /// </summary>
    /// <param name="context">The mapping context to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the rule should be applied; otherwise, <c>false</c>.
    /// </returns>
    bool Matches(HttpResultMappingContext context);

    /// <summary>
    /// Maps the given context to an HTTP result.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>An ASP.NET HTTP result.</returns>
    /// <remarks>
    /// This method is only invoked when <see cref="Matches"/>
    /// has returned <c>true</c> for the same context.
    /// </remarks>
    IResult Map(HttpResultMappingContext context);
}

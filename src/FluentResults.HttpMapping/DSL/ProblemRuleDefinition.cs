using FluentResults.HttpMapping.Context;
using System.Net;
using System.Security.Cryptography;

/// <summary>
/// Internal immutable definition of an RFC 7807 problem mapping rule.
///
/// <para>
/// This type represents the fully materialized configuration produced by
/// <see cref="DSL.ProblemRuleBuilder"/>. It contains only resolved delegates
/// and values required at execution time.
/// </para>
///
/// <para>
/// Instances of this type are consumed by HTTP mapping rules and are not
/// exposed as part of the public API surface.
/// </para>
/// </summary>
internal sealed class ProblemRuleDefinition
{
    /// <summary>
    /// The HTTP status code to be used for the problem response.
    /// </summary>
    public HttpStatusCode Status { get; init; }

    /// <summary>
    /// A delegate that produces the problem title based on the current
    /// <see cref="HttpResultMappingContext"/>, or <c>null</c> if no title
    /// should be included.
    /// </summary>
    public Func<HttpResultMappingContext, string?>? Title { get; init; }

    /// <summary>
    /// A delegate that produces the problem detail based on the current
    /// <see cref="HttpResultMappingContext"/>, or <c>null</c> if no detail
    /// should be included.
    /// </summary>
    public Func<HttpResultMappingContext, string?>? Detail { get; init; }

    /// <summary>
    /// The collection of Problem Details extension members to be included
    /// in the response.
    /// </summary>
    public IReadOnlyList<ProblemExtensionDescriptor> Extensions { get; init; }
}

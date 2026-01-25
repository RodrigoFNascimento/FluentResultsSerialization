using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// HTTP mapping rule that converts failed FluentResults into
/// RFC 7807 Problem Details responses.
/// </summary>
/// <remarks>
/// This rule is typically produced by the DSL through
/// <see cref="FluentResults.HttpMapping.DSL.RuleBuilder.Problem"/>,
/// and is intended to handle failure cases in a standardized,
/// interoperable way.
///
/// The rule:
/// <list type="bullet">
/// <item>
/// <description>
/// Evaluates a predicate against the mapping context
/// to determine applicability.
/// </description>
/// </item>
/// <item>
/// <description>
/// Produces an ASP.NET <see cref="IResult"/> using
/// <c>Results.Problem</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// Lazily evaluates title, detail, and extensions using
/// the current <see cref="HttpResultMappingContext"/>.
/// </description>
/// </item>
/// </list>
///
/// This rule does not enforce failure-only usage by itself;
/// correctness is guaranteed by how the rule is constructed
/// via the DSL.
/// </remarks>
internal sealed class ProblemHttpMappingRule : IHttpMappingRule
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly HttpStatusCode _status;
    private readonly Func<HttpResultMappingContext, string?>? _title;
    private readonly Func<HttpResultMappingContext, string?>? _detail;
    private readonly IReadOnlyList<HeaderDescriptor> _headers;
    private readonly IReadOnlyList<ProblemExtensionDescriptor> _extensions;

    /// <summary>
    /// Gets the optional name of the rule.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new <see cref="ProblemHttpMappingRule"/>.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used to determine whether the rule applies
    /// to a given mapping context.
    /// </param>
    /// <param name="status">
    /// HTTP status code to use for the Problem Details response.
    /// </param>
    /// <param name="title">
    /// Optional factory used to compute the problem title
    /// from the mapping context.
    /// </param>
    /// <param name="detail">
    /// Optional factory used to compute the problem detail
    /// from the mapping context.
    /// </param>
    /// <param name="headers">
    /// Headers to be applied to the HTTP response.
    /// </param>
    /// <param name="extensions">
    /// Extension members to be included in the Problem Details
    /// response.
    /// </param>
    /// <param name="name">
    /// Optional rule name, used for diagnostics and observability.
    /// </param>
    public ProblemHttpMappingRule(
        Func<HttpResultMappingContext, bool> predicate,
        HttpStatusCode status,
        Func<HttpResultMappingContext, string?>? title,
        Func<HttpResultMappingContext, string?>? detail,
        IReadOnlyList<HeaderDescriptor> headers,
        IReadOnlyList<ProblemExtensionDescriptor> extensions,
        string? name)
    {
        _predicate = predicate;
        _status = status;
        _title = title;
        _detail = detail;
        _headers = headers;
        _extensions = extensions;
        Name = name;
    }

    /// <summary>
    /// Determines whether this rule applies to the given mapping context.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>
    /// <c>true</c> if the predicate evaluates to <c>true</c>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Matches(HttpResultMappingContext context)
        => _predicate(context);

    /// <summary>
    /// Maps the given context to an RFC 7807 Problem Details response.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>An ASP.NET <see cref="IResult"/> representing the problem.</returns>
    /// <remarks>
    /// Title, detail, and extensions are evaluated lazily
    /// at execution time using the provided context.
    /// </remarks>
    public IResult Map(HttpResultMappingContext context)
    {
        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: (int)_status,
            title: _title?.Invoke(context),
            detail: _detail?.Invoke(context),
            extensions: BuildExtensions(context)
        );
    }

    /// <summary>
    /// Gets the headers produced by this rule.
    /// </summary>
    public IReadOnlyList<HeaderDescriptor> Headers => _headers;

    private IDictionary<string, object?>? BuildExtensions(
        HttpResultMappingContext context)
    {
        if (_extensions.Count == 0)
            return null;

        var dict = new Dictionary<string, object?>();

        foreach (var ext in _extensions)
            dict[ext.Name] = ext.ValueFactory(context);

        return dict;
    }
}

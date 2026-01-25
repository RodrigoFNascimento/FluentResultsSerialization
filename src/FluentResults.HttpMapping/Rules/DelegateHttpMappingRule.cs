using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// HTTP mapping rule implemented using delegate functions.
/// </summary>
/// <remarks>
/// This rule type represents the most general form of an HTTP mapping rule.
/// It evaluates a predicate against a <see cref="HttpResultMappingContext"/>
/// to determine whether it applies, and if so, uses a delegate to produce
/// the corresponding <see cref="IResult"/>.
///
/// Instances of this rule are typically created by the DSL layer
/// (<see cref="FluentResults.HttpMapping.DSL.RuleBuilder"/>).
/// </remarks>
internal sealed class DelegateHttpMappingRule : IHttpMappingRule
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly Func<HttpResultMappingContext, IResult> _mapper;
    private readonly IReadOnlyList<HeaderDescriptor> _headers;

    /// <summary>
    /// Gets the optional name of the rule.
    /// </summary>
    /// <remarks>
    /// Rule names are intended for diagnostics and debugging purposes only.
    /// They do not affect rule execution or precedence.
    /// </remarks>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new <see cref="DelegateHttpMappingRule"/>.
    /// </summary>
    /// <param name="predicate">
    /// A function that determines whether this rule matches
    /// the given mapping context.
    /// </param>
    /// <param name="mapper">
    /// A function that maps the context to an HTTP result
    /// when the rule matches.
    /// </param>
    /// <param name="headers">
    /// The headers to apply to the HTTP response produced by this rule.
    /// </param>
    /// <param name="name">
    /// An optional name used to identify the rule.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/>, <paramref name="mapper"/>,
    /// or <paramref name="headers"/> is <c>null</c>.
    /// </exception>
    public DelegateHttpMappingRule(
        Func<HttpResultMappingContext, bool> predicate,
        Func<HttpResultMappingContext, IResult> mapper,
        IReadOnlyList<HeaderDescriptor> headers,
        string? name)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Name = name;
    }

    /// <summary>
    /// Determines whether this rule matches the given mapping context.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>
    /// <c>true</c> if the rule applies to the context; otherwise, <c>false</c>.
    /// </returns>
    public bool Matches(HttpResultMappingContext context)
        => _predicate(context);

    /// <summary>
    /// Maps the given context to an HTTP result.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>
    /// The HTTP result produced by this rule.
    /// </returns>
    /// <remarks>
    /// This method is only called when <see cref="Matches"/> has
    /// returned <c>true</c> for the same context.
    /// </remarks>
    public IResult Map(HttpResultMappingContext context)
        => _mapper(context);

    /// <summary>
    /// Gets the headers to be applied to the HTTP response
    /// produced by this rule.
    /// </summary>
    public IReadOnlyList<HeaderDescriptor> Headers => _headers;
}

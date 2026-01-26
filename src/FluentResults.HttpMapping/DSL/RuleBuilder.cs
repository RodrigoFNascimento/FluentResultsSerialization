using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// Builds a single HTTP mapping rule.
///
/// A <see cref="RuleBuilder"/> is created as part of the HTTP mapping DSL
/// after a matching predicate has been defined (for example, via
/// <c>WhenSuccess</c>, <c>WhenFailure</c>, or <c>WhenError&lt;T&gt;</c>).
///
/// The builder allows configuring:
/// <list type="bullet">
/// <item><description>The rule name (for diagnostics and observability)</description></item>
/// <item><description>Response headers applied when the rule matches</description></item>
/// <item><description>The final HTTP response mapping logic</description></item>
/// </list>
///
/// A rule is only finalized and registered once either
/// <see cref="Map(Func{HttpResultMappingContext, IResult})"/> or
/// <see cref="Problem(Action{ProblemRuleBuilder})"/> is called.
/// </summary>
public sealed class RuleBuilder
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly Action<IHttpMappingRule> _commit;
    private readonly List<HeaderDescriptor> _headers = new();
    private string? _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleBuilder"/> class.
    /// </summary>
    /// <param name="predicate">
    /// A predicate that determines whether this rule applies to a given
    /// <see cref="HttpResultMappingContext"/>.
    /// </param>
    /// <param name="commit">
    /// A callback used internally to register the fully constructed
    /// <see cref="IHttpMappingRule"/> into the mapping pipeline.
    /// </param>
    internal RuleBuilder(
        Func<HttpResultMappingContext, bool> predicate,
        Action<IHttpMappingRule> commit)
    {
        _predicate = predicate;
        _commit = commit;
    }

    /// <summary>
    /// Defines how a matched result is mapped to an HTTP response.
    ///
    /// Calling this method finalizes the rule and registers it into the
    /// HTTP mapping rule set.
    /// </summary>
    /// <param name="mapper">
    /// A function that converts the matched
    /// <see cref="HttpResultMappingContext"/> into an ASP.NET
    /// <see cref="IResult"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mapper"/> is <c>null</c>.
    /// </exception>
    public void Map(Func<HttpResultMappingContext, IResult> mapper)
    {
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        var rule = new DelegateHttpMappingRule(
            _predicate,
            mapper,
            _headers.ToList(),
            _name
        );

        _commit(rule);
    }

    /// <summary>
    /// Assigns a human-readable name to the rule.
    ///
    /// Rule names are intended for diagnostics, debugging, and
    /// observability purposes (for example, identifying which rule
    /// produced a response).
    /// </summary>
    /// <param name="name">
    /// The rule name. This value is not required to be unique.
    /// </param>
    /// <returns>
    /// The current <see cref="RuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public RuleBuilder Named(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Maps a matched failure result to an RFC 7807
    /// (<c>application/problem+json</c>) response.
    ///
    /// This method finalizes the rule and registers it into the
    /// HTTP mapping rule set.
    /// </summary>
    /// <param name="configure">
    /// An action that configures the problem details response
    /// using a <see cref="ProblemRuleBuilder"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <c>null</c>.
    /// </exception>
    public void Problem(Action<ProblemRuleBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new ProblemRuleBuilder();
        configure(builder);

        var definition = builder.Build();

        var rule = new ProblemHttpMappingRule(
            _predicate,
            definition.Status,
            definition.Title,
            definition.Detail,
            _headers,
            definition.Extensions,
            _name
        );

        _commit(rule);
    }

    /// <summary>
    /// Adds a static HTTP response header that will be applied
    /// when this rule matches.
    /// </summary>
    /// <param name="name">
    /// The HTTP header name.
    /// </param>
    /// <param name="value">
    /// The header value. A <c>null</c> value results in the header
    /// being added with a <c>null</c> value.
    /// </param>
    /// <returns>
    /// The current <see cref="RuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public RuleBuilder WithHeader(
        string name,
        string? value)
    {
        WithHeader(name, _ => value);
        return this;
    }

    /// <summary>
    /// Adds a dynamic HTTP response header that will be evaluated
    /// against the current <see cref="HttpResultMappingContext"/>
    /// when the rule matches.
    /// </summary>
    /// <param name="name">
    /// The HTTP header name.
    /// </param>
    /// <param name="valueFactory">
    /// A factory function that produces the header value based on
    /// the mapping context.
    /// </param>
    /// <returns>
    /// The current <see cref="RuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public RuleBuilder WithHeader(
        string name,
        Func<HttpResultMappingContext, string?> valueFactory)
    {
        _headers.Add(new HeaderDescriptor(name, valueFactory));
        return this;
    }
}

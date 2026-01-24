using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// Builds a single HTTP mapping rule.
/// </summary>
public sealed class RuleBuilder
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly Action<IHttpMappingRule> _commit;
    private readonly List<HeaderDescriptor> _headers = new();
    private readonly List<ProblemExtensionDescriptor> _extensions = new();
    private string? _name;

    internal RuleBuilder(
        Func<HttpResultMappingContext, bool> predicate,
        Action<IHttpMappingRule> commit)
    {
        _predicate = predicate;
        _commit = commit;
    }

    /// <summary>
    /// Defines how the matched result is mapped to an HTTP response.
    /// </summary>
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
    /// Names the rule.
    /// </summary>
    public RuleBuilder Named(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Maps the matched failure to an RFC 7807 problem response.
    /// </summary>
    public void Problem(Action<ProblemRuleBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new ProblemRuleBuilder();
        configure(builder);

        var definition = builder.Build();

        var headers = _headers
            .Concat(definition.Headers)
            .ToList();

        var rule = new ProblemHttpMappingRule(
            _predicate,
            definition.Status,
            definition.Title,
            definition.Detail,
            headers,
            _extensions,
            _name
        );

        _commit(rule);
    }

    /// <summary>
    /// Adds an HTTP header to the mapped response.
    /// </summary>
    public RuleBuilder WithHeader(
        string name,
        string? value)
    {
        WithHeader(name, _ => value);
        return this;
    }
    
    /// <summary>
    /// Adds an HTTP header to the mapped response.
    /// </summary>
    public RuleBuilder WithHeader(
        string name,
        Func<HttpResultMappingContext, string?> valueFactory)
    {
        _headers.Add(new HeaderDescriptor(name, valueFactory));
        return this;
    }
}

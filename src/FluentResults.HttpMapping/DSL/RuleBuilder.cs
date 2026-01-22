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

        var rule = new DelegateHttpMappingRule(_predicate, mapper);
        _commit(rule);
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

        Map(builder.Build());
    }
}

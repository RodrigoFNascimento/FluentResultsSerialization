using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Execution;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Routing;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// Root DSL builder used to configure HTTP result mapping rules.
/// </summary>
public sealed class HttpResultMappingBuilder
{
    private readonly List<IHttpMappingRule> _rules = new();

    /// <summary>
    /// Starts a new rule definition.
    /// </summary>
    public RuleBuilder When(Func<HttpResultMappingContext, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return new RuleBuilder(predicate, rule => _rules.Add(rule));
    }

    /// <summary>
    /// Builds the configured rule set.
    /// </summary>
    internal IHttpMappingRuleSet Build()
        => new HttpMappingRuleSet(_rules);
}

using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Default implementation of <see cref="IHttpMappingRuleSet"/>.
/// 
/// This type is responsible for executing a sequence of HTTP mapping rules
/// in order and returning the result of the first rule that matches.
/// </summary>
internal sealed class HttpMappingRuleSet : IHttpMappingRuleSet
{
    private readonly IReadOnlyList<IHttpMappingRule> _rules;

    /// <summary>
    /// Initializes a new <see cref="HttpMappingRuleSet"/>.
    /// </summary>
    /// <param name="rules">
    /// The ordered list of mapping rules.
    /// The order is significant: the first matching rule wins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rules"/> is null.
    /// </exception>
    public HttpMappingRuleSet(IEnumerable<IHttpMappingRule> rules)
    {
        if (rules is null)
            throw new ArgumentNullException(nameof(rules));

        _rules = rules.ToList();
    }

    /// <summary>
    /// Executes the rule set against the provided mapping context.
    /// </summary>
    /// <param name="context">The mapping context.</param>
    /// <returns>
    /// The HTTP result produced by the first matching rule.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no rule matches the given context.
    /// </exception>
    public IResult Execute(HttpResultMappingContext context)
    {
        foreach (var rule in _rules)
        {
            if (rule.Matches(context))
                return rule.Map(context);
        }

        throw new InvalidOperationException(
            "No HTTP mapping rule matched the given result.");
    }
}

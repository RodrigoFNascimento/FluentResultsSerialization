using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Executes HTTP mapping rules in order until one matches.
/// </summary>
internal sealed class HttpMappingRuleSet : IHttpMappingRuleSet
{
    private readonly IReadOnlyList<IHttpMappingRule> _rules;

    /// <summary>
    /// Creates a rule set with user-defined rules and a default success rule.
    /// </summary>
    public HttpMappingRuleSet(IEnumerable<IHttpMappingRule> rules)
    {
        _rules = rules
            .Concat(new[] { new SuccessHttpMappingRule() })
            .ToList();
    }

    /// <summary>
    /// Executes the first matching rule against the given mapping context.
    /// </summary>
    public IResult Execute(HttpResultMappingContext context)
    {
        foreach (var rule in _rules)
        {
            if (!rule.Matches(context))
                continue;

            var result = rule.Map(context);

            // Apply collected headers
            foreach (var header in context.Headers)
            {
                context.HttpContext.Response.Headers[header.Key] =
                    new Microsoft.Extensions.Primitives.StringValues(header.Value);
            }

            return result;
        }

        throw new InvalidOperationException(
            "No HTTP mapping rule matched the result.");
    }

}

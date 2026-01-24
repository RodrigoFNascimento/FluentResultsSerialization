using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Results;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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
            .Concat(new[] { new SuccessHttpMappingRule("default-success") })
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

            Dictionary<string, StringValues> headers = new();
            
            foreach (var header in rule.Headers)
            {
                var value = header.ValueFactory(context);

                if (value is null)
                    continue;

                headers.Add(header.Name, new string?[] { value });
            }

            return result.WithHeaders(headers);
        }

        throw new InvalidOperationException(
            "No HTTP mapping rule matched the result.");
    }
}

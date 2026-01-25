using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Results;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Executes HTTP mapping rules in order until one matches.
///
/// This type is responsible for iterating through the configured
/// <see cref="IHttpMappingRule"/> instances, selecting the first rule
/// whose predicate matches the current <see cref="HttpResultMappingContext"/>,
/// executing it, and applying any headers produced by that rule.
/// </summary>
internal sealed class HttpMappingRuleSet : IHttpMappingRuleSet
{
    private readonly IReadOnlyList<IHttpMappingRule> _rules;

    /// <summary>
    /// Initializes a new <see cref="HttpMappingRuleSet"/> with the provided rules
    /// and appends a default success rule as a fallback.
    /// </summary>
    /// <param name="rules">
    /// The user-defined HTTP mapping rules, evaluated in the order they are provided.
    /// </param>
    /// <remarks>
    /// A default success rule is always appended to ensure that successful results
    /// are mapped even if no explicit success rule is defined by the consumer.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rules"/> is <c>null</c>.
    /// </exception>
    public HttpMappingRuleSet(IEnumerable<IHttpMappingRule> rules)
    {
        if (rules is null)
            throw new ArgumentNullException(nameof(rules));

        _rules = rules
            .Concat(new[] { new SuccessHttpMappingRule("default-success") })
            .ToList();
    }

    public IResult Execute(HttpResultMappingContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        foreach (var rule in _rules)
        {
            if (!rule.Matches(context))
                continue;

            var result = rule.Map(context);

            var headers = new Dictionary<string, StringValues>();

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

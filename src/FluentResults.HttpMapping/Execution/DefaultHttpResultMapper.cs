using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Default implementation of <see cref="IHttpResultMapper"/>.
///
/// This type acts as the bridge between Minimal API endpoints
/// and the HTTP mapping rule system. It takes a <see cref="Result"/>
/// produced by an endpoint, wraps it in an <see cref="HttpResultMappingContext"/>,
/// and delegates execution to the configured mapping rule set.
/// </summary>
internal sealed class DefaultHttpResultMapper : IHttpResultMapper
{
    private readonly IHttpMappingRuleSet _ruleSet;

    /// <summary>
    /// Initializes a new <see cref="DefaultHttpResultMapper"/>.
    /// </summary>
    /// <param name="ruleSet">
    /// The rule set used to map results to HTTP responses.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="ruleSet"/> is <c>null</c>.
    /// </exception>
    public DefaultHttpResultMapper(IHttpMappingRuleSet ruleSet)
    {
        _ruleSet = ruleSet ?? throw new ArgumentNullException(nameof(ruleSet));
    }

    public IResult Map(Result result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }

    public IResult Map<T>(Result<T> result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }
}

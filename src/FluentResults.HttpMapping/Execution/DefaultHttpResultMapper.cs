using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Default implementation of <see cref="IHttpResultMapper"/>.
/// 
/// This type acts as the bridge between Minimal API endpoints
/// and the HTTP mapping rule system.
/// </summary>
public sealed class DefaultHttpResultMapper : IHttpResultMapper
{
    private readonly IHttpMappingRuleSet _ruleSet;

    /// <summary>
    /// Initializes a new <see cref="DefaultHttpResultMapper"/>.
    /// </summary>
    /// <param name="ruleSet">
    /// The rule set used to map results to HTTP responses.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="ruleSet"/> is null.
    /// </exception>
    public DefaultHttpResultMapper(IHttpMappingRuleSet ruleSet)
    {
        _ruleSet = ruleSet ?? throw new ArgumentNullException(nameof(ruleSet));
    }

    /// <inheritdoc />
    public IResult Map(Result result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }

    /// <inheritdoc />
    public IResult Map<T>(Result<T> result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }
}

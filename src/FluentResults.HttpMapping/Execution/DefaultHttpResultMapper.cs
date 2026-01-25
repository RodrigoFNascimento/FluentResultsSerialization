using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Default implementation of <see cref="IHttpResultMapper"/>.
///
/// This type acts as the bridge between Minimal API endpoints
/// and the HTTP mapping rule system. It takes a <see cref="FluentResults.Result"/>
/// produced by an endpoint, wraps it in an <see cref="HttpResultMappingContext"/>,
/// and delegates execution to the configured mapping rule set.
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
    /// Thrown when <paramref name="ruleSet"/> is <c>null</c>.
    /// </exception>
    public DefaultHttpResultMapper(IHttpMappingRuleSet ruleSet)
    {
        _ruleSet = ruleSet ?? throw new ArgumentNullException(nameof(ruleSet));
    }

    /// <summary>
    /// Maps a non-generic <see cref="Result"/> to an HTTP response.
    /// </summary>
    /// <param name="result">
    /// The FluentResults result produced by an endpoint.
    /// </param>
    /// <returns>
    /// An <see cref="IResult"/> representing the mapped HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    public IResult Map(Result result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }

    /// <summary>
    /// Maps a generic <see cref="Result{T}"/> to an HTTP response.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value contained in the successful result.
    /// </typeparam>
    /// <param name="result">
    /// The FluentResults result produced by an endpoint.
    /// </param>
    /// <returns>
    /// An <see cref="IResult"/> representing the mapped HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    public IResult Map<T>(Result<T> result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        var context = new HttpResultMappingContext(result);
        return _ruleSet.Execute(context);
    }
}

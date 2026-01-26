using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FluentResults.HttpMapping.Results;

/// <summary>
/// Extension methods for composing ASP.NET <see cref="IResult"/> instances
/// produced by the HTTP mapping pipeline.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Wraps an <see cref="IResult"/> with additional HTTP response headers.
    /// </summary>
    /// <remarks>
    /// This method does not mutate the original result. Instead, it returns
    /// a decorated <see cref="IResult"/> that applies the provided headers
    /// before executing the inner result.
    /// 
    /// This is used internally by the mapping engine to apply headers
    /// defined by mapping rules.
    /// </remarks>
    /// <param name="result">
    /// The result to which headers should be applied.
    /// </param>
    /// <param name="headers">
    /// The headers to add to the HTTP response.
    /// </param>
    /// <returns>
    /// A new <see cref="IResult"/> that applies the given headers
    /// before executing the original result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> or <paramref name="headers"/> is <c>null</c>.
    /// </exception>
    public static IResult WithHeaders(
        this IResult result,
        IDictionary<string, StringValues> headers)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        if (headers is null)
            throw new ArgumentNullException(nameof(headers));

        return new HeaderResult(result, headers);
    }
}

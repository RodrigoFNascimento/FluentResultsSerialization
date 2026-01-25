using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FluentResults.HttpMapping.Results;

/// <summary>
/// An <see cref="IResult"/> decorator that adds HTTP headers
/// to the response before executing an inner result.
/// </summary>
/// <remarks>
/// This type is used internally by the HTTP mapping pipeline to
/// apply headers defined by mapping rules without mutating or
/// depending on the original <see cref="IResult"/> instance.
/// 
/// Headers are written to the HTTP response prior to executing
/// the wrapped result.
/// </remarks>
internal sealed class HeaderResult : IResult
{
    private readonly IResult _inner;
    private readonly IDictionary<string, StringValues> _headers;

    /// <summary>
    /// Initializes a new <see cref="HeaderResult"/>.
    /// </summary>
    /// <param name="inner">
    /// The inner <see cref="IResult"/> to execute after applying headers.
    /// </param>
    /// <param name="headers">
    /// The collection of HTTP headers to add to the response.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="inner"/> or <paramref name="headers"/> is <c>null</c>.
    /// </exception>
    public HeaderResult(
        IResult inner,
        IDictionary<string, StringValues> headers)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>
    /// Executes the inner result after adding the configured headers
    /// to the HTTP response.
    /// </summary>
    /// <param name="httpContext">
    /// The current HTTP context.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous execution operation.
    /// </returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext is null)
            throw new ArgumentNullException(nameof(httpContext));

        foreach (var header in _headers)
            httpContext.Response.Headers.Add(header.Key, header.Value);

        await _inner.ExecuteAsync(httpContext);
    }
}

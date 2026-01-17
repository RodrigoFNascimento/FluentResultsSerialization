using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FluentResultsSerialization.IResults;

/// <summary>
/// An IResult with added headers.
/// </summary>
internal sealed class HeaderResult : IResult
{
    private readonly IResult _inner;
    private readonly IDictionary<string, StringValues> _headers;

    public HeaderResult(IResult inner, IDictionary<string, StringValues> headers)
    {
        _inner = inner;
        _headers = headers;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        foreach (var header in _headers)
            httpContext.Response.Headers.Add(header.Key, header.Value);

        await _inner.ExecuteAsync(httpContext);
    }
}

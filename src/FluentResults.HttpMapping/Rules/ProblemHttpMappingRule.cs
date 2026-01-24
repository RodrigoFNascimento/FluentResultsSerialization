using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// Maps failed FluentResults to RFC 7807 Problem responses.
/// </summary>
internal sealed class ProblemHttpMappingRule : IHttpMappingRule
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly HttpStatusCode _status;
    private readonly Func<HttpResultMappingContext, string?>? _title;
    private readonly Func<HttpResultMappingContext, string?>? _detail;
    private readonly IReadOnlyList<HeaderDescriptor> _headers;
    private readonly IReadOnlyList<ProblemExtensionDescriptor> _extensions;

    public string? Name { get; }

    /// <summary>
    /// Creates a problem mapping rule.
    /// </summary>
    public ProblemHttpMappingRule(
        Func<HttpResultMappingContext, bool> predicate,
        HttpStatusCode status,
        Func<HttpResultMappingContext, string?>? title,
        Func<HttpResultMappingContext, string?>? detail,
        IReadOnlyList<HeaderDescriptor> headers,
        IReadOnlyList<ProblemExtensionDescriptor> extensions,
        string? name)
    {
        _predicate = predicate;
        _status = status;
        _title = title;
        _detail = detail;
        _headers = headers;
        _extensions = extensions;
        Name = name;
    }

    /// <summary>
    /// Matches when the predicate evaluates to true.
    /// </summary>
    public bool Matches(HttpResultMappingContext context)
        => _predicate(context);

    /// <summary>
    /// Maps a failure result to a ProblemDetails response.
    /// </summary>
    public IResult Map(HttpResultMappingContext context)
    {
        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: (int)_status,
            title: _title?.Invoke(context),
            detail: _detail?.Invoke(context),
            extensions: BuildExtensions(context)
        );
    }

    public IReadOnlyList<HeaderDescriptor> Headers => _headers;

    private IDictionary<string, object?>? BuildExtensions(
        HttpResultMappingContext context)
    {
        if (_extensions.Count == 0)
            return null;

        var dict = new Dictionary<string, object?>();

        foreach (var ext in _extensions)
            dict[ext.Name] = ext.ValueFactory(context);

        return dict;
    }
}

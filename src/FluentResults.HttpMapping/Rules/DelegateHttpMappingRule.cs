using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// HTTP mapping rule backed by delegates.
/// </summary>
internal sealed class DelegateHttpMappingRule : IHttpMappingRule
{
    private readonly Func<HttpResultMappingContext, bool> _predicate;
    private readonly Func<HttpResultMappingContext, IResult> _mapper;
    private readonly IReadOnlyList<HeaderDescriptor> _headers;

    public string? Name { get; }

    public DelegateHttpMappingRule(
        Func<HttpResultMappingContext, bool> predicate,
        Func<HttpResultMappingContext, IResult> mapper,
        IReadOnlyList<HeaderDescriptor> headers,
        string? name)
    {
        _predicate = predicate;
        _mapper = mapper;
        _headers = headers;
        Name = name;
    }

    public bool Matches(HttpResultMappingContext context)
        => _predicate(context);

    public IResult Map(HttpResultMappingContext context)
        => _mapper(context);

    /// <summary>
    /// Headers to apply to the HTTP response.
    /// </summary>
    public IReadOnlyList<HeaderDescriptor> Headers => _headers;
}

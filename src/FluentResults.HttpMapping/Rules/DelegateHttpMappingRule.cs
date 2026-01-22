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

    public DelegateHttpMappingRule(
        Func<HttpResultMappingContext, bool> predicate,
        Func<HttpResultMappingContext, IResult> mapper)
    {
        _predicate = predicate;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public bool Matches(HttpResultMappingContext context)
        => _predicate(context);

    /// <inheritdoc />
    public IResult Map(HttpResultMappingContext context)
        => _mapper(context);

    public IReadOnlyList<HeaderDescriptor> Headers { get; }
}

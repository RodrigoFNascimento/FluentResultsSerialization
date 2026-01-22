using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// DSL builder for configuring RFC 7807 problem responses.
/// </summary>
public sealed class ProblemRuleBuilder
{
    private HttpStatusCode? _status;
    private string? _title;
    private Func<HttpResultMappingContext, string?>? _detail;
    private readonly List<HeaderDescriptor> _headers = new();

    /// <summary>
    /// Sets the HTTP status code.
    /// </summary>
    public ProblemRuleBuilder WithStatus(HttpStatusCode status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the problem title.
    /// </summary>
    public ProblemRuleBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the problem detail dynamically.
    /// </summary>
    public ProblemRuleBuilder WithDetail(
        Func<HttpResultMappingContext, string?> detail)
    {
        _detail = detail;
        return this;
    }

    /// <summary>
    /// Adds an HTTP header to the problem response.
    /// </summary>
    public ProblemRuleBuilder WithHeader(
        string name,
        Func<HttpResultMappingContext, string?> valueFactory)
    {
        _headers.Add(new HeaderDescriptor(name, valueFactory));
        return this;
    }

    internal ProblemRuleDefinition Build()
    {
        return new ProblemRuleDefinition
        {
            Status = _status ?? HttpStatusCode.InternalServerError,
            Title = _title is null ? null : _ => _title,
            Detail = _detail,
            Headers = _headers.ToList()
        };
    }
}

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

    internal Func<HttpResultMappingContext, IResult> Build()
    {
        return ctx =>
            Results.Problem(
                statusCode: (int?)_status,
                title: _title,
                detail: _detail?.Invoke(ctx));
    }
}

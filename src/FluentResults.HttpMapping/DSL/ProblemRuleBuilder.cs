using FluentResults.HttpMapping.Context;
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
    private readonly List<ProblemExtensionDescriptor> _extensions = new();

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
    /// Sets the problem detail.
    /// </summary>
    public ProblemRuleBuilder WithDetail(
        string detail)
    {
        WithDetail(_ => detail);
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
    /// Adds a standard "errors" extension for validation failures.
    /// Intended for 400 Bad Request responses.
    /// </summary>
    public ProblemRuleBuilder WithErrors(
        Func<HttpResultMappingContext, IDictionary<string, string[]>> factory)
    {
        return WithExtension("errors", ctx => factory(ctx));
    }

    /// <summary>
    /// Adds a Problem Details extension member.
    /// </summary>
    public ProblemRuleBuilder WithExtension(
        string name,
        Func<HttpResultMappingContext, object?> valueFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Extension name cannot be null or empty.", nameof(name));

        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));

        _extensions.Add(new ProblemExtensionDescriptor(name, valueFactory));
        return this;
    }

    /// <summary>
    /// Adds a standard "errors" extension for validation failures
    /// from the Error metadata.
    /// Intended for 400 Bad Request responses.
    /// </summary>
    public ProblemRuleBuilder WithValidationErrorsFromMetadata(
        string fieldMetadataKey = "errors")
    {
        return WithErrors(ctx =>
        {
            return ctx.Result.Errors
                .SelectMany(error =>
                {
                    if (!error.Metadata.TryGetValue(fieldMetadataKey, out var field) ||
                        field is null)
                    {
                        return Enumerable.Empty<(string Field, string Message)>();
                    }

                    return new[]
                    {
                    (Field: field.ToString()!, Message: error.Message)
                    };
                })
                .GroupBy(x => x.Field)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Message).ToArray()
                );
        });
    }

    /// <summary>
    /// Helper method for composing a full validation Problem Details.
    /// </summary>
    public ProblemRuleBuilder WithValidationProblem(
        string title = "Invalid request data.",
        string detail = "One or more validation errors occurred.",
        string fieldMetadataKey = "errors")
    {
        WithStatus(HttpStatusCode.BadRequest);

        WithTitle(title);

        WithDetail(detail);

        WithValidationErrorsFromMetadata(fieldMetadataKey);

        return this;
    }

    internal ProblemRuleDefinition Build()
    {
        return new ProblemRuleDefinition
        {
            Status = _status ?? HttpStatusCode.InternalServerError,
            Title = _title is null ? null : _ => _title,
            Detail = _detail,
            Extensions = _extensions.ToList()
        };
    }
}

using FluentResults.HttpMapping.Context;
using System.Net;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// DSL builder for configuring RFC 7807
/// (<c>application/problem+json</c>) responses.
///
/// <see cref="ProblemRuleBuilder"/> is used to declaratively construct
/// Problem Details responses for failed results, including:
/// <list type="bullet">
/// <item><description>HTTP status code</description></item>
/// <item><description>Problem title and detail</description></item>
/// <item><description>Custom extension members</description></item>
/// </list>
///
/// Instances of this builder are short-lived and are only used during
/// rule configuration. The resulting configuration is materialized
/// into an immutable rule definition when the rule is finalized.
/// </summary>
public sealed class ProblemRuleBuilder
{
    private HttpStatusCode? _status;
    private string? _title;
    private Func<HttpResultMappingContext, string?>? _detail;
    private readonly List<ProblemExtensionDescriptor> _extensions = new();

    /// <summary>
    /// Sets the HTTP status code for the Problem Details response.
    /// </summary>
    /// <param name="status">
    /// The HTTP status code to return.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public ProblemRuleBuilder WithStatus(HttpStatusCode status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the <c>title</c> member of the Problem Details response.
    /// </summary>
    /// <param name="title">
    /// A short, human-readable summary of the problem type.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public ProblemRuleBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets a static <c>detail</c> value for the Problem Details response.
    /// </summary>
    /// <param name="detail">
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public ProblemRuleBuilder WithDetail(
        string detail)
    {
        WithDetail(_ => detail);
        return this;
    }

    /// <summary>
    /// Sets the <c>detail</c> member of the Problem Details response dynamically.
    /// </summary>
    /// <param name="detail">
    /// A function that produces the <c>detail</c> value based on the
    /// <see cref="HttpResultMappingContext"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public ProblemRuleBuilder WithDetail(
        Func<HttpResultMappingContext, string?> detail)
    {
        _detail = detail;
        return this;
    }

    /// <summary>
    /// Adds a standard <c>"errors"</c> extension member to the Problem Details
    /// response.
    ///
    /// This method is primarily intended for validation failures and is
    /// commonly used with <c>400 Bad Request</c> responses.
    /// </summary>
    /// <param name="factory">
    /// A function that produces a dictionary of validation errors, keyed
    /// by field name.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    public ProblemRuleBuilder WithErrors(
        Func<HttpResultMappingContext, IDictionary<string, string[]>> factory)
    {
        return WithExtension("errors", ctx => factory(ctx));
    }

    /// <summary>
    /// Adds a custom extension member to the Problem Details response.
    /// </summary>
    /// <param name="name">
    /// The extension member name. This value must be non-empty and non-whitespace.
    /// </param>
    /// <param name="valueFactory">
    /// A function that produces the extension value based on the
    /// <see cref="HttpResultMappingContext"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is <c>null</c>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="valueFactory"/> is <c>null</c>.
    /// </exception>
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
    /// Adds a standard <c>"errors"</c> extension member for validation failures
    /// by extracting field information from error metadata.
    ///
    /// This method assumes validation errors store field identifiers in
    /// metadata entries and groups messages by field name.
    /// </summary>
    /// <param name="fieldMetadataKey">
    /// The metadata key that identifies the field associated with a validation error.
    /// Defaults to <c>"errors"</c>.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance, allowing
    /// fluent chaining.
    /// </returns>
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
    /// Configures a complete validation Problem Details response using
    /// error metadata to populate the <c>"errors"</c> extension.
    /// </summary>
    /// <param name="title">
    /// The problem title. Defaults to <c>"Invalid request data."</c>.
    /// </param>
    /// <param name="detail">
    /// The problem detail message. Defaults to
    /// <c>"One or more validation errors occurred."</c>.
    /// </param>
    /// <param name="fieldMetadataKey">
    /// The metadata key used to extract field identifiers from validation errors.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance.
    /// </returns>
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

    /// <summary>
    /// Configures a complete validation Problem Details response by
    /// extracting validation data directly from a typed validation error.
    /// </summary>
    /// <typeparam name="TValidationError">
    /// The specific validation error type to extract data from.
    /// </typeparam>
    /// <param name="errorsSelector">
    /// A function that extracts a dictionary of validation errors from
    /// the matched validation error instance.
    /// </param>
    /// <param name="title">
    /// The problem title. Defaults to <c>"Invalid request data."</c>.
    /// </param>
    /// <param name="detail">
    /// The problem detail message. Defaults to
    /// <c>"One or more validation errors occurred."</c>.
    /// </param>
    /// <returns>
    /// The current <see cref="ProblemRuleBuilder"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorsSelector"/> is <c>null</c>.
    /// </exception>
    public ProblemRuleBuilder WithValidationProblem<TValidationError>(
        Func<TValidationError, IDictionary<string, string[]>> errorsSelector,
        string title = "Invalid request data.",
        string detail = "One or more validation errors occurred.")
        where TValidationError : IReason
    {
        if (errorsSelector is null)
            throw new ArgumentNullException(nameof(errorsSelector));

        WithStatus(HttpStatusCode.BadRequest);
        WithTitle(title);
        WithDetail(detail);

        WithErrors(ctx =>
        {
            var error = ctx.Result.Reasons
                .OfType<TValidationError>()
                .First();

            return errorsSelector(error);
        });

        return this;
    }

    /// <summary>
    /// Builds the immutable definition representing the configured
    /// Problem Details response.
    /// </summary>
    /// <returns>
    /// A <see cref="ProblemRuleDefinition"/> containing the configured
    /// status code, title, detail, and extensions.
    /// </returns>
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

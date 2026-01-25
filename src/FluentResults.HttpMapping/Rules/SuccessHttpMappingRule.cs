using FluentResults.HttpMapping.Context;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// Default HTTP mapping rule that converts successful FluentResults
/// into conventional HTTP success responses.
/// </summary>
/// <remarks>
/// This rule is automatically appended to the end of the rule set
/// and acts as a fallback for successful results when no user-defined
/// rule matches.
///
/// The mapping behavior follows common REST conventions:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="Result{T}"/> is mapped to <c>200 OK</c> with the value
/// serialized as the response body.
/// </description>
/// </item>
/// <item>
/// <description>
/// Non-generic <see cref="Result"/> is mapped to
/// <c>204 No Content</c>.
/// </description>
/// </item>
/// </list>
///
/// This rule produces no headers and has no side effects.
/// </remarks>
internal sealed class SuccessHttpMappingRule : IHttpMappingRule
{
    public string? Name { get; }

    public IReadOnlyList<HeaderDescriptor> Headers { get; }
        = Array.Empty<HeaderDescriptor>();

    /// <summary>
    /// Initializes a new <see cref="SuccessHttpMappingRule"/>.
    /// </summary>
    /// <param name="name">
    /// Optional rule name, primarily intended for diagnostics
    /// and observability.
    /// </param>
    public SuccessHttpMappingRule(string? name = null)
    {
        Name = name;
    }

    public bool Matches(HttpResultMappingContext context)
        => context.Result.IsSuccess;

    public IResult Map(HttpResultMappingContext context)
    {
        var result = context.Result;
        var type = result.GetType();

        // Result<T> → 200 OK with value
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var value = type
                .GetProperty("Value")!
                .GetValue(result);

            return Microsoft.AspNetCore.Http.Results.Ok(value);
        }

        // Result → 204 No Content
        return Microsoft.AspNetCore.Http.Results.NoContent();
    }
}

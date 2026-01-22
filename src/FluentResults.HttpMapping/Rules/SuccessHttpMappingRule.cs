using FluentResults;
using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Rules;

/// <summary>
/// Default rule that maps successful FluentResults to HTTP responses.
/// </summary>
internal sealed class SuccessHttpMappingRule : IHttpMappingRule
{
    /// <summary>
    /// Matches when the result represents success.
    /// </summary>
    public bool Matches(HttpResultMappingContext context)
        => context.Result.IsSuccess;

    /// <summary>
    /// Maps a successful result to an HTTP response.
    /// </summary>
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

            return Results.Ok(value);
        }

        // Result → 204 No Content
        return Results.NoContent();
    }
}

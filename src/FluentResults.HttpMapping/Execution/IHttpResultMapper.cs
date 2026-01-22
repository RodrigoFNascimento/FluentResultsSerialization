using FluentResults;
using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Service responsible for converting FluentResults results returned
/// by endpoints into ASP.NET HTTP results.
/// 
/// This is the primary interface consumed by Minimal API endpoints.
/// </summary>
public interface IHttpResultMapper
{
    /// <summary>
    /// Maps a non-generic FluentResults result to an HTTP result.
    /// </summary>
    /// <param name="result">The result returned by the endpoint.</param>
    /// <returns>An ASP.NET HTTP result.</returns>
    IResult Map(Result result);

    /// <summary>
    /// Maps a generic FluentResults result to an HTTP result.
    /// </summary>
    /// <typeparam name="T">The type of the successful value.</typeparam>
    /// <param name="result">The result returned by the endpoint.</param>
    /// <returns>An ASP.NET HTTP result.</returns>
    IResult Map<T>(Result<T> result);
}

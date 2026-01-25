using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Execution;

/// <summary>
/// Service responsible for converting FluentResults results returned
/// by endpoints into ASP.NET HTTP results.
/// </summary>
/// <remarks>
/// This interface represents the primary integration point between
/// Minimal API endpoints and the HTTP mapping system.
/// 
/// Implementations delegate the mapping process to a configured
/// set of HTTP mapping rules, translating a <see cref="Result"/> or
/// <see cref="Result{T}"/> into an ASP.NET <see cref="IResult"/>.
/// </remarks>
public interface IHttpResultMapper
{
    /// <summary>
    /// Maps a non-generic FluentResults result to an HTTP result.
    /// </summary>
    /// <param name="result">
    /// The <see cref="Result"/> instance returned by the endpoint.
    /// </param>
    /// <returns>
    /// An ASP.NET <see cref="IResult"/> representing the HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    IResult Map(Result result);

    /// <summary>
    /// Maps a generic FluentResults result to an HTTP result.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the successful value contained in the result.
    /// </typeparam>
    /// <param name="result">
    /// The <see cref="Result{T}"/> instance returned by the endpoint.
    /// </param>
    /// <returns>
    /// An ASP.NET <see cref="IResult"/> representing the HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    IResult Map<T>(Result<T> result);
}

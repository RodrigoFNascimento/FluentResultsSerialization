using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentResults.HttpMapping.MVC;

/// <summary>
/// Extension methods for adapting ASP.NET Minimal API <see cref="IResult"/>
/// instances to MVC-compatible <see cref="IActionResult"/>.
/// </summary>
/// <remarks>
/// This allows FluentResults HTTP mappings to be reused in applications
/// that use MVC controllers instead of Minimal APIs.
/// </remarks>
public static class ResultAdapterExtensions
{
    /// <summary>
    /// Converts an <see cref="IResult"/> into an <see cref="IActionResult"/>
    /// so it can be returned from MVC controller actions.
    /// </summary>
    /// <param name="result">
    /// The ASP.NET HTTP result produced by the FluentResults HTTP mapping system.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that delegates execution to the underlying
    /// <see cref="IResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    public static IActionResult ToActionResult(this IResult result)
        => new ResultActionResult(result);
}

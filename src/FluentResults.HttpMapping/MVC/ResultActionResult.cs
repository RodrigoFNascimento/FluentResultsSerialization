using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentResults.HttpMapping.MVC;

/// <summary>
/// MVC <see cref="IActionResult"/> adapter that wraps an ASP.NET
/// <see cref="IResult"/>.
/// </summary>
/// <remarks>
/// This type bridges the gap between Minimal API results and MVC controllers
/// by delegating execution directly to the wrapped <see cref="IResult"/>.
/// 
/// No additional behavior is introduced — headers, status codes, and response
/// bodies are entirely controlled by the underlying result.
/// </remarks>
internal sealed class ResultActionResult : IActionResult
{
    private readonly IResult _result;

    /// <summary>
    /// Initializes a new <see cref="ResultActionResult"/>.
    /// </summary>
    /// <param name="result">
    /// The ASP.NET HTTP result to execute when this action result runs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    public ResultActionResult(IResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// Executes the wrapped <see cref="IResult"/> using the MVC
    /// <see cref="ActionContext"/>.
    /// </summary>
    /// <param name="context">
    /// The current action context.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous execution of the result.
    /// </returns>
    public Task ExecuteResultAsync(ActionContext context)
    {
        return _result.ExecuteAsync(context.HttpContext);
    }
}

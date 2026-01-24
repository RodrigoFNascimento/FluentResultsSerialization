using Microsoft.AspNetCore.Http;

namespace FluentResults.HttpMapping.Context;

/// <summary>
/// Immutable context object passed to HTTP mapping rules.
/// 
/// It represents a snapshot of a <see cref="FluentResults.Result"/> or
/// <see cref="Result{T}"/> after an endpoint has executed,
/// exposing only the data that mapping rules are allowed to inspect.
/// 
/// This type is intentionally pure and contains no ASP.NET dependencies,
/// allowing mapping rules to remain deterministic and side-effect free.
/// </summary>
public sealed class HttpResultMappingContext
{
    /// <summary>
    /// The current ASP.NET HTTP context.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// The underlying FluentResults result returned by the endpoint.
    /// </summary>
    public IResultBase Result { get; }

    /// <summary>
    /// Indicates whether the result represents a successful outcome.
    /// </summary>
    public bool IsSuccess => Result.IsSuccess;

    /// <summary>
    /// Indicates whether the result represents a failed outcome.
    /// </summary>
    public bool IsFailed => Result.IsFailed;

    /// <summary>
    /// All reasons (errors or successes) attached to the result.
    /// </summary>
    public IReadOnlyList<IReason> Reasons => Result.Reasons;

    /// <summary>
    /// Initializes a new <see cref="HttpResultMappingContext"/> for the given result.
    /// </summary>
    /// <param name="result">
    /// The FluentResults result produced by an endpoint.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is null.
    /// </exception>
    public HttpResultMappingContext(IResultBase result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// Determines whether any reason attached to the result contains
    /// metadata with the specified key.
    /// </summary>
    /// <param name="key">The metadata key to look for.</param>
    /// <returns>
    /// <c>true</c> if at least one reason contains the key; otherwise, <c>false</c>.
    /// </returns>
    public bool HasMetadata(string key)
        => Reasons.Any(r => r.Metadata.ContainsKey(key));

    /// <summary>
    /// Retrieves all metadata values associated with the specified key
    /// across all reasons.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>
    /// A sequence of metadata values. The sequence may be empty if the key
    /// does not exist on any reason.
    /// </returns>
    public IEnumerable<object?> GetMetadata(string key)
        => Reasons
            .Where(r => r.Metadata.ContainsKey(key))
            .Select(r => r.Metadata[key]);

    /// <summary>
    /// Headers to be applied to the HTTP response.
    /// </summary>
    public IDictionary<string, string[]> Headers { get; }
        = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds or replaces a response header.
    /// </summary>
    public void AddHeader(string name, params string[] values)
    {
        Headers[name] = values;
    }
}

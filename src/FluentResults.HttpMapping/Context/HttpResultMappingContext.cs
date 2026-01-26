namespace FluentResults.HttpMapping.Context;

/// <summary>
/// Immutable context object passed to HTTP mapping rules.
///
/// <para>
/// This type represents a read-only snapshot of a
/// <see cref="FluentResults.Result"/> or <see cref="Result{T}"/> after an
/// endpoint has executed.
/// </para>
///
/// <para>
/// It exposes only the information that mapping rules are allowed to inspect,
/// ensuring rules remain deterministic, side-effect free, and independent of
/// ASP.NET infrastructure.
/// </para>
/// </summary>
public sealed class HttpResultMappingContext
{
    /// <summary>
    /// Gets the underlying FluentResults result returned by the endpoint.
    /// </summary>
    public IResultBase Result { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents a successful outcome.
    /// </summary>
    public bool IsSuccess => Result.IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the result represents a failed outcome.
    /// </summary>
    public bool IsFailed => Result.IsFailed;

    /// <summary>
    /// Gets all reasons (errors or successes) attached to the result.
    /// </summary>
    public IReadOnlyList<IReason> Reasons => Result.Reasons;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResultMappingContext"/> class
    /// for the specified FluentResults result.
    /// </summary>
    /// <param name="result">
    /// The FluentResults result produced by an endpoint.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="result"/> is <c>null</c>.
    /// </exception>
    public HttpResultMappingContext(IResultBase result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// Gets the first reason of type <typeparamref name="TReason"/>
    /// that contains the specified metadata key.
    /// </summary>
    /// <typeparam name="TReason">
    /// The expected reason type.
    /// </typeparam>
    /// <param name="key">
    /// The metadata key that must be present on the reason.
    /// </param>
    /// <returns>
    /// The first matching reason.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no reason of type <typeparamref name="TReason"/> exists
    /// with the specified metadata key.
    /// </exception>
    public TReason FirstReasonWithMetadata<TReason>(string key)
        where TReason : IReason
    {
        return Result.Reasons
            .OfType<TReason>()
            .First(error => error.HasMetadataKey(key));
    }

    /// <summary>
    /// Gets the first reason of type <typeparamref name="TReason"/>
    /// attached to the result.
    /// </summary>
    /// <typeparam name="TReason">
    /// The expected reason type.
    /// </typeparam>
    /// <returns>
    /// The first matching reason.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no reason of type <typeparamref name="TReason"/>
    /// is attached to the result.
    /// </exception>
    public TReason FirstReason<TReason>()
        where TReason : IReason
    {
        return Result.Reasons.OfType<TReason>().First();
    }

    /// <summary>
    /// Retrieves all metadata values associated with the specified key
    /// across all reasons.
    /// </summary>
    /// <param name="key">
    /// The metadata key to retrieve.
    /// </param>
    /// <returns>
    /// A sequence of metadata values. The sequence is empty if the key
    /// is not present on any reason.
    /// </returns>
    public IEnumerable<object?> GetMetadata(string key)
        => Reasons
            .Where(r => r.Metadata.ContainsKey(key))
            .Select(r => r.Metadata[key]);

    /// <summary>
    /// Gets all reasons of type <typeparamref name="TReason"/>
    /// attached to the result.
    /// </summary>
    /// <typeparam name="TReason">
    /// The reason type to retrieve.
    /// </typeparam>
    /// <returns>
    /// A sequence of matching reasons. The sequence may be empty.
    /// </returns>
    public IEnumerable<TReason> GetReasons<TReason>()
        where TReason : IReason
    {
        return Result.Reasons.OfType<TReason>();
    }

    /// <summary>
    /// Determines whether any reason attached to the result contains
    /// metadata with the specified key.
    /// </summary>
    /// <param name="key">
    /// The metadata key to look for.
    /// </param>
    /// <returns>
    /// <c>true</c> if at least one reason contains the key; otherwise, <c>false</c>.
    /// </returns>
    public bool HasMetadata(string key)
        => Reasons.Any(r => r.Metadata.ContainsKey(key));
}

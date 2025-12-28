using FluentResults;
using Microsoft.Extensions.Primitives;

namespace FluentResultsSerialization;

internal sealed class SerializationPredicates
{
    /// <summary>
    /// Logic used to define which <see cref="Result"/> should be handled.
    /// </summary>
    public List<Func<Result, bool>> ResultPredicates { get; init; } = new();

    /// <summary>
    /// Logic used to define which <see cref="Result{T}"/> should be handled.
    /// </summary>
    public Dictionary<Type, object> GenericResultPredicates { get; init; } = new();

    /// <summary>
    /// Logic used to define that a success
    /// <see cref="Result"/> or <see cref="Result{T}"/>
    /// should be handled.
    /// </summary>
    public List<Func<Result, bool>> DefaultSuccessPredicates { get; init; } = new();

    /// <summary>
    /// Logic used to define the value of the additional
    /// headers of an HTTP response.
    /// </summary>
    public Dictionary<string, Func<Result, StringValues>> HeaderPredicates { get; init; } = new();

    /// <summary>
    /// Logic used to define the value of the additional
    /// members of a problem details object.
    /// </summary>
    public Dictionary<string, Func<Result, object?>> ExtensionPredicates { get; init; } = new();

    /// <summary>
    /// Logic used to define the value of the "detail"
    /// member of a problem details object.
    /// </summary>
    public Func<Result, string>? DetailPredicate { get; init; }

    /// <summary>
    /// Logic used to define the value of the "errors"
    /// member of a validation problem details object.
    /// </summary>
    public Func<Result, IDictionary<string, string[]>>? ValidationErrorsPredicate { get; init; }
}

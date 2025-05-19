using Microsoft.AspNetCore.Http;

namespace FluentResultsSerialization.Serializer;
/// <summary>
/// Handles serialization of instances of <see cref="FluentResults.Result"/> and <see cref="FluentResults.Result{TValue}"/>
/// into an <see cref="IResult"/>.
/// </summary>
/// <remarks>
/// Provides methods for converting FluentResults objects into a format suitable for HTTP responses or other serialized representations.
/// It utilizes underlying serialization strategies to handle different types and states of results.
/// </remarks>
public interface IResultSerializer
{
    /// <summary>
    /// Serializes <paramref name="result"/> into an instance of <see cref="IResult"/>.
    /// </summary>
    /// <param name="result">The <see cref="FluentResults.Result"/> that should be serialized.</param>
    /// <returns>The resulting instance of <see cref="IResult"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no serialization strategy is found.</exception>
    IResult Serialize(FluentResults.Result result);
    /// <summary>
    /// Serializes <paramref name="result"/> into an instance of <see cref="IResult"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="result">The <see cref="FluentResults.Result"/> that should be serialized.</param>
    /// <returns>The resulting instance of <see cref="IResult"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no serialization strategy is found.</exception>
    IResult Serialize<TValue>(FluentResults.Result<TValue> result);
}

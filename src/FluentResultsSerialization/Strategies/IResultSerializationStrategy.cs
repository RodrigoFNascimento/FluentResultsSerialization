using FluentResults;
using Microsoft.AspNetCore.Http;

namespace FluentResultsSerialization.Strategies;
/// <summary>
/// Defines a contract for strategies that serialize instances of <see cref="Result"/> and <see cref="Result{TValue}"/>
/// to an <see cref="IResult"/>. 
/// </summary>
/// <remarks>
/// This interface provides methods for determining if a specific result instance should be serialized by the strategy,
/// and for performing the serialization itself. 
/// Implementations of this interface can define custom serialization logic for different result types and scenarios,
/// such as converting errors or successes into HTTP responses or other serialized formats.
/// </remarks>
internal interface IResultSerializationStrategy
{
    /// <summary>
    /// Serializes a <see cref="Result"/> to <see cref="IResult"/>.
    /// </summary>
    /// <param name="result">The <see cref="Result"/> that will be serialized.</param>
    /// <returns>An instance of <see cref="IResult"/>.</returns>
    IResult Serialize(Result result);
    /// <summary>
    /// Serializes a <see cref="Result"/> to <see cref="IResult"/>.
    /// </summary>
    /// <typeparam name="TValue">The value of the Result.</typeparam>
    /// <param name="result">The <see cref="Result"/> that will be serialized.</param>
    /// <returns>An instance of <see cref="IResult"/>.</returns>
    IResult Serialize<TValue>(Result<TValue> result);
    /// <summary>
    /// Whether the instance of <see cref="IResultSerializationStrategy"/>
    /// should handle the serialization of <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The <see cref="Result"/> to be evaluated.</param>
    /// <returns>true if the <see cref="Result"/> should be serialized; false, otherwise.</returns>
    bool ShouldSerialize(Result result);
    /// <summary>
    /// Whether the instance of <see cref="IResultSerializationStrategy"/>
    /// should handle the serialization of <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TValue">The value of the Result.</typeparam>
    /// <param name="result">The <see cref="Result"/> to be evaluated.</param>
    /// <returns>true if the <see cref="Result"/> should be serialized; false, otherwise.</returns>
    bool ShouldSerialize<TValue>(Result<TValue> result);
}

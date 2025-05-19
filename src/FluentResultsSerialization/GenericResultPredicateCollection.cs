using FluentResults;

namespace FluentResultsSerialization;
/// <summary>
/// A collection that stores and manages predicates for a specific type of <see cref="Result{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type parameter of the <see cref="Result{TValue}"/> that the predicates handle.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="GenericResultPredicateCollection{TValue}"/> class is designed to manage a set of predicates
/// that operate on a specific type of <see cref="Result{TValue}"/>. This class provides a straightforward
/// approach to adding and checking predicates that evaluate a <see cref="Result{TValue}"/> based on
/// certain conditions. 
/// </para>
/// <para>
/// Key benefits of using this class include:
/// <list type="bullet">
///   <item>Type Safety: Ensures that predicates are only applied to the specific <see cref="Result{TValue}"/> type they are designed to handle.</item>
///   <item>Simplicity: Provides a simple, focused way to manage predicates for a single result type without the complexity of handling multiple types in the same collection.</item>
///   <item>Efficiency: Reduces overhead by only storing predicates relevant to the specific result type.</item>
/// </list>
/// </para>
/// </remarks>
internal sealed class GenericResultPredicateCollection<TValue>
{
    private readonly List<Func<Result<TValue>, bool>> _predicates = new();

    /// <summary>
    /// Adds a predicate to the collection for the specified <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="predicate">The predicate function to add, which evaluates a <see cref="Result{TValue}"/> and returns a boolean indicating whether a condition is met.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="predicate"/> is null.</exception>
    /// <remarks>
    /// Predicates are used to evaluate results and determine if they match certain conditions.
    /// </remarks>
    public void Add(Func<Result<TValue>, bool> predicate)
    {
        _predicates.Add(predicate);
    }

    /// <summary>
    /// Determines whether any of the stored predicates match the given <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="result">The result to evaluate using the stored predicates.</param>
    /// <returns><c>true</c> if any predicate matches the <paramref name="result"/>; otherwise, <c>false</c>.</returns>
    public bool Exists(Result<TValue> result)
    {
        return _predicates.Exists(predicate => predicate(result));
    }
}

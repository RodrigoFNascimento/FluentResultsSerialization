using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Execution;
using FluentResults.HttpMapping.Rules;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// Root DSL builder used to configure HTTP result mapping rules.
///
/// <para>
/// <see cref="HttpResultMappingBuilder"/> is the main entry point for
/// configuring how <see cref="FluentResults.Result"/> and
/// <see cref="FluentResults.Result{T}"/> instances are translated into
/// HTTP responses.
/// </para>
///
/// <para>
/// Rules are evaluated in the order they are defined. The first rule
/// whose predicate matches the result will be used to produce the
/// HTTP response.
/// </para>
/// </summary>
public sealed partial class HttpResultMappingBuilder
{
    private readonly List<IHttpMappingRule> _rules = new();

    /// <summary>
    /// Starts a new rule definition using a custom predicate.
    /// </summary>
    /// <param name="predicate">
    /// A function that determines whether the rule applies to the
    /// current <see cref="HttpResultMappingContext"/>.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to configure how the matched
    /// result is mapped to an HTTP response.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> is <c>null</c>.
    /// </exception>
    public RuleBuilder When(Func<HttpResultMappingContext, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return new RuleBuilder(predicate, rule => _rules.Add(rule));
    }

    /// <summary>
    /// Starts a rule that matches when the result contains at least one
    /// error of type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TError">
    /// The error type to match.
    /// </typeparam>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched error.
    /// </returns>
    public RuleBuilder WhenError<TError>()
        where TError : IError
    {
        return WhenError<TError>(_ => true);
    }

    /// <summary>
    /// Starts a rule that matches when the result contains at least one
    /// error of type <typeparamref name="TError"/> that satisfies
    /// the given predicate.
    /// </summary>
    /// <typeparam name="TError">
    /// The error type to match.
    /// </typeparam>
    /// <param name="predicate">
    /// A predicate used to further filter errors of type
    /// <typeparamref name="TError"/>.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched error.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> is <c>null</c>.
    /// </exception>
    public RuleBuilder WhenError<TError>(
        Func<TError, bool> predicate)
        where TError : IError
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return WhenFailure(error =>
            error is TError typedError &&
            predicate(typedError));
    }

    /// <summary>
    /// Starts a rule that matches when the result contains an error of type
    /// <typeparamref name="TError"/> with the specified metadata key.
    /// </summary>
    /// <typeparam name="TError">
    /// The error type to match.
    /// </typeparam>
    /// <param name="key">
    /// The metadata key that must be present on the error.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched error.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is <c>null</c>, empty,
    /// or consists only of whitespace.
    /// </exception>
    public RuleBuilder WhenErrorWithMetadata<TError>(string key)
        where TError : IError
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        return WhenError<TError>(e =>
            e.Metadata.ContainsKey(key));
    }

    /// <summary>
    /// Starts a rule that matches when the result contains an error of type
    /// <typeparamref name="TError"/> with the specified metadata key
    /// and value.
    /// </summary>
    /// <typeparam name="TError">
    /// The error type to match.
    /// </typeparam>
    /// <param name="key">
    /// The metadata key that must be present on the error.
    /// </param>
    /// <param name="value">
    /// The expected metadata value.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched error.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is <c>null</c>, empty,
    /// or consists only of whitespace.
    /// </exception>
    public RuleBuilder WhenErrorWithMetadata<TError>(
        string key,
        string value)
        where TError : IError
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        return WhenError<TError>(e =>
            e.Metadata.TryGetValue(key, out var metadataValue) &&
            string.Equals(
                metadataValue?.ToString(),
                value,
                StringComparison.Ordinal));
    }

    /// <summary>
    /// Starts a rule that matches when the result contains an error of type
    /// <typeparamref name="TError"/> whose metadata value satisfies
    /// the given predicate.
    /// </summary>
    /// <typeparam name="TError">
    /// The error type to match.
    /// </typeparam>
    /// <param name="key">
    /// The metadata key to inspect.
    /// </param>
    /// <param name="predicate">
    /// A predicate used to evaluate the metadata value.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched error.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="key"/> is <c>null</c>, empty,
    /// or consists only of whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> is <c>null</c>.
    /// </exception>
    public RuleBuilder WhenErrorWithMetadata<TError>(
        string key,
        Func<object?, bool> predicate)
        where TError : IError
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return WhenError<TError>(e =>
            e.Metadata.TryGetValue(key, out var metadataValue) &&
            predicate(metadataValue));
    }

    /// <summary>
    /// Starts a rule that matches any failed result.
    /// </summary>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for failed results.
    /// </returns>
    public RuleBuilder WhenFailure()
        => When(ctx => ctx.Result.IsFailed);

    /// <summary>
    /// Starts a rule that matches a failed result when at least one
    /// error satisfies the given predicate.
    /// </summary>
    /// <param name="predicate">
    /// A predicate used to evaluate individual errors.
    /// </param>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for the matched failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> is <c>null</c>.
    /// </exception>
    public RuleBuilder WhenFailure(Func<IError, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return When(ctx =>
            !ctx.Result.IsSuccess &&
            ctx.Result.Errors.Any(predicate));
    }

    /// <summary>
    /// Starts a rule that matches successful results.
    /// </summary>
    /// <returns>
    /// A <see cref="RuleBuilder"/> used to define the HTTP mapping
    /// for successful results.
    /// </returns>
    public RuleBuilder WhenSuccess()
    {
        return When(ctx => ctx.Result.IsSuccess);
    }

    /// <summary>
    /// Builds the immutable rule set from the configured rules.
    /// </summary>
    /// <returns>
    /// An <see cref="IHttpMappingRuleSet"/> containing all configured
    /// mapping rules in the order they were defined.
    /// </returns>
    internal IHttpMappingRuleSet Build()
        => new HttpMappingRuleSet(_rules);
}

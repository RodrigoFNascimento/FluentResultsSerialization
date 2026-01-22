using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Execution;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Routing;

namespace FluentResults.HttpMapping.DSL;

/// <summary>
/// Root DSL builder used to configure HTTP result mapping rules.
/// </summary>
public sealed partial class HttpResultMappingBuilder
{
    private readonly List<IHttpMappingRule> _rules = new();

    /// <summary>
    /// Starts a new rule definition.
    /// </summary>
    public RuleBuilder When(Func<HttpResultMappingContext, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return new RuleBuilder(predicate, rule => _rules.Add(rule));
    }

    /// <summary>
    /// Starts a rule that matches when the result contains
    /// at least one error of type <typeparamref name="TError"/>.
    /// </summary>
    public RuleBuilder WhenError<TError>()
        where TError : IError
    {
        return WhenError<TError>(_ => true);
    }

    /// <summary>
    /// Starts a rule that matches when the result contains
    /// at least one error of type <typeparamref name="TError"/>
    /// matching the given predicate.
    /// </summary>
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
    /// Matches when the result contains an error of type
    /// <typeparamref name="TError"/> with the given metadata key.
    /// </summary>
    public RuleBuilder WhenErrorWithMetadata<TError>(string key)
        where TError : IError
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(key));

        return WhenError<TError>(e =>
            e.Metadata.ContainsKey(key));
    }

    /// <summary>
    /// Matches when the result contains an error of type
    /// <typeparamref name="TError"/> with the given metadata key and value.
    /// </summary>
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
    /// Matches when the result contains an error of type
    /// <typeparamref name="TError"/> whose metadata value
    /// satisfies the given predicate.
    /// </summary>
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
    public RuleBuilder WhenFailure()
        => When(ctx => ctx.Result.IsFailed);

    /// <summary>
    /// Starts a rule that matches a failed result
    /// when any error satisfies the given predicate.
    /// </summary>
    public RuleBuilder WhenFailure(Func<IError, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return When(ctx =>
            !ctx.Result.IsSuccess &&
            ctx.Result.Errors.Any(predicate));
    }

    /// <summary>
    /// Adds a rule that matches successful results.
    /// </summary>
    public RuleBuilder WhenSuccess()
    {
        return When(ctx => ctx.Result.IsSuccess);
    }

    /// <summary>
    /// Builds the configured rule set.
    /// </summary>
    internal IHttpMappingRuleSet Build()
        => new HttpMappingRuleSet(_rules);
}

using FluentResults;
using FluentResultsSerialization.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace FluentResultsSerialization.Strategies;
internal sealed class ResultSerializationStrategy : IResultSerializationStrategy
{
    private readonly List<Type> _handledReasons;
    private readonly SerializationData _serializationData;
    private readonly SerializationPredicates _serializationPredicates;

    public ResultSerializationStrategy(
        SerializationData serializationData,
        SerializationPredicates serializationPredicates,
        List<Type> handledReasons)
    {
        _serializationData = serializationData;
        _serializationPredicates = serializationPredicates;
        _handledReasons = handledReasons;
    }

    public IResult Serialize(Result result)
    {
        GetHeaders(result);

        if (result.IsSuccess)
            return Results.StatusCode((int)_serializationData.Status)
                .WithHeaders(_serializationData.Headers);

        return SerializeFailedResult(result)
            .WithHeaders(_serializationData.Headers);
    }

    public IResult Serialize<TValue>(Result<TValue> result)
    {
        GetHeaders(result);

        if (result.IsSuccess)
        {
            if (!ShouldHaveResponseBody(_serializationData.Status))
                return Results.StatusCode((int)_serializationData.Status)
                    .WithHeaders(_serializationData.Headers);

            return Results.Json(
                    result.Value,
                    contentType: _serializationData.ContentType,
                    statusCode: (int)_serializationData.Status)
                .WithHeaders(_serializationData.Headers);
        }

        return SerializeFailedResult(result.ToResult())
            .WithHeaders(_serializationData.Headers);
    }

    public bool ShouldSerialize(Result result) =>
        result.Reasons.Exists(reason => _handledReasons.Contains(reason.GetType()))
        || _serializationPredicates.ResultPredicates.Exists(predicate => predicate(result))
        || _serializationPredicates.DefaultSuccessPredicates.Exists(predicate => predicate(result));

    public bool ShouldSerialize<TValue>(Result<TValue> result)
    {
        var key = typeof(TValue);
        if (_serializationPredicates.GenericResultPredicates.TryGetValue(key, out var predicates))
        {
            return ((GenericResultPredicateCollection<TValue>)predicates).Exists(result);
        }

        return result.Reasons.Exists(reason => _handledReasons.Contains(reason.GetType()))
            || _serializationPredicates.DefaultSuccessPredicates.Exists(predicate => predicate(result.ToResult()));
    }

    /// <summary>
    /// Processes the extension predicates and add the results to the extensions.
    /// </summary>
    /// <param name="result">The Result from which the extensions will be extracted.</param>
    private void GetExtensions(Result result)
    {
        foreach (var predicate in _serializationPredicates.ExtensionPredicates)
        {
            var value = predicate.Value(result);
            _serializationData.Extensions.Add(predicate.Key, value);
        }
    }

    /// <summary>
    /// Processes the header predicates and add the results to the headers.
    /// </summary>
    /// <param name="result">The Result from which the headers will be extracted.</param>
    private void GetHeaders(Result result)
    {
        foreach (var predicate in _serializationPredicates.HeaderPredicates)
        {
            var value = predicate.Value(result);
            if (!StringValues.IsNullOrEmpty(value))
                _serializationData.Headers.Add(predicate.Key, value);
        }
    }

    /// <summary>
    /// Processes the header predicates and add the results to the headers.
    /// </summary>
    /// <typeparam name="TValue">The value of the Result.</typeparam>
    /// <param name="result">The Result from which the headers will be extracted.</param>
    private void GetHeaders<TValue>(Result<TValue> result) =>
        GetHeaders(result.ToResult());

    /// <summary>
    /// Serializes a failed <see cref="Result"/> to <see cref="IResult"/>.
    /// </summary>
    /// <param name="result">Failed Result to be serialized.</param>
    /// <returns>The serialized <see cref="Result"/>.</returns>
    private IResult SerializeFailedResult(Result result)
    {
        GetExtensions(result);

        var validationErrors = _serializationPredicates.ValidationErrorsPredicate?.Invoke(result);

        if (validationErrors is not null)
            return Results.ValidationProblem(
                validationErrors,
                _serializationData.Detail ?? _serializationPredicates.DetailPredicate?.Invoke(result),
                _serializationData.Instance,
                (int)_serializationData.Status,
                _serializationData.Title,
                _serializationData.Type,
                _serializationData.Extensions);

        return Results.Problem(
            _serializationData.Detail ?? _serializationPredicates.DetailPredicate?.Invoke(result),
            _serializationData.Instance,
            (int)_serializationData.Status,
            _serializationData.Title,
            _serializationData.Type,
            _serializationData.Extensions);
    }

    /// <summary>
    /// Returns whether a HTTP response of Status Code <paramref name="httpStatusCode"/>
    /// should have a body according to
    /// <see href="https://www.rfc-editor.org/rfc/rfc7230#section-3.3">[RFC7230, Section 3.3]</see>.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP Status Code of the response.</param>
    /// <returns>Whether the response should have a body.</returns>
    private static bool ShouldHaveResponseBody(HttpStatusCode httpStatusCode) => httpStatusCode switch
    {
        _ when httpStatusCode < HttpStatusCode.OK => false,
        HttpStatusCode.NoContent or HttpStatusCode.NotModified => false,
        _ => true
    };
}

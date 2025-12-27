using FluentResults;
using FluentResultsSerialization.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace FluentResultsSerialization.Strategies;
internal sealed class ResultSerializationStrategy : IResultSerializationStrategy
{
    private const string ValidationErrorsKey = "ValidationErrors";
    internal string _contentType = string.Empty;
    internal string _title = string.Empty;
    internal string _type = string.Empty;
    internal string _detail = string.Empty;
    internal string _instance = string.Empty;
    internal HttpStatusCode _status;
    internal Dictionary<string, object?> _extensions = new();
    internal Dictionary<string, StringValues> _headers = new();
    internal List<Type> _handledReasons = Array.Empty<Type>().ToList();
    internal List<Func<Result, bool>> _resultPredicates = new();
    internal Dictionary<Type, object> _genericResultPredicates = new();
    internal List<Func<Result, bool>> _defaultSuccessPredicates = new();
    internal Dictionary<string, Func<Result, StringValues>> _headerPredicates = new();
    internal Dictionary<string, Func<Result, object?>> _extensionPredicates = new();

    public IResult Serialize(Result result)
    {
        GetHeaders(result);

        if (result.IsSuccess)
            return Results.StatusCode((int)_status)
                .WithHeaders(_headers);

        return SerializeFailedResult(result)
            .WithHeaders(_headers);
    }

    public IResult Serialize<TValue>(Result<TValue> result)
    {
        GetHeaders(result);

        if (result.IsSuccess)
        {
            if (!ShouldHaveResponseBody(_status))
                return Results.StatusCode((int)_status)
                    .WithHeaders(_headers);

            return Results.Json(result.Value, contentType: _contentType, statusCode: (int)_status)
                .WithHeaders(_headers);
        }

        return SerializeFailedResult(result.ToResult())
            .WithHeaders(_headers);
    }

    public bool ShouldSerialize(Result result) =>
        result.Reasons.Exists(reason => _handledReasons.Contains(reason.GetType()))
        || _resultPredicates.Exists(predicate => predicate(result))
        || _defaultSuccessPredicates.Exists(predicate => predicate(result));

    public bool ShouldSerialize<TValue>(Result<TValue> result)
    {
        var key = typeof(TValue);
        if (_genericResultPredicates.TryGetValue(key, out var predicates))
        {
            return ((GenericResultPredicateCollection<TValue>)predicates).Exists(result);
        }

        return result.Reasons.Exists(reason => _handledReasons.Contains(reason.GetType()))
            || _defaultSuccessPredicates.Exists(predicate => predicate(result.ToResult()));
    }

    /// <summary>
    /// Processes the extension predicates and add the results to the extensions.
    /// </summary>
    /// <param name="result">The Result from which the extensions will be extracted.</param>
    private void GetExtensions(Result result)
    {
        foreach (var predicate in _extensionPredicates)
        {
            var value = predicate.Value(result);
            _extensions.Add(predicate.Key, value);
        }
    }

    /// <summary>
    /// Processes the header predicates and add the results to the headers.
    /// </summary>
    /// <param name="result">The Result from which the headers will be extracted.</param>
    private void GetHeaders(Result result)
    {
        foreach (var predicate in _headerPredicates)
        {
            var value = predicate.Value(result);
            if (!StringValues.IsNullOrEmpty(value))
                _headers.Add(predicate.Key, value);
        }
    }

    /// <summary>
    /// Processes the header predicates and add the results to the headers.
    /// </summary>
    /// <typeparam name="TValue">The value of the Result.</typeparam>
    /// <param name="result">The Result from which the headers will be extracted.</param>
    private void GetHeaders<TValue>(Result<TValue> result) =>
        GetHeaders(result.ToResult());

    private IResult SerializeFailedResult(Result result)
    {
        string detail;

        if (!string.IsNullOrWhiteSpace(_detail))
            detail = _detail;
        else
            detail = result.Errors.FirstOrDefault(x => _handledReasons.Contains(x.GetType()))?.Message
                ?? result.Errors.FirstOrDefault()?.Message
                ?? string.Empty;

        GetExtensions(result);

        var validationErrors = result
            .Errors.FirstOrDefault(x => x.Metadata.TryGetValue(ValidationErrorsKey, out var metadata) && metadata is IDictionary<string, string[]>)
            ?.Metadata[ValidationErrorsKey] as IDictionary<string, string[]>;

        if (validationErrors is not null)
            return Results.ValidationProblem(validationErrors, detail, _instance, (int)_status, _title, _type);

        return Results.Problem(detail, _instance, (int)_status, _title, _type, _extensions);
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

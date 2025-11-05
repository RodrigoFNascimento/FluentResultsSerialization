using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    internal bool _showReasons;
    internal List<Type> _handledReasons = Array.Empty<Type>().ToList();
    internal List<Func<Result, bool>> _resultPredicates = new();
    internal Dictionary<Type, object> _genericResultPredicates = new();
    internal List<Func<Result, bool>> _defaultSuccessPredicates = new();

    public IResult Serialize(Result result)
    {
        if (result.IsSuccess)
            return Results.StatusCode((int)_status);

        return SerializeFailedResult(result);
    }

    public IResult Serialize<TValue>(Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            if (!ShouldHaveResponseBody(_status))
                return Results.StatusCode((int)_status);

            return Results.Json(result.Value, contentType: _contentType, statusCode: (int)_status);
        }

        return SerializeFailedResult(result.ToResult());
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

    private IResult SerializeFailedResult(Result result)
    {
        string detail;

        if (!string.IsNullOrWhiteSpace(_detail))
            detail = _detail;
        else
            detail = result.Errors.FirstOrDefault(x => _handledReasons.Contains(x.GetType()))?.Message
                ?? result.Errors.FirstOrDefault()?.Message
                ?? string.Empty;

        var problemDetails = new ProblemDetails();

        var validationErrors = result
            .Errors.FirstOrDefault(x => x.Metadata.TryGetValue(ValidationErrorsKey, out var metadata) && metadata is IDictionary<string, string[]>)
            ?.Metadata[ValidationErrorsKey] as IDictionary<string, string[]>;

        if (validationErrors is not null)
            problemDetails = new ValidationProblemDetails(validationErrors!);

        problemDetails.Type = _type;
        problemDetails.Title = _title;
        problemDetails.Detail = detail;
        problemDetails.Status = (int)_status;

        /* By adding the actual Reasons, the nested IReason doesn't get added to the JSON,
         * since IReason does not have a Reasons property. Since this method serializes
         * failed Result, we assume that the Errors are what really matter and add them instead,
         * since they do contain a Reasons property that is therefore added to the JSON.
         */
        if (_showReasons)
            problemDetails.Extensions.Add(nameof(result.Reasons).ToLower(), result.Errors);

        return Results.Json(problemDetails, contentType: _contentType, statusCode: (int)_status);
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

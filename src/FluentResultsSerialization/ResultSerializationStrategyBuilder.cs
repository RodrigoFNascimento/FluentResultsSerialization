using FluentResults;
using FluentResultsSerialization.Generators;
using FluentResultsSerialization.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

namespace FluentResultsSerialization;

/// <summary>
/// Builds instances of <see cref="ResultSerializationStrategy"/>.
/// </summary>
public class ResultSerializationStrategyBuilder
{
    private readonly List<Type> _handledReasons = new();
    private readonly List<Func<Result, bool>> _resultPredicates = new();
    private readonly Dictionary<Type, object> _genericResultPredicates = new();
    private readonly List<Func<Result, bool>> _defaultSuccessPredicates = new();

    private string _contentType = MediaTypeNames.Application.Json;
    private string _title = string.Empty;
    private string _type = TypeGenerator.DefaultType;
    private string _detail = string.Empty;
    private string _instance = string.Empty;
    private HttpStatusCode _status;
    private bool _showReasons;

    /// <summary>
    /// Indicates which <see cref="IReason"/> the strategy should handle.
    /// </summary>
    /// <typeparam name="TReason">The type of <see cref="IReason"/> that should be handled.</typeparam>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    public ResultSerializationStrategyBuilder Handle<TReason>() where TReason : IReason
    {
        _handledReasons.Add(typeof(TReason));
        return this;
    }

    /// <summary>
    /// Indicates which <see cref="Result"/> the strategy should handle.
    /// </summary>
    /// <param name="predicate">The logic used to determine whether the strategy can handle a <see cref="Result"/>.</param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public ResultSerializationStrategyBuilder Handle(Func<Result, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _resultPredicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Indicates which <see cref="Result"/> the strategy should handle.
    /// </summary>
    /// <typeparam name="TValue">The value of <see cref="Result{TValue}"/>.</typeparam>
    /// <param name="predicate">The logic used to determine whether the strategy can handle a <see cref="Result"/>.</param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public ResultSerializationStrategyBuilder Handle<TValue>(Func<Result<TValue>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var key = typeof(TValue);
        if (!_genericResultPredicates.TryGetValue(key, out var predicates))
        {
            var newCollection = new GenericResultPredicateCollection<TValue>();
            _genericResultPredicates[key] = newCollection;
            predicates = newCollection;
        }

        ((GenericResultPredicateCollection<TValue>)predicates).Add(predicate);
        return this;
    }

    /// <summary>
    /// Indicates that the strategy should handle any success
    /// <see cref="Result"/> or <see cref="Result{TValue}"/>.
    /// </summary>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    public ResultSerializationStrategyBuilder HandleSuccess()
    {
        _defaultSuccessPredicates.Add(result => result.IsSuccess);
        return this;
    }

    /// <summary>
    /// Whether the extension members of the <see cref="ProblemDetails"/>
    /// should contain the data from all the <see cref="IReason"/>
    /// of the <see cref="Result"/>.
    /// </summary>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    public ResultSerializationStrategyBuilder ShowReasons()
    {
        _showReasons = true;
        return this;
    }

    /// <summary>
    /// Sets the value of the Content-Type header of the <see cref="IResult"/>.
    /// </summary>
    /// <remarks>When this method is not used, the default content-type is application/json.</remarks>
    /// <param name="contentType">The media type of the returned data.</param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="contentType"/> is null or whitespace.</exception>
    public ResultSerializationStrategyBuilder WithContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException(LocalizationHelper.GetMessage("InvalidContentType"), nameof(contentType));

        _contentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the value of the "detail" member of the Problem Details object.
    /// </summary>
    /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="detail"/> is null or whitespace.</exception>
    public ResultSerializationStrategyBuilder WithDetail(string detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
            throw new ArgumentException(LocalizationHelper.GetMessage("InvalidDetail"), nameof(detail));

        _detail = detail;
        return this;
    }

    /// <summary>
    /// Sets the value of the "instance" member of the Problem Details object.
    /// </summary>
    /// <param name="instance">
    /// A URI reference that identifies the specific occurrence of the problem.
    /// It may or may not yield further information if dereferenced.
    /// </param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is null or whitespace.</exception>
    public ResultSerializationStrategyBuilder WithInstance(string instance)
    {
        if (string.IsNullOrWhiteSpace(instance))
            throw new ArgumentException(LocalizationHelper.GetMessage("InvalidInstance"), nameof(instance));

        _instance = instance;
        return this;
    }

    /// <summary>
    /// Sets the HTTP Status Code of the <see cref="IResult"/>.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> of <see cref="IResult"/></param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    public ResultSerializationStrategyBuilder WithStatus(HttpStatusCode statusCode)
    {
        _status = statusCode;
        return this;
    }

    /// <summary>
    /// Sets the value of the "title" member of the Problem Details object.
    /// </summary>
    /// <param name="title">
    /// A short, human-readable summary of the problem
    /// type. It SHOULD NOT change from occurrence to occurrence of the
    /// problem, except for purposes of localization(e.g., using
    /// proactive content negotiation;
    /// see <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-3.4">[RFC7231], Section 3.4</see>).
    /// </param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is null or whitespace.</exception>
    public ResultSerializationStrategyBuilder WithTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(LocalizationHelper.GetMessage("InvalidTitle"), nameof(title));

        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the value of the "type" member of the Problem Details object.
    /// </summary>
    /// <param name="type">
    /// A URI reference <see href="https://datatracker.ietf.org/doc/html/rfc3986">[RFC3986]</see> that identifies the
    /// problem type. This specification encourages that, when
    /// dereferenced, a human-readable documentation for the problem type is provided
    /// (e.g., using HTML <see href="https://datatracker.ietf.org/doc/html/rfc7807#ref-W3C.REC-html5-20141028">[W3C.REC-html5-20141028]</see>).
    /// When this member is not present, its value is assumed to be "about:blank".
    /// </param>
    /// <returns>The instance of <see cref="ResultSerializationStrategyBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is null or whitespace.</exception>
    public ResultSerializationStrategyBuilder WithType(string type)
    {
        if (!string.IsNullOrWhiteSpace(type))
            _type = type;

        return this;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ResultSerializationStrategy"/>
    /// with the provided configuration.
    /// </summary>
    /// <returns>A new instance of <see cref="ResultSerializationStrategy"/>.</returns>
    internal ResultSerializationStrategy Build()
    {
        if (_type == TypeGenerator.DefaultType)
            _type = TypeGenerator.Generate(_status);

        /*
         * When "about:blank" is used, the title SHOULD be the same as the
         * recommended HTTP status phrase for that code (e.g., "Not Found" for
         * 404, and so on), although it MAY be localized to suit client
         * preferences (expressed with the Accept-Language request header).
         */
        if (_type == TypeGenerator.DefaultType)
            _title = ReasonPhraseGenerator.Generate(_status);

        var strategy = new ResultSerializationStrategy()
        {
            _contentType = _contentType,
            _title = _title,
            _type = _type,
            _detail = _detail,
            _instance = _instance,
            _status = _status,
            _showReasons = _showReasons,
            _resultPredicates = _resultPredicates,
            _genericResultPredicates = _genericResultPredicates,
            _defaultSuccessPredicates = _defaultSuccessPredicates,
            _handledReasons = _handledReasons
        };

        return strategy;
    }
}

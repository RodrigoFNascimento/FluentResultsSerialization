using System.Net;

namespace FluentResultsSerialization.Generators;
/// <summary>
/// Generates values for the "type" member of problem details objects, which are used in HTTP error responses
/// to convey machine-readable descriptions of error types.
/// </summary>
internal static class TypeGenerator
{
    /// <summary>
    /// <para>
    /// The "about:blank" URI [<see href="https://datatracker.ietf.org/doc/html/rfc6694">RFC6694</see>],
    /// when used as a problem type, indicates that the problem has no additional semantics beyond that of the HTTP status code.
    /// </para>
    /// <para>
    /// Please note that according to how the "type" member is defined
    /// (<see href="https://datatracker.ietf.org/doc/html/rfc7807#section-3.1">Section 3.1</see>),
    /// the "about:blank" URI is the default value for that member.Consequently,
    /// any problem details object not carrying an explicit "type" member implicitly uses this URI.
    /// </para>
    /// </summary>
    public const string DefaultType = "about:blank";

    /// <summary>
    /// Generates the value of problem details member "type" based on a HTTP Status Code.
    /// </summary>
    /// <remarks>
    /// If the HTTP Status Code is not present in the
    /// <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-6">[RFC7231, Section 6]</see>
    /// the default value for the member, "about:blank", will be returned.
    /// </remarks>
    /// <param name="httpStatusCode">HTTP Status Code.</param>
    /// <returns>The value of member "type".</returns>
    public static string Generate(HttpStatusCode httpStatusCode) => httpStatusCode switch
    {
        HttpStatusCode.Continue => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.2.1",
        HttpStatusCode.SwitchingProtocols => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.2.2",
        HttpStatusCode.OK => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.1",
        HttpStatusCode.Created => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.2",
        HttpStatusCode.Accepted => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.3",
        HttpStatusCode.NonAuthoritativeInformation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.4",
        HttpStatusCode.NoContent => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.5",
        HttpStatusCode.ResetContent => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.3.6",
        HttpStatusCode.PartialContent => "https://datatracker.ietf.org/doc/html/rfc7233#section-4.1",
        HttpStatusCode.MultipleChoices => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.1",
        HttpStatusCode.MovedPermanently => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.2",
        HttpStatusCode.Found => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.3",
        HttpStatusCode.SeeOther => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.4",
        HttpStatusCode.NotModified => "https://datatracker.ietf.org/doc/html/rfc7232#section-4.1",
        HttpStatusCode.UseProxy => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.5",
        HttpStatusCode.TemporaryRedirect => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.4.7",
        HttpStatusCode.BadRequest => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
        HttpStatusCode.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
        HttpStatusCode.PaymentRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.2",
        HttpStatusCode.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
        HttpStatusCode.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
        HttpStatusCode.MethodNotAllowed => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.5",
        HttpStatusCode.NotAcceptable => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.6",
        HttpStatusCode.ProxyAuthenticationRequired => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.2",
        HttpStatusCode.RequestTimeout => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.7",
        HttpStatusCode.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
        HttpStatusCode.Gone => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.9",
        HttpStatusCode.LengthRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.10",
        HttpStatusCode.PreconditionFailed => "https://datatracker.ietf.org/doc/html/rfc7232#section-4.2",
        HttpStatusCode.RequestEntityTooLarge => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.11",
        HttpStatusCode.RequestUriTooLong => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.12",
        HttpStatusCode.UnsupportedMediaType => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13",
        HttpStatusCode.RequestedRangeNotSatisfiable => "https://datatracker.ietf.org/doc/html/rfc7233#section-4.4",
        HttpStatusCode.ExpectationFailed => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.14",
        HttpStatusCode.UpgradeRequired => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.15",
        HttpStatusCode.InternalServerError => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
        HttpStatusCode.NotImplemented => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.2",
        HttpStatusCode.BadGateway => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3",
        HttpStatusCode.ServiceUnavailable => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
        HttpStatusCode.GatewayTimeout => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.5",
        HttpStatusCode.HttpVersionNotSupported => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.6",
        _ => DefaultType
    };
}

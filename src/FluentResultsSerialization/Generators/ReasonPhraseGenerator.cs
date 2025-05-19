using System.Net;

namespace FluentResultsSerialization.Generators;
/// <summary>
/// Generates reason phrases corresponding to HTTP status codes as defined in 
/// <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-6.1">[RFC7231, Section 6.1]</see>.
/// </summary>
internal static class ReasonPhraseGenerator
{
    /// <summary>
    /// Generates reason-phrases based on
    /// <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-6.1">Section 6.1 of [RFC7231]</see>.
    /// </summary>
    /// <param name="httpStatusCode">HTTP Status Code.</param>
    /// <returns>The generated reason-phrase.</returns>
    public static string Generate(HttpStatusCode httpStatusCode) => httpStatusCode switch
    {
        HttpStatusCode.Continue => nameof(HttpStatusCode.Continue),
        HttpStatusCode.SwitchingProtocols => "Switching Protocols",
        HttpStatusCode.OK => nameof(HttpStatusCode.OK),
        HttpStatusCode.Created => nameof(HttpStatusCode.Created),
        HttpStatusCode.Accepted => nameof(HttpStatusCode.Accepted),
        HttpStatusCode.NonAuthoritativeInformation => "Non-Authoritative Information",
        HttpStatusCode.NoContent => "No Content",
        HttpStatusCode.ResetContent => "Reset Content",
        HttpStatusCode.PartialContent => "Partial Content",
        HttpStatusCode.MultipleChoices => "Multiple Choices",
        HttpStatusCode.MovedPermanently => "Moved Permanently",
        HttpStatusCode.Found => nameof(HttpStatusCode.Found),
        HttpStatusCode.SeeOther => "See Other",
        HttpStatusCode.NotModified => "Not Modified",
        HttpStatusCode.UseProxy => "Use Proxy",
        HttpStatusCode.TemporaryRedirect => "Temporary Redirect",
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => nameof(HttpStatusCode.Unauthorized),
        HttpStatusCode.PaymentRequired => "Payment Required",
        HttpStatusCode.Forbidden => nameof(HttpStatusCode.Forbidden),
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
        HttpStatusCode.NotAcceptable => "Not Acceptable",
        HttpStatusCode.ProxyAuthenticationRequired => "Proxy Authentication Required",
        HttpStatusCode.RequestTimeout => "Request Timeout",
        HttpStatusCode.Conflict => nameof(HttpStatusCode.Conflict),
        HttpStatusCode.Gone => nameof(HttpStatusCode.Gone),
        HttpStatusCode.LengthRequired => "Length Required",
        HttpStatusCode.PreconditionFailed => "Precondition Failed",
        HttpStatusCode.RequestEntityTooLarge => "Payload Too Large",
        HttpStatusCode.RequestUriTooLong => "URI Too Long",
        HttpStatusCode.UnsupportedMediaType => "Unsupported Media Type",
        HttpStatusCode.RequestedRangeNotSatisfiable => "Range Not Satisfiable",
        HttpStatusCode.ExpectationFailed => "Expectation Failed",
        HttpStatusCode.UpgradeRequired => "Upgrade Required",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        HttpStatusCode.NotImplemented => "Not Implemented",
        HttpStatusCode.BadGateway => "Bad Gateway",
        HttpStatusCode.ServiceUnavailable => "Service Unavailable",
        HttpStatusCode.GatewayTimeout => "Gateway Timeout",
        HttpStatusCode.HttpVersionNotSupported => "HTTP Version Not Supported",
        _ => "Internal Server Error"
    };
}

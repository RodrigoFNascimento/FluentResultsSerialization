namespace FluentResults.HttpMapping.Context;

/// <summary>
/// Describes an HTTP response header produced by an HTTP mapping rule.
///
/// <para>
/// A <see cref="HeaderDescriptor"/> is a declarative description of a header,
/// not the header itself. The header value is computed lazily at execution time
/// using the current <see cref="HttpResultMappingContext"/>.
/// </para>
/// </summary>
internal sealed class HeaderDescriptor
{
    /// <summary>
    /// Gets the HTTP header name.
    /// </summary>
    /// <remarks>
    /// Header names are treated as case-insensitive when applied to the
    /// HTTP response.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Gets the function used to compute the header value from the
    /// current mapping context.
    /// </summary>
    /// <remarks>
    /// The returned value may be <c>null</c>, in which case the header
    /// will not be added to the response.
    /// </remarks>
    public Func<HttpResultMappingContext, string?> ValueFactory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderDescriptor"/> class.
    /// </summary>
    /// <param name="name">
    /// The HTTP header name.
    /// </param>
    /// <param name="valueFactory">
    /// A function that produces the header value based on the
    /// <see cref="HttpResultMappingContext"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> or <paramref name="valueFactory"/>
    /// is <c>null</c>.
    /// </exception>
    public HeaderDescriptor(
        string name,
        Func<HttpResultMappingContext, string?> valueFactory)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
    }
}

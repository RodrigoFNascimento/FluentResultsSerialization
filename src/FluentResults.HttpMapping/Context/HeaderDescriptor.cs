namespace FluentResults.HttpMapping.Context;

/// <summary>
/// Describes an HTTP header to be produced by a mapping rule.
/// </summary>
internal sealed class HeaderDescriptor
{
    /// <summary>
    /// The HTTP header name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Function that computes the header value from the mapping context.
    /// </summary>
    public Func<HttpResultMappingContext, string?> ValueFactory { get; }

    /// <summary>
    /// Creates a new header descriptor.
    /// </summary>
    public HeaderDescriptor(
        string name,
        Func<HttpResultMappingContext, string?> valueFactory)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
    }
}

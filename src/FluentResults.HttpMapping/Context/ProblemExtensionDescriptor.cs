namespace FluentResults.HttpMapping.Context;

/// <summary>
/// Describes a Problem Details extension member to be added
/// to an RFC 7807 response.
/// </summary>
/// <param name="Name">The name of the Problem Details extension member.</param>
/// <param name="ValueFactory">A function that computes the extension value from the mapping context.</param>
internal sealed record ProblemExtensionDescriptor(
    string Name,
    Func<HttpResultMappingContext, object?> ValueFactory
);

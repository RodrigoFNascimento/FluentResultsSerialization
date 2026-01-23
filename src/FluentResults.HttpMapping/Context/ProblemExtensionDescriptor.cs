namespace FluentResults.HttpMapping.Context;

internal sealed record ProblemExtensionDescriptor(
    string Name,
    Func<HttpResultMappingContext, object?> ValueFactory);


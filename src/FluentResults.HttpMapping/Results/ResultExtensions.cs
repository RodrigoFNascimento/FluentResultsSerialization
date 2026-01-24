using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FluentResults.HttpMapping.Results;

internal static class ResultExtensions
{
    /// <summary>
    /// Adds headers to a Result.
    /// </summary>
    /// <param name="result">Result the headers will be added to.</param>
    /// <param name="headers">Headers that will be added to the Result.</param>
    /// <returns>A Result with the added headers.</returns>
    public static IResult WithHeaders(
        this IResult result,
        IDictionary<string, StringValues> headers)
    {
        return new HeaderResult(result, headers);
    }
}

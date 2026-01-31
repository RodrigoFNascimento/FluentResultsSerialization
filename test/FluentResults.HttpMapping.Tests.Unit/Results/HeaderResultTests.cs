using FluentResults.HttpMapping.Results;
using Microsoft.Extensions.Primitives;

namespace FluentResults.HttpMapping.Tests.Unit.Results;

public sealed class HeaderResultTests
{
    [Fact]
    public async Task ExecuteAsync_WhenResultHasHeaders_ShouldAddHeadersToResponse()
    {
        // Arrange
        var innerResult = Microsoft.AspNetCore.Http.Results.Ok();
        var headers = new Dictionary<string, StringValues>() { { "a", "b" } };
        var headerResult = new HeaderResult(innerResult, headers);

        // Act
        var (httpContext, _) = await headerResult.ExecuteAsync();

        // Assert
        Assert.Equal(headers.First().Value, httpContext.Response.Headers[headers.First().Key]);
    }
}

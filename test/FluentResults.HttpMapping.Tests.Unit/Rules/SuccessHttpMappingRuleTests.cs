using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using System.Net;

namespace FluentResults.HttpMapping.Tests.Unit.Rules;

public sealed class SuccessHttpMappingRuleTests
{
    [Fact]
    public async Task Map_WhenResultHasValue_ShouldMapWithValue()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Ok(42));
        var sut = new SuccessHttpMappingRule();

        // Act
        var (context, body) = await sut.Map(httpResultMappingContext).ExecuteAsync();

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        Assert.NotEmpty(body);
    }
    
    [Fact]
    public async Task Map_WhenResultHasNoValue_ShouldMapWithoutValue()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Ok());
        var sut = new SuccessHttpMappingRule();

        // Act
        var (context, body) = await sut.Map(httpResultMappingContext).ExecuteAsync();

        // Assert
        Assert.Equal((int)HttpStatusCode.NoContent, context.Response.StatusCode);
        Assert.Empty(body);
    }
}

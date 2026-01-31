using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Execution;
using FluentResults.HttpMapping.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace FluentResults.HttpMapping.Tests.Unit.Execution;

public sealed class HttpMappingRuleSetTests
{
    [Fact]
    public void Execute_WhenNoMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Fail(string.Empty));

        var mappingRule = Substitute.For<IHttpMappingRule>();
        mappingRule
            .Matches(Arg.Any<HttpResultMappingContext>())
            .Returns(false);

        IEnumerable<IHttpMappingRule> rules = new List<IHttpMappingRule>() { mappingRule };

        var sut = new HttpMappingRuleSet(rules);

        // Act
        IResult throwingFunction() => sut.Execute(httpResultMappingContext);

        // Assert
        Assert.Throws<InvalidOperationException>(throwingFunction);
    }
    
    [Fact]
    public async Task Execute_WhenMatchRuleHasNoHeaders_ShouldNotAddHeaders()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Ok());

        var mappingRule = Substitute.For<IHttpMappingRule>();
        mappingRule
            .Matches(Arg.Any<HttpResultMappingContext>())
            .Returns(false);

        mappingRule.Headers
            .Returns(new List<HeaderDescriptor>());

        IEnumerable<IHttpMappingRule> rules = new List<IHttpMappingRule>() { mappingRule };

        var sut = new HttpMappingRuleSet(rules);

        // Act
        var httpContext = await sut.Execute(httpResultMappingContext).ExecuteResultAsync();

        // Assert
        Assert.Empty(httpContext.Response.Headers);
    }
    
    [Fact]
    public async Task Execute_WhenMatchRuleHasHeaders_ShouldAddHeaders()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Ok());
        var header = new KeyValuePair<string, StringValues>("a", "b");
        var expectedHeaders = new HeaderDictionary() { header };

        var mappingRule = Substitute.For<IHttpMappingRule>();
        mappingRule
            .Matches(Arg.Any<HttpResultMappingContext>())
            .Returns(true);
        
        mappingRule.Headers
            .Returns(expectedHeaders.Select(x => new HeaderDescriptor(x.Key, _ => x.Value)).ToList());

        var rules = new List<IHttpMappingRule>() { mappingRule };

        var sut = new HttpMappingRuleSet(rules);

        // Act
        var httpContext = await sut.Execute(httpResultMappingContext).ExecuteResultAsync();

        // Assert
        Assert.Equivalent(expectedHeaders, httpContext.Response.Headers);
    }
}

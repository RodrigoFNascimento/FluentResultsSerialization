using FluentResults.HttpMapping.Context;
using FluentResults.HttpMapping.Rules;
using System.Net;
using System.Text.Json;

namespace FluentResults.HttpMapping.Tests.Unit.Rules;

public sealed class ProblemHttpMappingRuleTests
{
    [Fact]
    public async Task Map_ShouldProduceProblem()
    {
        // Arrange
        var httpResultMappingContext = new HttpResultMappingContext(Result.Fail(string.Empty));
        var status = HttpStatusCode.OK;
        var title = "title";
        var detail = "detail";
        var extension = new KeyValuePair<string, string>("extension-key", "extension-value");

        var sut = new ProblemHttpMappingRule(
            _ => true,
            status,
            _ => title,
            _ => detail,
            new List<HeaderDescriptor>(),
            new List<ProblemExtensionDescriptor>() { new(extension.Key, _ => extension.Value) },
            "name");

        // Act
        var (context, body) = await sut.Map(httpResultMappingContext).ExecuteAsync();

        // Assert
        Assert.Equal((int)status, context.Response.StatusCode);
        var jsonBody = JsonDocument.Parse(body);
        Assert.Equal((int)status, jsonBody.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(title, jsonBody.RootElement.GetProperty("title").GetString());
        Assert.Equal(detail, jsonBody.RootElement.GetProperty("detail").GetString());
        Assert.Equal(extension.Value, jsonBody.RootElement.GetProperty(extension.Key).GetString());
    }
}

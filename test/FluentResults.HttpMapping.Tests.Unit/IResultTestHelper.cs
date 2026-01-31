using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentResults.HttpMapping.Tests.Unit;

internal static class IResultTestHelper
{
    public static async Task<(HttpContext Context, string Body)> ExecuteAsync(this IResult result)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();


        await result.ExecuteAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        return (context, body);
    }

    public static async Task<HttpContext> ExecuteResultAsync(this IResult result)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddRouting()
            .AddEndpointsApiExplorer()
            .BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = services
        };

        context.Response.Body = new MemoryStream();

        await result.ExecuteAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        return context;
    }
}

using FluentResults.HttpMapping.DSL;
using FluentResults.HttpMapping.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace FluentResults.HttpMapping;

/// <summary>
/// Service registration for HTTP result mapping.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers HTTP result mapping using a fluent DSL.
    /// </summary>
    public static IServiceCollection AddHttpResultMapping(
        this IServiceCollection services,
        Action<HttpResultMappingBuilder> configure)
    {
        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new HttpResultMappingBuilder();
        configure(builder);

        services.AddSingleton<IHttpMappingRuleSet>(builder.Build());
        services.AddSingleton<IHttpResultMapper, DefaultHttpResultMapper>();

        return services;
    }
}

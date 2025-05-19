using FluentResults;
using FluentResultsSerialization.Serializer;
using FluentResultsSerialization.Strategies;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace FluentResultsSerialization;
public static class DependencyInjection
{
    /// <summary>
    /// Injects an instance of <see cref="IResultSerializer"/> for handling Result serialization.
    /// </summary>
    /// <param name="services">The instance of <see cref="IServiceCollection"/> to be configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for further configuration.</returns>
    public static IServiceCollection AddResultSerializer(this IServiceCollection services)
    {
        services.AddTransient<IResultSerializer, ResultSerializer>();

        return services;
    }

    /// <summary>
    /// Injects default strategies for Result serialization.
    /// </summary>
    /// <remarks>
    /// The strategies are as follows:
    /// <list type="bullet">
    /// <item>
    /// Return HTTP Status Code 200 Ok
    /// when either a <see cref="Result"/> or <see cref="ResultBase{TValue}"/>
    /// is successful.
    /// </item>
    /// <item>
    /// Return HTTP Status Code 500 Internal Server Error
    /// when either a <see cref="Result"/> or <see cref="ResultBase{TValue}"/>
    /// has an <see cref="Error"/>.
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="services">The instance of <see cref="IServiceCollection"/> to be configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for further configuration.</returns>
    public static IServiceCollection AddResultSerializationStrategy(this IServiceCollection services)
    {
        services.AddResultSerializationStrategy(builder => builder
            .HandleSuccess()
            .WithStatus(HttpStatusCode.OK));

        services.AddResultSerializationStrategy(builder => builder
            .Handle<Error>()
            .WithTitle(LocalizationHelper.GetMessage("InternalErrorMessage"))
            .WithStatus(HttpStatusCode.InternalServerError)
            .WithContentType("application/problem+json"));

        return services;
    }

    /// <summary>
    /// Injects a strategy for Result serialization.
    /// </summary>
    /// <param name="services">The instance of <see cref="IServiceCollection"/> to be configured.</param>
    /// <param name="configure">The strategy configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for further configuration.</returns>
    public static IServiceCollection AddResultSerializationStrategy(
        this IServiceCollection services,
        Action<ResultSerializationStrategyBuilder> configure)
    {
        services.AddTransient<IResultSerializationStrategy>(_ =>
        {
            var instance = new ResultSerializationStrategyBuilder();
            configure(instance);
            return instance.Build();
        });

        return services;
    }
}

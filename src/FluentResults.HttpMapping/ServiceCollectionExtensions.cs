using FluentResults.HttpMapping.DSL;
using FluentResults.HttpMapping.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace FluentResults.HttpMapping;

/// <summary>
/// Extension methods for registering HTTP result mapping services.
/// </summary>
/// <remarks>
/// This class provides the primary integration point between
/// FluentResults.HttpMapping and ASP.NET Core dependency injection.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers HTTP result mapping using a fluent DSL configuration.
    /// </summary>
    /// <param name="services">
    /// The service collection to add the HTTP result mapping services to.
    /// </param>
    /// <param name="configure">
    /// A delegate used to configure HTTP mapping rules via
    /// <see cref="HttpResultMappingBuilder"/>.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, allowing
    /// further service registration chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This method registers:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A singleton <see cref="IHttpMappingRuleSet"/> built from the
    /// configured DSL rules.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A singleton <see cref="IHttpResultMapper"/> that endpoints can
    /// use to map <see cref="FluentResults.Result"/> values to HTTP responses.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Rules are evaluated in the order they are defined.
    /// The first matching rule produces the HTTP response.
    ///
    /// A default success rule is always appended automatically to handle
    /// successful results that are not explicitly mapped.
    /// </remarks>
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

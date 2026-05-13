using ELKStack.Observability.Correlation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ELKStack.Observability;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddElkStackObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddCorrelation();

        return builder;
    }

    public static IServiceCollection AddCorrelation(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped<CorrelationMiddleware>();
        services.AddTransient<CorrelationDelegatingHandler>();

        return services;
    }

    public static IHttpClientBuilder AddCorrelationHandler(this IHttpClientBuilder builder) =>
        builder.AddHttpMessageHandler<CorrelationDelegatingHandler>();

    public static void UseCorrelationFilters(this IBusFactoryConfigurator configurator, IBusRegistrationContext context)
    {
        configurator.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);
        configurator.UsePublishFilter(typeof(CorrelationPublishFilter<>), context);
    }

    public static WebApplication UseElkStackObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationMiddleware>();

        return app;
    }
}

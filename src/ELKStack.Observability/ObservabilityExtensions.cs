using ELKStack.Observability.Correlation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace ELKStack.Observability;

public static class ObservabilityExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static WebApplicationBuilder AddElkStackObservability(this WebApplicationBuilder builder)
    {
        builder.AddStructuredLogging();
        builder.AddOpenTelemetryExport();
        builder.Services.AddCorrelation();
        builder.Services.AddDefaultHealthChecks();
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders
                                    | HttpLoggingFields.ResponsePropertiesAndHeaders
                                    | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

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
        app.UseHttpLogging();
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("Host", httpContext.Request.Host.Value ?? string.Empty);
                diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
                diagnosticContext.Set("Scheme", httpContext.Request.Scheme);
                diagnosticContext.Set("Path", httpContext.Request.Path.Value ?? string.Empty);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? string.Empty);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.Items.TryGetValue(CorrelationHeaders.CorrelationIdKey, out var correlationId))
                {
                    diagnosticContext.Set(CorrelationHeaders.CorrelationIdKey, correlationId);
                }

                if (httpContext.Items.TryGetValue(CorrelationHeaders.CausationIdKey, out var causationId))
                {
                    diagnosticContext.Set(CorrelationHeaders.CausationIdKey, causationId?.ToString() ?? "root");
                }

                if (httpContext.Items.TryGetValue(CorrelationHeaders.OperationIdKey, out var operationId))
                {
                    diagnosticContext.Set(CorrelationHeaders.OperationIdKey, operationId);
                }
            };

            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                return httpContext.Request.Path.StartsWithSegments(HealthEndpointPath)
                       || httpContext.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information;
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });
        }

        return app;
    }

    private static void AddStructuredLogging(this WebApplicationBuilder builder)
    {
        var serviceName = GetServiceName(builder);
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        builder.Host.UseSerilog((context, _, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            {
                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["deployment.environment"] = context.HostingEnvironment.EnvironmentName
                    };
                });
            }
        });
    }

    private static void AddOpenTelemetryExport(this WebApplicationBuilder builder)
    {
        var serviceName = GetServiceName(builder);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
                    })
                    .AddHttpClientInstrumentation();
            });

        if (Uri.TryCreate(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"], UriKind.Absolute, out var otlpEndpoint))
        {
            openTelemetry.UseOtlpExporter(OtlpExportProtocol.Grpc, otlpEndpoint);
        }
    }

    private static void AddDefaultHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    private static string GetServiceName(WebApplicationBuilder builder) =>
        builder.Configuration["OTEL_SERVICE_NAME"]
        ?? builder.Configuration["Observability:ServiceName"]
        ?? builder.Environment.ApplicationName;
}

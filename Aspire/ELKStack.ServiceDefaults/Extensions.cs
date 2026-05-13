using ELKStack.Observability.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureSerilog();
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureSerilog<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var serviceName = GetServiceName(builder);
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        builder.Services.AddSerilog((_, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
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
                        ["deployment.environment"] = builder.Environment.EnvironmentName
                    };
                });
            }
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
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

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders
                                    | HttpLoggingFields.ResponsePropertiesAndHeaders
                                    | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.UseHttpLogging();
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = EnrichDiagnosticContext;
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

    private static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
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
    }

    private static string GetServiceName(IHostApplicationBuilder builder) =>
        builder.Configuration["OTEL_SERVICE_NAME"]
        ?? builder.Configuration["Observability:ServiceName"]
        ?? builder.Environment.ApplicationName;
}

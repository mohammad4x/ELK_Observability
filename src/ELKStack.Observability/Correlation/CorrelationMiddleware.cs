using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ELKStack.Observability.Correlation;

public sealed class CorrelationMiddleware(
    ICorrelationContextAccessor correlationAccessor,
    ILogger<CorrelationMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var correlationId = ExtractOrGenerateGuid(httpContext, CorrelationHeaders.CorrelationId)
            ?? ExtractOrGenerateGuid(httpContext, CorrelationHeaders.RequestId)
            ?? Guid.NewGuid();
        var causationId = ExtractOrGenerateGuid(httpContext, CorrelationHeaders.CausationId);

        var context = CorrelationContext.CreateForHttp(correlationId, causationId);
        correlationAccessor.Context = context;
        httpContext.Items[CorrelationHeaders.CorrelationIdKey] = context.CorrelationId;
        httpContext.Items[CorrelationHeaders.CausationIdKey] = context.CausationId;
        httpContext.Items[CorrelationHeaders.OperationIdKey] = context.OperationId;
        EnrichActivity(context);

        httpContext.Response.OnStarting(() =>
        {
            httpContext.Response.Headers[CorrelationHeaders.CorrelationId] = context.CorrelationId.ToString();
            httpContext.Response.Headers[CorrelationHeaders.RequestId] = context.OperationId.ToString();

            if (context.CausationId.HasValue)
            {
                httpContext.Response.Headers[CorrelationHeaders.CausationId] = context.CausationId.Value.ToString();
            }

            return Task.CompletedTask;
        });

        using (logger.BeginScope(CreateScope(context)))
        {
            logger.LogDebug(
                "HTTP request entered with correlation {CorrelationId} and causation {CausationId}",
                context.CorrelationId,
                context.CausationId);

            await next(httpContext);
        }
    }

    private static Guid? ExtractOrGenerateGuid(HttpContext httpContext, string headerName)
    {
        return httpContext.Request.Headers.TryGetValue(headerName, out var value)
               && Guid.TryParse(value.FirstOrDefault(), out var parsed)
            ? parsed
            : null;
    }

    private static void EnrichActivity(CorrelationContext context)
    {
        var activity = Activity.Current;
        activity?.SetTag("correlation.id", context.CorrelationId);
        activity?.SetTag("causation.id", context.CausationId);
        activity?.SetTag("operation.id", context.OperationId);
    }

    internal static Dictionary<string, object> CreateScope(CorrelationContext context) =>
        new()
        {
            [CorrelationHeaders.CorrelationIdKey] = context.CorrelationId,
            [CorrelationHeaders.CausationIdKey] = context.CausationId?.ToString() ?? "root",
            [CorrelationHeaders.OperationIdKey] = context.OperationId
        };
}

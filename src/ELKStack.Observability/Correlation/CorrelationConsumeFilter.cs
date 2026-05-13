using System.Diagnostics;
using ELKStack.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ELKStack.Observability.Correlation;

public sealed class CorrelationConsumeFilter<T>(
    ICorrelationContextAccessor correlationAccessor,
    ILogger<CorrelationConsumeFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (context.Message is not IIntegrationEvent message)
        {
            await next.Send(context);
            return;
        }

        var correlationContext = CorrelationContext.CreateForConsumedEvent(message);
        correlationAccessor.Context = correlationContext;
        EnrichActivity(correlationContext, message);

        using (logger.BeginScope(CorrelationMiddleware.CreateScope(correlationContext)))
        {
            logger.LogDebug(
                "Consuming {MessageType} with event {EventId}, correlation {CorrelationId}, causation {CausationId}",
                typeof(T).Name,
                message.EventId,
                message.CorrelationId,
                message.CausationId);

            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlation-consume");

    private static void EnrichActivity(CorrelationContext context, IIntegrationEvent message)
    {
        var activity = Activity.Current;
        activity?.SetTag("event.id", message.EventId);
        activity?.SetTag("correlation.id", context.CorrelationId);
        activity?.SetTag("causation.id", context.CausationId);
        activity?.SetTag("operation.id", context.OperationId);
    }
}

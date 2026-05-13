using ELKStack.Contracts;
using MassTransit;

namespace ELKStack.Observability.Correlation;

public sealed class CorrelationPublishFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (context.Message is IIntegrationEvent message)
        {
            context.CorrelationId = message.CorrelationId;
            context.Headers.Set(CorrelationHeaders.CorrelationId, message.CorrelationId.ToString());
            context.Headers.Set(CorrelationHeaders.CausationId, message.CausationId?.ToString() ?? "root");
            context.Headers.Set("EventId", message.EventId.ToString());
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("correlation-publish");
}

using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using ELKStack.PaymentService.State;
using MassTransit;

namespace ELKStack.PaymentService.Consumers;

public sealed class PaymentRequestedConsumer(
    PaymentState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    ILogger<PaymentRequestedConsumer> logger) : IConsumer<PaymentRequested>
{
    public async Task Consume(ConsumeContext<PaymentRequested> context)
    {
        var transactionId = $"TX-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var payment = state.MarkCompleted(context.Message, transactionId);

        var metadata = correlationContext.CreateMetadata();
        var message = new PaymentCompleted(
            payment.PaymentId,
            payment.BookingId,
            payment.Amount,
            payment.Currency,
            transactionId,
            payment.CompletedAt ?? DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        await publishEndpoint.Publish(message, context.CancellationToken);
        logger.LogInformation("Payment {PaymentId} completed for booking {BookingId}", payment.PaymentId, payment.BookingId);
    }
}

using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using ELKStack.Observability;
using ELKStack.PaymentService.State;
using MassTransit;

namespace ELKStack.PaymentService.Consumers;

public sealed class PaymentRequestedConsumer(
    PaymentState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<PaymentRequestedConsumer> logger) : IConsumer<PaymentRequested>
{
    public async Task Consume(ConsumeContext<PaymentRequested> context)
    {
        if (context.Message.Scenario == DemoScenario.PaymentFailure)
        {
            const string reason = "Payment provider rejected the card.";
            var failedPayment = state.MarkFailed(context.Message, reason);
            var failureMetadata = correlationContext.CreateMetadata();
            var failure = new PaymentFailed(
                failedPayment.PaymentId,
                failedPayment.BookingId,
                failedPayment.Amount,
                failedPayment.Currency,
                reason,
                DateTimeOffset.UtcNow,
                failureMetadata.EventId,
                failureMetadata.CorrelationId,
                failureMetadata.CausationId);

            await publishEndpoint.Publish(failure, context.CancellationToken);
            logger.LogForStage(
                configuration,
                LogLevel.Warning,
                "Payment failed",
                "Payment {PaymentId} failed for booking {BookingId}: {Reason}",
                failedPayment.PaymentId,
                failedPayment.BookingId,
                reason);
            return;
        }

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
        logger.LogForStage(
            configuration,
            LogLevel.Information,
            "Payment completed",
            "Payment {PaymentId} completed for booking {BookingId}",
            payment.PaymentId,
            payment.BookingId);
    }
}

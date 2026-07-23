using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using ELKStack.Observability;
using ELKStack.PaymentService.State;
using MassTransit;

namespace ELKStack.PaymentService.Consumers;

public sealed class BookingRequestedConsumer(
    PaymentState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<BookingRequestedConsumer> logger) : IConsumer<BookingRequested>
{
    public async Task Consume(ConsumeContext<BookingRequested> context)
    {
        var existing = state.GetByBookingId(context.Message.BookingId);

        var metadata = correlationContext.CreateMetadata();
        var message = new PaymentRequested(
            existing?.PaymentId ?? Guid.NewGuid(),
            context.Message.BookingId,
            context.Message.Amount,
            context.Message.Currency,
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId,
            context.Message.Scenario);

        state.MarkRequested(message);
        await publishEndpoint.Publish(message, context.CancellationToken);

        logger.LogForStage(
            configuration,
            LogLevel.Information,
            "Processing payment",
            "Payment requested for booking {BookingId}",
            context.Message.BookingId);
    }
}

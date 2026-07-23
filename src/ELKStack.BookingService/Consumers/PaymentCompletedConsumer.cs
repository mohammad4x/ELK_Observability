using ELKStack.BookingService.State;
using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using MassTransit;

namespace ELKStack.BookingService.Consumers;

public sealed class PaymentCompletedConsumer(
    BookingState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    ILogger<PaymentCompletedConsumer> logger) : IConsumer<PaymentCompleted>
{
    public async Task Consume(ConsumeContext<PaymentCompleted> context)
    {
        var booking = state.MarkPaid(context.Message);

        var metadata = correlationContext.CreateMetadata();
        var message = new BookingConfirmed(
            booking.BookingId,
            booking.PassengerName,
            booking.CustomerEmail,
            booking.Destination,
            booking.Amount,
            booking.Currency,
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        await publishEndpoint.Publish(message, context.CancellationToken);
        logger.LogInformation(
            "Booking {BookingId} confirmed after payment {PaymentId}",
            booking.BookingId,
            context.Message.PaymentId);
    }
}

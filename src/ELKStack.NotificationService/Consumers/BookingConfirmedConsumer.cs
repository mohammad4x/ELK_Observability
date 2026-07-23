using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using ELKStack.Observability;
using MassTransit;

namespace ELKStack.NotificationService.Consumers;

public sealed class BookingConfirmedConsumer(
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<BookingConfirmedConsumer> logger) : IConsumer<BookingConfirmed>
{
    public async Task Consume(ConsumeContext<BookingConfirmed> context)
    {
        var metadata = correlationContext.CreateMetadata();
        var message = new NotificationRequested(
            Guid.NewGuid(),
            context.Message.BookingId,
            context.Message.CustomerEmail,
            $"Booking confirmed for {context.Message.Destination}",
            $"Hi {context.Message.PassengerName}, your booking to {context.Message.Destination} is confirmed.",
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        await publishEndpoint.Publish(message, context.CancellationToken);
        logger.LogForStage(
            configuration,
            LogLevel.Information,
            "Notification requested",
            "Notification requested for booking {BookingId}",
            context.Message.BookingId);
    }
}

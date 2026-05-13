using ELKStack.BookingService.State;
using ELKStack.Contracts;
using MassTransit;

namespace ELKStack.BookingService.Consumers;

public sealed class NotificationSentConsumer(
    BookingState state,
    ILogger<NotificationSentConsumer> logger) : IConsumer<NotificationSent>
{
    public Task Consume(ConsumeContext<NotificationSent> context)
    {
        state.MarkNotificationSent(context.Message);
        logger.LogInformation("Booking {BookingId} notification sent to {Recipient}", context.Message.BookingId, context.Message.Recipient);

        return Task.CompletedTask;
    }
}

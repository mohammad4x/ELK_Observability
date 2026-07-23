using ELKStack.Contracts;
using ELKStack.NotificationService.State;
using ELKStack.Observability.Correlation;
using ELKStack.Observability;
using MassTransit;

namespace ELKStack.NotificationService.Consumers;

public sealed class NotificationRequestedConsumer(
    NotificationState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration,
    ILogger<NotificationRequestedConsumer> logger) : IConsumer<NotificationRequested>
{
    public async Task Consume(ConsumeContext<NotificationRequested> context)
    {
        var notification = state.MarkRequested(context.Message);

        var metadata = correlationContext.CreateMetadata();
        var message = new NotificationSent(
            notification.NotificationId,
            notification.BookingId,
            notification.Recipient,
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        state.MarkSent(message);
        await publishEndpoint.Publish(message, context.CancellationToken);

        logger.LogForStage(
            configuration,
            LogLevel.Information,
            "Notification sent",
            "Notification {NotificationId} sent to {Recipient}",
            notification.NotificationId,
            notification.Recipient);
    }
}

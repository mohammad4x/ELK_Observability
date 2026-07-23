using ELKStack.Contracts;
using ELKStack.NotificationService.Models;
using ELKStack.NotificationService.State;
using ELKStack.Observability.Correlation;
using MassTransit;

namespace ELKStack.NotificationService.Features.Notifications;

public static class CreateNotification
{
    public sealed record Request(Guid BookingId, string Recipient, string Subject, string Body);
    public sealed record Response(Guid NotificationId, Guid BookingId, string Recipient, string Subject, string Body, string Status, DateTimeOffset RequestedAt, DateTimeOffset? SentAt);

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", async (Request request, NotificationState state, ICorrelationContextAccessor correlationContext, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
        {
            var metadata = correlationContext.CreateMetadata();
            var message = new NotificationRequested(Guid.NewGuid(), request.BookingId, request.Recipient, request.Subject, request.Body, DateTimeOffset.UtcNow, metadata.EventId, metadata.CorrelationId, metadata.CausationId);
            var notification = state.MarkRequested(message);
            await publishEndpoint.Publish(message, cancellationToken);

            return Results.AcceptedAtRoute("GetNotificationById", new { notificationId = notification.NotificationId }, MapResponse(notification));
        }).WithName("CreateNotification");

    private static Response MapResponse(NotificationRecord notification) => new(notification.NotificationId, notification.BookingId, notification.Recipient, notification.Subject, notification.Body, notification.Status, notification.RequestedAt, notification.SentAt);
}

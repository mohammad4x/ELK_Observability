using ELKStack.NotificationService.Models;
using ELKStack.NotificationService.State;

namespace ELKStack.NotificationService.Features.Notifications;

public static class GetNotifications
{
    public sealed record Response(Guid NotificationId, Guid BookingId, string Recipient, string Subject, string Body, string Status, DateTimeOffset RequestedAt, DateTimeOffset? SentAt);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", (NotificationState state) => Results.Ok(state.All.Select(MapResponse)))
            .WithName("GetNotifications");

    private static Response MapResponse(NotificationRecord notification) => new(notification.NotificationId, notification.BookingId, notification.Recipient, notification.Subject, notification.Body, notification.Status, notification.RequestedAt, notification.SentAt);
}

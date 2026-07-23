namespace ELKStack.NotificationService.Features.Notifications;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications").WithTags("Notifications");

        GetNotifications.Map(group);
        GetNotificationById.Map(group);
        CreateNotification.Map(group);

        return endpoints;
    }
}

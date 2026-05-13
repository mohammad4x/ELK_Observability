using System.Collections.Concurrent;
using ELKStack.Contracts;
using ELKStack.NotificationService.Models;

namespace ELKStack.NotificationService.State;

public sealed class NotificationState
{
    private readonly ConcurrentDictionary<Guid, NotificationRecord> _notifications = new();

    public IReadOnlyCollection<NotificationRecord> All =>
        _notifications.Values.OrderByDescending(notification => notification.RequestedAt).ToArray();

    public NotificationRecord? Get(Guid notificationId) =>
        _notifications.GetValueOrDefault(notificationId);

    public NotificationRecord MarkRequested(NotificationRequested message)
    {
        return _notifications.AddOrUpdate(
            message.NotificationId,
            _ => new NotificationRecord(
                message.NotificationId,
                message.BookingId,
                message.Recipient,
                message.Subject,
                message.Body,
                "Requested",
                message.RequestedAt,
                null),
            (_, existing) => existing with
            {
                Status = "Requested",
                RequestedAt = message.RequestedAt
            });
    }

    public NotificationRecord MarkSent(NotificationSent message)
    {
        return _notifications.AddOrUpdate(
            message.NotificationId,
            _ => new NotificationRecord(
                message.NotificationId,
                message.BookingId,
                message.Recipient,
                "Unknown subject",
                "Unknown body",
                "Sent",
                message.SentAt,
                message.SentAt),
            (_, existing) => existing with
            {
                Status = "Sent",
                SentAt = message.SentAt
            });
    }
}

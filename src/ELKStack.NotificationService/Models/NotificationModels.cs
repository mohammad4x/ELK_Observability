namespace ELKStack.NotificationService.Models;

public sealed record CreateNotificationRequest(
    Guid BookingId,
    string Recipient,
    string Subject,
    string Body);

public sealed record NotificationRecord(
    Guid NotificationId,
    Guid BookingId,
    string Recipient,
    string Subject,
    string Body,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? SentAt);

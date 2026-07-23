namespace ELKStack.BookingService.Models;

public sealed record BookingRecord(
    Guid BookingId,
    string PassengerName,
    string CustomerEmail,
    string Destination,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? NotificationSentAt,
    DateTimeOffset? FailedAt = null);

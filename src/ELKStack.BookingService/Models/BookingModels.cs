namespace ELKStack.BookingService.Models;

public sealed record CreateBookingRequest(
    string PassengerName,
    string CustomerEmail,
    string Destination,
    decimal Amount,
    string Currency = "USD");

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
    DateTimeOffset? NotificationSentAt);

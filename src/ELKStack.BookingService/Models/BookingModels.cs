namespace ELKStack.BookingService.Models;

using ELKStack.Contracts;

public sealed record CreateBookingRequest(
    string PassengerName,
    string CustomerEmail,
    string Destination,
    decimal Amount,
    string Currency = "USD",
    DemoScenario Scenario = DemoScenario.Normal);

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

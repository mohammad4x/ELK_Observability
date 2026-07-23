using System.Collections.Concurrent;
using ELKStack.BookingService.Models;
using ELKStack.Contracts;

namespace ELKStack.BookingService.State;

public sealed class BookingState
{
    private readonly ConcurrentDictionary<Guid, BookingRecord> _bookings = new();

    public IReadOnlyCollection<BookingRecord> All =>
        _bookings.Values.OrderByDescending(booking => booking.CreatedAt).ToArray();

    public BookingRecord? Get(Guid bookingId) =>
        _bookings.GetValueOrDefault(bookingId);

    public BookingRecord Add(BookingRequested message)
    {
        var booking = new BookingRecord(
            message.BookingId,
            message.PassengerName,
            message.CustomerEmail,
            message.Destination,
            message.Amount,
            message.Currency,
            "PaymentPending",
            message.RequestedAt,
            null,
            null,
            null);

        _bookings[message.BookingId] = booking;
        return booking;
    }

    public BookingRecord MarkPaid(PaymentCompleted message)
    {
        return _bookings.AddOrUpdate(
            message.BookingId,
            _ => new BookingRecord(
                message.BookingId,
                "Unknown passenger",
                "unknown@example.com",
                "Unknown destination",
                message.Amount,
                message.Currency,
                "Paid",
                message.CompletedAt,
                message.CompletedAt,
                message.CompletedAt,
                null),
            (_, existing) => existing with
            {
                Status = "Paid",
                PaidAt = message.CompletedAt,
                ConfirmedAt = message.CompletedAt
            });
    }

    public BookingRecord MarkNotificationSent(NotificationSent message)
    {
        return _bookings.AddOrUpdate(
            message.BookingId,
            _ => new BookingRecord(
                message.BookingId,
                "Unknown passenger",
                message.Recipient,
                "Unknown destination",
                0,
                "USD",
                "NotificationSent",
                message.SentAt,
                null,
                null,
                message.SentAt),
            (_, existing) => existing with
            {
                Status = "NotificationSent",
                NotificationSentAt = message.SentAt
            });
    }

    public BookingRecord MarkPaymentFailed(PaymentFailed message)
    {
        return _bookings.AddOrUpdate(
            message.BookingId,
            _ => new BookingRecord(
                message.BookingId,
                "Unknown passenger",
                "unknown@example.com",
                "Unknown destination",
                message.Amount,
                message.Currency,
                "PaymentFailed",
                message.FailedAt,
                null,
                null,
                null,
                message.FailedAt),
            (_, existing) => existing with
            {
                Status = "PaymentFailed",
                FailedAt = message.FailedAt
            });
    }
}

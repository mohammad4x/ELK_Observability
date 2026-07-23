using ELKStack.BookingService.Models;
using ELKStack.BookingService.State;

namespace ELKStack.BookingService.Features.Bookings;

public static class GetBookingById
{
    public sealed record Response(
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
        DateTimeOffset? FailedAt);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{bookingId:guid}", (Guid bookingId, BookingState state) =>
        {
            var booking = state.Get(bookingId);
            return booking is null ? Results.NotFound() : Results.Ok(MapResponse(booking));
        }).WithName("GetBookingById");

    public static Response MapResponse(BookingRecord booking) => new(
        booking.BookingId,
        booking.PassengerName,
        booking.CustomerEmail,
        booking.Destination,
        booking.Amount,
        booking.Currency,
        booking.Status,
        booking.CreatedAt,
        booking.PaidAt,
        booking.ConfirmedAt,
        booking.NotificationSentAt,
        booking.FailedAt);
}

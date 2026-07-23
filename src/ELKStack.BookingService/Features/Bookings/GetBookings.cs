using ELKStack.BookingService.Models;
using ELKStack.BookingService.State;

namespace ELKStack.BookingService.Features.Bookings;

public static class GetBookings
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
        group.MapGet("", (BookingState state) =>
            Results.Ok(state.All.Select(MapResponse)))
            .WithName("GetBookings");

    private static Response MapResponse(BookingRecord booking) => new(
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

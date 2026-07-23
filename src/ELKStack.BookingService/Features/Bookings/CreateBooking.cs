using ELKStack.BookingService.State;
using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using MassTransit;

namespace ELKStack.BookingService.Features.Bookings;

public static class CreateBooking
{
    public sealed record Request(
        string PassengerName,
        string CustomerEmail,
        string Destination,
        decimal Amount,
        string Currency = "USD",
        DemoScenario Scenario = DemoScenario.Normal);

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
        group.MapPost("", async (
            Request request,
            BookingState state,
            ICorrelationContextAccessor correlationContext,
            IPublishEndpoint publishEndpoint,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            if (request.Amount <= 0)
            {
                return Results.BadRequest("Amount must be greater than zero.");
            }

            var metadata = correlationContext.CreateMetadata();
            var message = new BookingRequested(
                Guid.NewGuid(),
                request.PassengerName,
                request.CustomerEmail,
                request.Destination,
                request.Amount,
                string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.ToUpperInvariant(),
                DateTimeOffset.UtcNow,
                metadata.EventId,
                metadata.CorrelationId,
                metadata.CausationId,
                request.Scenario);

            var booking = state.Add(message);
            await publishEndpoint.Publish(message, cancellationToken);
            loggerFactory.CreateLogger("Bookings").LogInformation(
                "Booking {BookingId} requested for {Destination}",
                booking.BookingId,
                booking.Destination);

            return Results.AcceptedAtRoute(
                "GetBookingById",
                new { bookingId = booking.BookingId },
                MapResponse(booking));
        }).WithName("CreateBooking");

    private static Response MapResponse(Models.BookingRecord booking) => new(
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

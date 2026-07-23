namespace ELKStack.BookingService.Features.Bookings;

public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/bookings").WithTags("Bookings");

        GetBookings.Map(group);
        GetBookingById.Map(group);
        CreateBooking.Map(group);

        return endpoints;
    }
}

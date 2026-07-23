using ELKStack.PaymentService.Models;
using ELKStack.PaymentService.State;

namespace ELKStack.PaymentService.Features.Payments;

public static class GetPayments
{
    public sealed record Response(Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Status, DateTimeOffset RequestedAt, DateTimeOffset? CompletedAt, string? TransactionId, string? FailureReason);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", (PaymentState state) => Results.Ok(state.All.Select(MapResponse)))
            .WithName("GetPayments");

    private static Response MapResponse(PaymentRecord payment) => new(payment.PaymentId, payment.BookingId, payment.Amount, payment.Currency, payment.Status, payment.RequestedAt, payment.CompletedAt, payment.TransactionId, payment.FailureReason);
}

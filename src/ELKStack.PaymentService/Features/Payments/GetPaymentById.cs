using ELKStack.PaymentService.Models;
using ELKStack.PaymentService.State;

namespace ELKStack.PaymentService.Features.Payments;

public static class GetPaymentById
{
    public sealed record Response(Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Status, DateTimeOffset RequestedAt, DateTimeOffset? CompletedAt, string? TransactionId, string? FailureReason);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{paymentId:guid}", (Guid paymentId, PaymentState state) =>
        {
            var payment = state.Get(paymentId);
            return payment is null ? Results.NotFound() : Results.Ok(MapResponse(payment));
        }).WithName("GetPaymentById");

    public static Response MapResponse(PaymentRecord payment) => new(payment.PaymentId, payment.BookingId, payment.Amount, payment.Currency, payment.Status, payment.RequestedAt, payment.CompletedAt, payment.TransactionId, payment.FailureReason);
}

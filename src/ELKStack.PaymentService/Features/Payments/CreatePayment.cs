using ELKStack.Observability.Correlation;
using ELKStack.PaymentService.Models;
using ELKStack.PaymentService.State;
using ELKStack.Contracts;
using MassTransit;

namespace ELKStack.PaymentService.Features.Payments;

public static class CreatePayment
{
    public sealed record Request(Guid BookingId, decimal Amount, string Currency = "USD");
    public sealed record Response(Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Status, DateTimeOffset RequestedAt, DateTimeOffset? CompletedAt, string? TransactionId, string? FailureReason);

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", async (Request request, PaymentState state, ICorrelationContextAccessor correlationContext, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
        {
            if (request.Amount <= 0)
            {
                return Results.BadRequest("Amount must be greater than zero.");
            }

            var metadata = correlationContext.CreateMetadata();
            var message = new PaymentRequested(Guid.NewGuid(), request.BookingId, request.Amount, string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.ToUpperInvariant(), DateTimeOffset.UtcNow, metadata.EventId, metadata.CorrelationId, metadata.CausationId);
            var payment = state.MarkRequested(message);
            await publishEndpoint.Publish(message, cancellationToken);

            return Results.AcceptedAtRoute("GetPaymentById", new { paymentId = payment.PaymentId }, MapResponse(payment));
        }).WithName("CreatePayment");

    private static Response MapResponse(PaymentRecord payment) => new(payment.PaymentId, payment.BookingId, payment.Amount, payment.Currency, payment.Status, payment.RequestedAt, payment.CompletedAt, payment.TransactionId, payment.FailureReason);
}

using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using ELKStack.PaymentService.Models;
using ELKStack.PaymentService.State;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ELKStack.PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController(
    PaymentState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<PaymentRecord>> GetAll() =>
        Ok(state.All);

    [HttpGet("{paymentId:guid}")]
    public ActionResult<PaymentRecord> GetById(Guid paymentId)
    {
        var payment = state.Get(paymentId);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentRecord>> Create(
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        var metadata = correlationContext.CreateMetadata();
        var message = new PaymentRequested(
            Guid.NewGuid(),
            request.BookingId,
            request.Amount,
            string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.ToUpperInvariant(),
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        var payment = state.MarkRequested(message);
        await publishEndpoint.Publish(message, cancellationToken);

        return AcceptedAtAction(nameof(GetById), new { paymentId = payment.PaymentId }, payment);
    }
}

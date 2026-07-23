using ELKStack.BookingService.Models;
using ELKStack.BookingService.State;
using ELKStack.Contracts;
using ELKStack.Observability.Correlation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ELKStack.BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController(
    BookingState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint,
    ILogger<BookingsController> logger) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<BookingRecord>> GetAll() =>
        Ok(state.All);

    [HttpGet("{bookingId:guid}")]
    public ActionResult<BookingRecord> GetById(Guid bookingId)
    {
        var booking = state.Get(bookingId);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<BookingRecord>> Create(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
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
        logger.LogInformation(
            "Booking {BookingId} requested for {Destination}",
            booking.BookingId,
            booking.Destination);

        return AcceptedAtAction(nameof(GetById), new { bookingId = booking.BookingId }, booking);
    }
}

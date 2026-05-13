using ELKStack.Contracts;
using ELKStack.NotificationService.Models;
using ELKStack.NotificationService.State;
using ELKStack.Observability.Correlation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ELKStack.NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(
    NotificationState state,
    ICorrelationContextAccessor correlationContext,
    IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<NotificationRecord>> GetAll() =>
        Ok(state.All);

    [HttpGet("{notificationId:guid}")]
    public ActionResult<NotificationRecord> GetById(Guid notificationId)
    {
        var notification = state.Get(notificationId);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationRecord>> Create(
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var metadata = correlationContext.CreateMetadata();
        var message = new NotificationRequested(
            Guid.NewGuid(),
            request.BookingId,
            request.Recipient,
            request.Subject,
            request.Body,
            DateTimeOffset.UtcNow,
            metadata.EventId,
            metadata.CorrelationId,
            metadata.CausationId);

        var notification = state.MarkRequested(message);
        await publishEndpoint.Publish(message, cancellationToken);

        return AcceptedAtAction(nameof(GetById), new { notificationId = notification.NotificationId }, notification);
    }
}

namespace ELKStack.Contracts;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
}

public enum DemoScenario
{
    Normal,
    PaymentFailure,
    SlowPayment,
    NotificationFailure,
    MissingTracePropagation,
    PaymentRetryThenSuccess
}

public sealed record IntegrationEventMetadata(
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId);

public sealed record BookingRequested(
    Guid BookingId,
    string PassengerName,
    string CustomerEmail,
    string Destination,
    decimal Amount,
    string Currency,
    DateTimeOffset RequestedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId,
    DemoScenario Scenario = DemoScenario.Normal) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => RequestedAt;
}

public sealed record PaymentRequested(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    DateTimeOffset RequestedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId,
    DemoScenario Scenario = DemoScenario.Normal) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => RequestedAt;
}

public sealed record PaymentCompleted(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string TransactionId,
    DateTimeOffset CompletedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => CompletedAt;
}

public sealed record PaymentFailed(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string Reason,
    DateTimeOffset FailedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId,
    DemoScenario Scenario = DemoScenario.PaymentFailure) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => FailedAt;
}

public sealed record BookingConfirmed(
    Guid BookingId,
    string PassengerName,
    string CustomerEmail,
    string Destination,
    decimal Amount,
    string Currency,
    DateTimeOffset ConfirmedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => ConfirmedAt;
}

public sealed record NotificationRequested(
    Guid NotificationId,
    Guid BookingId,
    string Recipient,
    string Subject,
    string Body,
    DateTimeOffset RequestedAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => RequestedAt;
}

public sealed record NotificationSent(
    Guid NotificationId,
    Guid BookingId,
    string Recipient,
    DateTimeOffset SentAt,
    Guid EventId,
    Guid CorrelationId,
    Guid? CausationId) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt => SentAt;
}

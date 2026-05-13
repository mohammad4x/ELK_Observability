using ELKStack.Contracts;

namespace ELKStack.Observability.Correlation;

public sealed record CorrelationContext(
    Guid OperationId,
    Guid CorrelationId,
    Guid? CausationId)
{
    public static CorrelationContext CreateRoot(Guid? correlationId = null) =>
        new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), null);

    public static CorrelationContext CreateForHttp(Guid correlationId, Guid? causationId) =>
        new(Guid.NewGuid(), correlationId, causationId);

    public static CorrelationContext CreateForConsumedEvent(IIntegrationEvent message) =>
        new(Guid.NewGuid(), message.CorrelationId, message.EventId);
}

using ELKStack.Contracts;

namespace ELKStack.Observability.Correlation;

public interface ICorrelationContextAccessor
{
    CorrelationContext Context { get; set; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
    IntegrationEventMetadata CreateMetadata();
}

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private CorrelationContext? _context;

    public CorrelationContext Context
    {
        get => _context ??= CorrelationContext.CreateRoot();
        set => _context = value;
    }

    public Guid CorrelationId => Context.CorrelationId;

    public Guid? CausationId => Context.CausationId;

    public IntegrationEventMetadata CreateMetadata() =>
        new(Guid.NewGuid(), Context.CorrelationId, Context.CausationId);
}

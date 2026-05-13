namespace ELKStack.Observability.Correlation;

public sealed class CorrelationDelegatingHandler(
    ICorrelationContextAccessor correlationAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = correlationAccessor.Context;

        request.Headers.TryAddWithoutValidation(CorrelationHeaders.CorrelationId, context.CorrelationId.ToString());
        request.Headers.TryAddWithoutValidation(CorrelationHeaders.CausationId, context.OperationId.ToString());
        request.Headers.TryAddWithoutValidation(CorrelationHeaders.RequestId, Guid.NewGuid().ToString());

        return base.SendAsync(request, cancellationToken);
    }
}

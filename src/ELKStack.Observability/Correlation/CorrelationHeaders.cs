namespace ELKStack.Observability.Correlation;

public static class CorrelationHeaders
{
    public const string CorrelationId = "X-Correlation-ID";
    public const string CausationId = "X-Causation-ID";
    public const string RequestId = "X-Request-ID";

    public const string CorrelationIdKey = "CorrelationId";
    public const string CausationIdKey = "CausationId";
    public const string OperationIdKey = "OperationId";
}

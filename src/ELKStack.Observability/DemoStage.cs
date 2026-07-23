using Microsoft.Extensions.Configuration;

namespace ELKStack.Observability;

/// <summary>
/// Selects the deliberately limited runtime behaviour used by course stages.
/// The reference application has no Demo:Stage value and therefore runs with
/// the complete observability implementation.
/// </summary>
public static class DemoStage
{
    public const string Opaque = "Opaque";
    public const string StructuredLogging = "StructuredLogging";
    public const string Correlation = "Correlation";
    public const string Tracing = "Tracing";
    public const string Metrics = "Metrics";
    public const string Pipeline = "Pipeline";
    public const string Complete = "Complete";

    public static bool IsOpaque(IConfiguration configuration) =>
        string.Equals(configuration["Demo:Stage"], Opaque, StringComparison.OrdinalIgnoreCase);

    public static bool UsesCorrelation(IConfiguration configuration) =>
        !IsOpaque(configuration) &&
        !string.Equals(configuration["Demo:Stage"], StructuredLogging, StringComparison.OrdinalIgnoreCase);

    public static bool UsesOpenTelemetry(IConfiguration configuration) =>
        string.Equals(configuration["Demo:Stage"], Tracing, StringComparison.OrdinalIgnoreCase)
        || string.Equals(configuration["Demo:Stage"], Metrics, StringComparison.OrdinalIgnoreCase)
        || string.Equals(configuration["Demo:Stage"], Pipeline, StringComparison.OrdinalIgnoreCase)
        || string.Equals(configuration["Demo:Stage"], Complete, StringComparison.OrdinalIgnoreCase)
        || string.IsNullOrWhiteSpace(configuration["Demo:Stage"]);
}

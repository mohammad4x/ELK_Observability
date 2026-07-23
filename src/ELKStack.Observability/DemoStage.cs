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

    public static bool IsOpaque(IConfiguration configuration) =>
        string.Equals(configuration["Demo:Stage"], Opaque, StringComparison.OrdinalIgnoreCase);
}

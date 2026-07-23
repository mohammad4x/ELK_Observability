using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ELKStack.Observability;

public static class DemoLoggingExtensions
{
    public static void LogForStage(
        this ILogger logger,
        IConfiguration configuration,
        LogLevel level,
        string opaqueMessage,
        string structuredMessage,
        params object?[] properties)
    {
        if (DemoStage.IsOpaque(configuration))
        {
            logger.Log(level, opaqueMessage);
            return;
        }

        logger.Log(level, structuredMessage, properties);
    }
}

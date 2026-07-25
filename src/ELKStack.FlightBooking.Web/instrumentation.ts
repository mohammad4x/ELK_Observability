import { diag, DiagConsoleLogger, DiagLogLevel } from "@opentelemetry/api";

export async function register() {
  if (process.env.NEXT_RUNTIME !== "nodejs") {
    return;
  }

  if (process.env.OTEL_DIAGNOSTIC_LOGGING === "true") {
    diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.INFO);
  }

  const { registerOtel } = await import("./lib/telemetry");
  registerOtel();
}

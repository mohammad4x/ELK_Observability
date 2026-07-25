import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { getNodeAutoInstrumentations } from "@opentelemetry/auto-instrumentations-node";
import { resourceFromAttributes } from "@opentelemetry/resources";
import { NodeSDK } from "@opentelemetry/sdk-node";
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from "@opentelemetry/semantic-conventions";

let started = false;

export function registerOtel() {
  if (started) {
    return;
  }

  const endpoint = process.env.OTEL_EXPORTER_OTLP_TRACES_ENDPOINT;
  const sdk = new NodeSDK({
    resource: resourceFromAttributes({
      [ATTR_SERVICE_NAME]: "elkstack-flight-booking-web",
      [ATTR_SERVICE_VERSION]: process.env.npm_package_version ?? "0.1.0",
    }),
    traceExporter: endpoint ? new OTLPTraceExporter({ url: endpoint }) : undefined,
    instrumentations: [getNodeAutoInstrumentations()],
  });

  sdk.start();
  started = true;
}

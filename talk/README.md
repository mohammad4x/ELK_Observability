# 30-minute observability talk

<p align="right"><a href="README.fa.md"><strong>فارسی</strong></a></p>

Use this as speaker notes, not slides. Keep the first 26 minutes conceptual;
use the final four minutes for the running system.

## 1. The fundamental problem

Start with: “A user tried to make a booking. Something went wrong. What
happened?” Monitoring answers expected questions—availability, error rate,
queue depth, latency thresholds. Observability supports the unexpected
question: why did this operation fail and what path did it take?

## 2. Logs, metrics, and traces

| Signal | Why it exists | Best question | Limitation alone |
| --- | --- | --- | --- |
| Logs | Detailed events, decisions, exceptions | What did code report? | No execution graph |
| Metrics | Cheap aggregate measurements | Is something changing system-wide? | Cannot explain one request |
| Traces | Timed parent/child operation graph | Which hop was slow or failed? | Needs a trace or symptom to start |

Say: **metrics find the smoke; traces map the path; logs explain the local
decision.** `CorrelationId` adds business workflow membership; `CausationId`
adds event-to-event cause.

## 3. Structured logs and Serilog

```csharp
// Bad: the identifier is trapped inside prose.
logger.LogInformation($"Payment {paymentId} failed");

// Good: named, typed, searchable properties.
logger.LogWarning(
    "Payment {PaymentId} failed for booking {BookingId}: {Reason}",
    paymentId, bookingId, reason);
```

In .NET, business code depends on `ILogger<T>` and `ILoggerFactory` routes
records to providers. ASP.NET Core wires logging and dependency injection
through `WebApplication.CreateBuilder`. Serilog is the provider used here for
structured events, `LogContext` enrichment, sinks, and request logging; the
application code remains on the standard `ILogger<T>` abstraction.

## 4. OpenTelemetry: vendor-neutral by design

OpenTelemetry is an open standard and ecosystem of APIs, SDKs,
instrumentation, collectors, semantic conventions, and OTLP transport. It is
not a storage or visualization backend.

```text
instrumentation → OTel SDK → OTLP → Collector → chosen backend
```

This is the anti-lock-in boundary: application instrumentation can remain OTel
while the backend changes. Dashboards, queries, alerts, retention, and cost
models are still backend-specific. In .NET, OTel builds on `Activity`,
`ActivitySource`, and `Meter`; an `Activity` is conceptually a span.

## 5. Zero-code agents vs code-based instrumentation

| Approach | Best for | Trade-off |
| --- | --- | --- |
| Zero-code agent / auto-instrumentation | Fast baseline for HTTP, DB, messaging, runtime libraries, legacy services | Cannot normally infer business intent |
| Code-based instrumentation | Provider calls, domain failures, business metrics and attributes | Needs deliberate design and ownership |

Use both: automatic instrumentation for framework boundaries, code for
business questions. OTel zero-code mechanisms vary by runtime—Java bytecode
agent, Python monkey patching, .NET startup hooks/profiling, or eBPF—and are
configured via environment/startup options. [Official OTel docs](https://opentelemetry.io/docs/zero-code/)

## 6. One standard across ecosystems

| Stack | Automatic instrumentation | Code API |
| --- | --- | --- |
| ASP.NET Core | ASP.NET Core, HttpClient, runtime, libraries | `ActivitySource`, `Meter`, OTel .NET SDK |
| Java / Spring Boot | Java agent: servlet/Spring/JDBC and libraries | OTel Java API/SDK |
| Python / Django | `opentelemetry-instrument`: Django and libraries | OTel Python API/SDK |

The portable concepts are resource attributes, trace-context propagation,
semantic conventions, OTLP, and vendor-neutral instrumentation.

## 7. Tool landscape

| Signal | Common open-source-oriented choices | Unified/commercial choices |
| --- | --- | --- |
| Metrics | Prometheus, Grafana Mimir, VictoriaMetrics | Datadog, New Relic, Elastic |
| Logs | Elasticsearch/Elastic, Grafana Loki, OpenSearch | Splunk, Datadog, New Relic |
| Traces | Jaeger, Grafana Tempo, Zipkin | Elastic APM, Datadog, Honeycomb, New Relic |

These overlap; this is a map, not a procurement scorecard. See
[Prometheus](https://prometheus.io/docs/introduction/overview/),
[Tempo](https://grafana.com/docs/tempo/latest/), and
[Jaeger](https://www.jaegertracing.io/docs/).

## 8. Why Elastic Observability for this demo

Make a strong but scoped case:

1. **One investigation surface:** structured logs, traces, metrics, and
   business identifiers can be searched without teaching separate systems.
2. **Search fits the incident story:** `CorrelationId`, `EventId`,
   `CausationId`, booking ID, and message type are natural searchable fields.
3. **OTel remains the application contract:** Elastic accepts OTLP for logs,
   metrics, and traces, so this is not Elastic-only instrumentation.
4. **Guided APM experience:** service and transaction views are effective in a
   short live demo.
5. **Collector remains a neutral policy point:** batching, filtering,
   sampling, enrichment, and routing do not belong in application code.

Be candid: Elastic has deployment, cost/licensing, and backend-specific query
considerations. The case is faster investigation of this event-driven system
while preserving OpenTelemetry at the application boundary. [Elastic OTLP
intake](https://www.elastic.co/docs/solutions/observability/apm/opentelemetry-intake-api)

## 9. Live demo script

1. Open `ELKStack.slnx`; identify the services, contracts, observability, and defaults.
2. Run the AppHost.
3. Send `Normal`, then `PaymentFailure` bookings.
4. Start at the failed payment symptom.
5. Show `BookingId`, `PaymentId`, and `Reason` in structured logs.
6. Open the trace: HTTP → RabbitMQ → Payment, including timing.
7. Search `CorrelationId`; explain the `EventId` → `CausationId` chain.
8. Close with metric → trace → log → business workflow.

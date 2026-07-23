# 06 — The telemetry pipeline

<p align="right"><a href="README.fa.md"><strong>فارسی</strong></a></p>

## Question

What is responsible for producing, exporting, receiving, storing, and showing
the evidence we just investigated?

## Pipeline

```text
Application instrumentation
  → OpenTelemetry SDK
  → OTLP
  → OpenTelemetry Collector
  → Elastic APM Server
  → Elasticsearch
  → Kibana
```

## Responsibilities

| Component | Responsibility |
| --- | --- |
| Instrumentation | Produces framework or custom telemetry |
| OTel SDK | Configures resources, listeners, sampling, processors, exporters |
| OTLP | Vendor-neutral telemetry transport protocol |
| Collector | Receives, batches, processes, routes, and exports telemetry |
| APM Server | Elastic intake endpoint for APM telemetry |
| Elasticsearch | Stores and indexes the resulting data |
| Kibana | Searches, visualizes, and investigates it |

Automatic instrumentation observes framework boundaries. Manual instrumentation
adds meaningful domain operations such as a payment-provider call.

## Still unanswered

Technical traces are not always equivalent to business workflows. What
happens with retries, delayed messages, scheduled work, and workflows that
last days?

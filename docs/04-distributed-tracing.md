# 04 — Logs are not an execution graph

## Question

Which component handled this operation, in what order, and where did it spend
its time?

## Why correlation is insufficient

Thirty correlated log records are still a flat pile. They do not expose a
parent/child execution graph or reliable durations.

## New capability

Use .NET diagnostics and OpenTelemetry tracing:

```text
ActivitySource creates Activity instances
Activity.Current carries the current context
ActivityListener observes activities
OpenTelemetry SDK listens, samples, enriches, and exports them
Activity ≈ span
```

ASP.NET Core and `HttpClient` instrumentation create activities automatically.
MassTransit emits `MassTransit` activities; the shared setup subscribes to
that activity source.

## Observe

Inspect the trace generated from `POST /api/bookings`. Follow the HTTP server
span through publish/consume spans. The `MissingTracePropagation` comparison
is introduced when its scenario implementation is added in the final lesson.

## Still unanswered

A trace explains a known operation. In production, how do we discover that
the system is unhealthy before we know which trace to inspect?

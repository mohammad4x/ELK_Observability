# 00 — Observability by Investigation

## Start with the incident, not the tooling

Imagine a user says:

> “I tried to make a booking. Something went wrong. What happened?”

This is deliberately an incomplete report. We do not know the booking ID, the
service that failed, whether the request reached RabbitMQ, whether payment was
rejected, or whether the notification was lost afterwards.

The useful engineering question is not “Do we have logs?” or “Do we use
OpenTelemetry?” It is:

> Can we reconstruct enough evidence to answer this question reliably?

That is the theme of this workshop. Every stage starts with a question that the
previous stage cannot answer, then adds only the information needed to answer
the next question.

## Monitoring and observability

Monitoring answers questions we expected to ask before an incident occurred.
Those questions are usually represented by dashboards, health checks, alerts,
and service-level objectives.

```text
Is the API up?
Is the error rate above 2%?
Is CPU high?
Is the queue depth growing?
Did payment latency exceed the alert threshold?
```

Those are valuable questions. They tell us whether a known condition requires
attention.

Observability supports investigation when the important question is not known
in advance:

```text
Why did this particular booking fail?
Which services and messages handled it?
What happened after this RabbitMQ event was published?
Which component added four seconds of latency?
Why were only bookings for one destination affected?
```

Monitoring often identifies a symptom. Observability helps move from symptom
to explanation.

```text
alert: payment latency is rising
  → metric: the increase started ten minutes ago
    → trace: provider calls are consuming four seconds
      → log: provider timeout after four seconds
        → business metadata: these failures belong to one booking workflow
```

The boundary is not absolute: a mature system uses monitoring and observability
together. The important distinction is the kind of question being answered.

| | Monitoring | Observability |
| --- | --- | --- |
| Primary question | “Is a known condition healthy?” | “Why did this unexpected thing happen?” |
| Starting point | Alert, dashboard, threshold, SLO | Incident report, anomaly, support ticket, hypothesis |
| Typical resolution | Aggregate/system level | Individual operation and its context |
| Result | Detect and notify | Explain and diagnose |

## Observability is not a product or a formula

These statements are too narrow:

```text
Observability = ELK
Observability = OpenTelemetry
Observability = logs + metrics + traces
```

Elastic is an observability backend in this repository. OpenTelemetry is a
standard and SDK/tooling ecosystem for producing and exporting telemetry.
Logs, metrics, and traces are useful types of evidence. None of them alone—or
even their mere presence together—guarantees that a system is understandable.

For example, a system can produce thousands of logs but remain opaque if the
records omit identifiers, use unstable prose, expose no meaningful business
context, or cannot be searched across services. It can emit traces that are
technically valid but do not tell us which business workflow a retry belongs
to. It can publish metrics that detect an outage without explaining its cause.

Observability is the capability created when the system emits useful evidence,
preserves meaningful context, and makes that evidence available for
investigation.

## Different evidence, different resolution

The signals complement one another because they answer different kinds of
questions.

| Evidence | Best first question | Strength | Limitation |
| --- | --- | --- | --- |
| Metrics | “Is something changing system-wide?” | Cheap aggregate trends, rates, percentiles, alerts | Usually cannot explain one request by itself |
| Traces | “What path did this operation take, and where was time spent?” | Execution graph, parent/child relationships, duration | You usually need a trace or symptom to start with |
| Logs | “What did the code decide or report at this point?” | Detailed narrative, exceptions, domain fields | A flat list does not show execution structure |
| Business event metadata | “Which messages are part of this workflow, and what caused what?” | Durable workflow identity beyond one technical trace | Does not replace timing or execution topology |

This repository intentionally treats `CorrelationId`, `EventId`, and
`CausationId` as business evidence, not as substitutes for tracing.

```text
TraceId       technical execution graph for a bounded operation
CorrelationId membership in a business workflow
EventId       identity of one event/message
CausationId   the earlier event that caused this event
```

A workflow may contain retries, delayed messages, scheduled jobs, or human
approval that happens hours later. In those cases, one `CorrelationId` can be
more durable than a single distributed trace.

## What “observable enough” means here

For the booking workflow, the system becomes progressively more useful when we
can answer these questions without relying on timing, intuition, or manually
stitching together console windows:

1. Did the booking succeed or fail?
2. Which booking and payment did the evidence describe?
3. Which records and messages belong to this user operation?
4. Which event caused the next event?
5. Which service handled each part, and how long did it take?
6. Is this isolated or a system-wide pattern?
7. What concrete evidence supports the root-cause conclusion?

The workshop does not add telemetry because “three pillars” says it should. It
adds telemetry only when the current evidence fails to answer one of those
questions.

## The .NET view

.NET already provides the building blocks for much of this story:

- `ILogger<T>` for application and framework log records;
- `System.Diagnostics.Activity` for operation context and spans;
- `ActivitySource` for manually named business/technical operations;
- built-in ASP.NET Core and `HttpClient` instrumentation points;
- `Meter`, counters, and histograms for measurements.

OpenTelemetry builds on those diagnostics APIs. It configures listeners,
enrichment, sampling, resources, processors, and exporters; it does not invent
a separate execution-context model for .NET applications.

## Frontend relevance

This is a backend-focused workshop, but a browser or Next.js server can be the
start of the same investigation:

```text
Browser / Next.js
  → trace context
    → .NET Booking API
      → RabbitMQ
        → Payment and Notification workers
```

The frontend does not need to become a telemetry backend. Its useful role is
to preserve request context across its own boundaries so an engineer can follow
the user experience into the backend workflow.

## How to present the course

Use each stage as a small production investigation:

```text
run the system
  → observe an unanswered question
    → inspect the available evidence
      → identify its limitation
        → open the next course solution
          → gain one new investigative capability
```

The next lesson begins with the deliberately opaque system. Its plain messages
say that a payment failed, but cannot tell us whose payment it was. That
limitation creates the need for queryable log records.

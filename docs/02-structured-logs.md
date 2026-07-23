# 02 — Logs we can actually query

## Before structured logs: what is a log?

A log is evidence emitted while software runs. It records something that
happened at a point in time: a request entered, a message was consumed, a
payment provider rejected a card, or an exception was thrown.

A useful log record usually contains more than its visible sentence:

```text
timestamp       when it happened
level           Debug / Information / Warning / Error / Critical
message         human-readable event description
properties      named data attached to that event
exception       error details, when applicable
category         logger/source name
context         service, request, trace, correlation, environment
```

The console may render a log record as one line, but an observability backend
can store its fields independently. This is the difference between reading a
diary and querying evidence.

## Logging in .NET

Application code normally depends on the `Microsoft.Extensions.Logging`
abstraction, not a specific logging library:

```csharp
public sealed class PaymentRequestedConsumer(
    ILogger<PaymentRequestedConsumer> logger)
{
    // ...
}
```

`ILogger<T>` gives the record a category—usually the fully qualified type
name. The application asks an `ILoggerFactory` for loggers. The factory sends
records to one or more registered providers.

```text
application code
  → ILogger<T>
  → ILoggerFactory
  → registered providers
      → console
      → OpenTelemetry / OTLP
      → file, Elastic, or another backend
```

This separation matters: business code says *what happened*; composition code
chooses *where evidence goes*.

## Why ASP.NET Core makes this easy

`WebApplication.CreateBuilder(args)` creates a host builder with configuration,
dependency injection, and logging already connected. Controllers, middleware,
hosted services, and MassTransit consumers can request `ILogger<T>` through
constructor injection; they do not construct loggers manually.

ASP.NET Core and its framework libraries also emit useful records themselves:

- hosting startup/shutdown and environment details;
- request processing and unhandled exceptions;
- routing, authentication, and framework warnings;
- health checks and HTTP-client behaviour when enabled.

The `Logging:LogLevel` settings in each service's `appsettings.json` control
which levels are emitted by default and by framework category. This repository
keeps Microsoft framework noise at `Warning` while preserving application and
MassTransit evidence at useful levels.

## Where Serilog fits

Serilog is a logging implementation, not a replacement for the .NET logging
abstraction. In the complete profile,
`ELKStack.ServiceDefaults.ConfigureSerilog` registers Serilog as the host
logger and configures:

- console output for the live demo;
- enrichment from `LogContext`;
- service name and environment fields;
- OTLP export through `Serilog.Sinks.OpenTelemetry` when an endpoint exists.

`UseSerilogRequestLogging` adds one completion record for each HTTP request,
including the response status and duration. The correlation middleware places
business context into the logging scope, and Serilog's `Enrich.FromLogContext()`
copies that context into the exported record.

```text
HTTP request
  → Correlation middleware creates a logging scope
  → controller/consumer writes ILogger records
  → Serilog enriches and exports the records
```

Serilog is useful here because it has mature structured-event handling,
enrichment, and sinks. The services still inject `ILogger<T>`, so their code is
not coupled to Serilog APIs.

## Question

We know a payment failed. Can we find its evidence without parsing prose or
guessing which line belongs to it?

## Why the previous stage fails

`Payment failed` is readable but carries no stable searchable data. If wording
changes, a text search changes with it. It cannot reliably join the payment to
a booking or an outcome.

## New capability

Record named properties rather than interpolated strings:

```csharp
logger.LogWarning(
    "Payment {PaymentId} failed for booking {BookingId}: {Reason}",
    paymentId, bookingId, reason);
```

`PaymentId`, `BookingId`, and `Reason` become fields that Kibana can filter,
aggregate, and display without extracting values from text.

### Log text vs log properties

Avoid building the message with interpolation:

```csharp
logger.LogInformation($"Processing booking {booking.Id}");
```

The rendered text looks fine, but `booking.Id` is now part of a sentence.
Prefer a message template:

```csharp
logger.LogInformation("Processing booking {BookingId}", booking.Id);
```

The template remains readable, and `BookingId` remains a typed property. Use
stable, domain-meaningful property names; do not put secrets, access tokens,
or full payment-card data into logs.

## Observe

Search for `BookingId` or `PaymentId`. Compare it with an interpolated log
message whose identifiers exist only inside the rendered sentence.

## Still unanswered

Structured logs identify records, but separate services still emit separate
evidence. Which records belong to the same business operation?

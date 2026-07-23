# 02 — Logs we can actually query

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

## Observe

Search for `BookingId` or `PaymentId`. Compare it with an interpolated log
message whose identifiers exist only inside the rendered sentence.

## Still unanswered

Structured logs identify records, but separate services still emit separate
evidence. Which records belong to the same business operation?

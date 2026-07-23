# 01 — Opaque system: something failed. Where?

## The question

A user reports that their booking failed. The system prints evidence such as:

```text
Booking requested
Processing payment
Payment failed
Booking requested
Processing payment
Payment completed
```

Which booking failed? Which message caused it? Did the failure happen in the
request, the payment worker, or notification delivery?

We cannot answer reliably. Reading interleaved process logs and guessing is
not an investigation strategy.

## What this stage intentionally withholds

- No searchable business fields in log records.
- No cross-service business correlation.
- No trace graph or duration evidence.
- No aggregate symptom signal.

The stage is intentionally incomplete; that is the point of the demo.

## Run it

1. Open `01-Opaque-System.slnx` and run the opaque AppHost.
2. Copy the Booking Service HTTP endpoint from the Aspire dashboard into
   [`requests.http`](requests.http).
3. Send the successful and failing requests close together.
4. Read the interleaved console output. It tells you that a payment failed,
   but not whose payment it was or how it relates to the other booking.

## What comes next

The next lesson makes log records queryable through named properties such as
`BookingId`, `PaymentId`, and `Outcome`. It still will not tell us which
records belong to one workflow across service boundaries.

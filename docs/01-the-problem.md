# 01 — The problem

## Question

“A user says their booking failed. What happened?”

## Why the system cannot answer yet

Plain text log messages do not provide stable fields to search, and separate
processes emit unrelated streams. Two bookings can produce identical,
interleaved messages.

## Observe

Run a successful and a `PaymentFailure` booking close together. Try to
identify the failed booking without relying on timing or guesswork.

## Leads to the next lesson

We need event data recorded as fields, not embedded in prose.

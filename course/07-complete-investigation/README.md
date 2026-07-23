# 07 — Complete investigation: technical execution and business causation

<p align="right"><a href="README.fa.md"><strong>فارسی</strong></a></p>

## Question

Why did this booking fail, what evidence supports that conclusion, and which
customer workflow was affected?

## The model

```text
TraceId       technical execution graph and timeline
CorrelationId business workflow membership
EventId       identity of one event/message
CausationId   previous event that caused this event
```

These identifiers are related but not interchangeable. A retry, delayed
message, human approval, or next-day scheduled step may continue the same
business workflow after the original distributed trace naturally ends.

## Investigation flow

```text
Metric: payment failures rise
  → filter failing payment traces
    → inspect the provider span duration/error
      → read structured log properties
        → follow CorrelationId and CausationId through the business workflow
```

## Scenarios

`Normal` and `PaymentFailure` are implemented. The course contract reserves
the following next scenarios for the final investigation work:

- `SlowPayment`
- `NotificationFailure`
- `MissingTracePropagation`
- `PaymentRetryThenSuccess`

The final lesson compares the technical trace graph with the business event
causation graph and explains why keeping both is valuable.

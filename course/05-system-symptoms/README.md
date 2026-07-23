# 05 — How did we know there was a problem?

<p align="right"><a href="README.fa.md"><strong>فارسی</strong></a></p>

## Question

No customer supplied a TraceId. How do we detect worsening payment behaviour
across the whole system?

## Why tracing is insufficient

Traces provide high-resolution evidence for one operation. They are not the
right first view for request rate, failure rate, queue pressure, or latency
percentiles.

## New capability

Use aggregate measurements:

```text
request rate       Is work arriving?
failure count      Is it succeeding?
duration histogram Is it slowing down?
queue depth        Is work backing up?
```

Framework metrics cover HTTP and runtime behaviour. The workshop will add
business measurements such as payment outcome and provider duration.

## Observe

The current profile exposes framework HTTP/runtime metrics. The next scenario
increment adds `SlowPayment` plus a business payment-duration histogram; then
the live investigation begins with the rising metric, selects a slow trace,
and ends at the provider log.

## Still unanswered

Where are these signals produced, transformed, transported, stored, and
queried? OpenTelemetry is the answer to that pipeline question—not the
definition of observability.

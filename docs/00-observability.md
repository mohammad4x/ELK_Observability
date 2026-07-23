# Observability by Investigation

Observability is the ability to understand a system’s behaviour from the
evidence it emits, including questions we did not predefine on a dashboard.

Monitoring answers expected questions. Observability supports investigation:

```text
Metric: payment latency is rising
  → Trace: PaymentService spent four seconds in the provider operation
    → Log: provider timeout for one payment attempt
      → Business metadata: the attempt belongs to this booking workflow
```

The signals have different resolutions. None of them alone is observability.

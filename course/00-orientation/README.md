# 00 — Orientation: known questions vs investigation

## The question

Monitoring is excellent when we already know what to ask:

```text
Is the API healthy?
What is the error rate?
Is latency above the alert threshold?
```

But an engineer often receives a different report:

> “I tried to make a booking. Something went wrong. What happened?”

That is an investigation. We do not yet know which service, message, request,
or failure mode to inspect.

## The workshop model

Each lesson answers one question that the preceding lesson cannot answer.

```text
symptom → evidence → limitation → next capability
```

Logs, metrics, traces, OpenTelemetry, and Elastic are not the definition of
observability. They are evidence and transport mechanisms that make specific
questions answerable.

## Next

Open `../01-opaque-system/01-Opaque-System.slnx`. Run two bookings and observe why plain log
messages are not enough to identify a single failed operation.

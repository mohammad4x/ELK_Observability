# 03 — Following one business operation

<p align="right"><a href="README.fa.md"><strong>فارسی</strong></a></p>

## Question

A booking crosses HTTP, RabbitMQ, three services, and six events. Which log
records belong to the same user operation?

## Why structured logs are insufficient

You can search by `BookingId` when every record happens to contain it, but
that is not a workflow identity. A later event may only have a payment or
notification identifier.

## New capability

Propagate business metadata with every integration event:

```text
CorrelationId  workflow membership
EventId        one event instance
CausationId    event that caused this event
```

All workflow records share a `CorrelationId`. Each child event sets
`CausationId` to its parent event's `EventId`.

## Code focus

- `CorrelationMiddleware` establishes HTTP correlation.
- `CorrelationPublishFilter` writes business metadata to MassTransit.
- `CorrelationConsumeFilter` restores it for each consumer.

## Observe

Search one `CorrelationId` across Booking, Payment, and Notification services.
Then reconstruct the event chain using `EventId` and `CausationId`.

## Still unanswered

Correlation gives membership, not execution structure. It cannot show span
parentage, parallelism, or where the time went.

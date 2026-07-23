# Observability by Investigation

<p align="right">
  <a href="README.fa.md"><strong>فارسی</strong></a>
</p>

This directory turns the repository into a progressive, code-driven workshop.
Each stage begins with a production question, runs a deliberately limited
system, and adds only the capability needed to answer the next question.

| Stage | Question | Capability introduced |
| --- | --- | --- |
| [00 — Orientation](00-orientation/README.md) | What makes an unknown production question different from a health check? | An investigation model |
| [01 — Opaque system](01-opaque-system/README.md) | A booking failed. Which booking, and what happened? | Deliberately insufficient evidence and `PaymentFailure` |
| 02 — Queryable logs | Can we search the evidence reliably? | Structured logging |
| 03 — Business operation | Which records belong to the same booking? | Correlation and causation |
| 04 — Execution structure | Which component did work, and for how long? | Distributed tracing |
| 05 — System symptoms | How did we know there was a problem? | Metrics |
| 06 — Telemetry pipeline | How did the telemetry reach the backend? | OpenTelemetry pipeline |
| 07 — Complete investigation | Why did this booking fail? | Signals and business workflow together |

Each stage keeps its `.slnx` file beside the lesson guide, so opening a course
folder reveals both the presentation notes and the runnable solution.

## Code layout

Every stage owns its AppHost source in its own folder. This is the code to show
when explaining what the stage enables: its RabbitMQ/Elastic infrastructure and
the `Demo__Stage` profile assigned to each service.

```text
course/NN-stage/
  NN-Stage.slnx
  README.md
  ELKStack.Course.*.AppHost/
    AppHost.cs                 # stage-specific composition and profile
    *.csproj

src/
  ELKStack.Contracts/          # shared business events and metadata
  ELKStack.Observability/      # shared logging/correlation/tracing mechanics
  ELKStack.*Service/           # shared booking/payment/notification workflow
```

The `src/` projects are intentionally common code. Each course solution adds
the relevant shared projects alongside its local AppHost, making the stage a
complete runnable system without copying three services for every lesson.
